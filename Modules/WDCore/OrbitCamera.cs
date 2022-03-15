using System;
using System.Collections;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.EventSystems;

public class OrbitCamera : MonoBehaviour
{  
    public bool autoRotateOn;
    public bool autoRotateReverse;
    [HideInInspector]
    public int autoRotateReverseValue = 1;
    public float autoRotateSpeed = 0.1f;
    public bool cameraCollision;
    public bool clickToRotate = true;
    public float collisionRadius = 1.0f;
    public float dampeningX = 0.9f;
    public float dampeningY = 0.9f;
    public float distance = 10f;
    private RaycastHit hit;
    public float initialAngleX;
    public float initialAngleY;
    public bool invertAxisX;
    public bool invertAxisY;
    public bool invertAxisZoom;
    [HideInInspector]
    public int invertXValue = 1;
    [HideInInspector]
    public int invertYValue = 1;
    [HideInInspector]
    public int invertZoomValue = 1;
    public string kbPanAxisX = "Horizontal";
    public string kbPanAxisY = "Vertical";
    public bool kbUseZoomAxis;
    public string kbZoomAxisName = string.Empty;
    public bool keyboardControl;
    public bool leftClickToRotate = true;
    public bool limitX;
    public bool limitY = true;
    public float maxDistance = 25f;
    public float maxSpinSpeed = 3f;
    public float minDistance = 5f;
    public string mouseAxisX = "Mouse X";
    public string mouseAxisY = "Mouse Y";
    public string mouseAxisZoom = "Mouse ScrollWheel";
    public bool mouseControl;
    private Vector3 position;
    private Ray ray;
    public bool rightClickToRotate;
    public float smoothingZoom = 0.1f;
    public string spinAxis = string.Empty;
    public bool SpinEnabled;
    public KeyCode spinKey;// = KeyCode.LeftControl;
    private bool spinning;
    private float spinSpeed;
    public bool spinUseAxis;
    public Transform target;
    public float targetDistance = 10f;
    private float x;
    public float xLimitOffset;
    public float xMaxLimit = 60f;
    public float xMinLimit = -60f;
    public float xSpeed = 1f;
    public float xMoveSpeed = 1f;
    private float xVelocity;
    private float y;
    public float yLimitOffset;
    public float yMaxLimit = 60f;
    public float yMinLimit = -60f;
    public float ySpeed = 1f;
    public float yMoveSpeed = 1f;
    private float yVelocity;
    public KeyCode zoomInKey = KeyCode.R;
    public KeyCode zoomOutKey = KeyCode.F;
    public float zoomSpeed = 5f;
    public float zoomVelocity;
    public bool touchControl;
    private Camera _cam;
    private Vector3 _targetPosition;
    private Vector3 _targetRotation;
    private bool isToFocus;
    private bool isDragging;
    private bool isDragStartedOverUI;
    private bool isBlock;
    private bool isMoving;
    public Action<bool> onMovingChanged;

    private Coroutine inputCoroutine;
    private void Awake()
    {
        _cam = GetComponent<Camera>();
        
        touchControl = true;
        mouseControl = true;
        
        SetTargetPositionAndRotation(target);
    }

    private void Start()
    {
        this.targetDistance = this.distance;

        if (this.invertAxisX)
        {
            this.invertXValue = -1;
        }
        else
        {
            this.invertXValue = 1;
        }
        if (this.invertAxisY)
        {
            this.invertYValue = -1;
        }
        else
        {
            this.invertYValue = 1;
        }
        if (this.invertAxisZoom)
        {
            this.invertZoomValue = -1;
        }
        else
        {
            this.invertZoomValue = 1;
        }
        if (this.autoRotateOn)
        {
            this.autoRotateReverseValue = -1;
        }
        else
        {
            this.autoRotateReverseValue = 1;
        }
      
        this.x = this.initialAngleX;
        this.y = this.initialAngleY;
        this.transform.Rotate(new Vector3(0f, this.initialAngleX, 0f), Space.World);
        this.transform.Rotate(new Vector3(this.initialAngleY, 0f, 0f), Space.Self);
        this.position = ((Vector3)(this.transform.rotation * new Vector3(0f, 0f, -this.distance))) + this.target.position;
    }

    private void OnEnable()
    {
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);
        inputCoroutine = StartCoroutine(InputListener());
    }

    public void Update()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            isDragStartedOverUI = false;
            isDragging = false;
        }
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !EventSystem.current.IsPointerOverGameObject())
            isDragging = true;
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && EventSystem.current.IsPointerOverGameObject())
            isDragStartedOverUI = true;

        if (EventSystem.current == null || EventSystem.current != null
            && (EventSystem.current.IsPointerOverGameObject() && !isDragging) || isDragStartedOverUI)
            isBlock = true;
        else
            isBlock = false;

        if (this.target != null)
        {
            var isZoomInAllowed = CameraCollision();

            if (this.autoRotateOn)
            {
                this.xVelocity += (this.autoRotateSpeed * this.autoRotateReverseValue) * Time.deltaTime;
            }

            if (this.touchControl)
            {
                if (Input.touchCount > 0)
                {
                    foreach (var touch in Input.touches)
                    {
                        if (touch.phase == TouchPhase.Began)
                        {
                            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                            {
                                isDragStartedOverUI = true;
                                break;
                            }
                            else
                            {
                                isDragStartedOverUI = false;
                            }
                        }
                    }

                    if (isDragStartedOverUI == true) return;

                    if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
                    {
                        var pointerX = Input.touches[0].deltaPosition.x;
                        var pointerY = Input.touches[0].deltaPosition.y;
                        this.xVelocity += (pointerX * 0.05f) * this.invertXValue;
                        this.yVelocity -= (pointerY * 0.05f) * this.invertYValue;
                        this.spinning = false;
                    }
                    if (Input.touchCount >= 2 && !EventSystem.current.IsPointerOverGameObject())
                    {
                        var touchZero = Input.GetTouch(0);
                        var touchOne = Input.GetTouch(1);

                        var touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                        var touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                        var touchZeroNormalized = (touchZero.position - touchZeroPrevPos).normalized;
                        var touchOneNormalized = (touchOne.position - touchOnePrevPos).normalized;
                        
                        if (GetTouchDirection(touchZeroNormalized).Item1 == GetTouchDirection(touchOneNormalized).Item1 &&
                            GetTouchDirection(touchZeroNormalized).Item2 == GetTouchDirection(touchOneNormalized).Item2)
                        {
                            target.LookAt(transform);
                            target.Translate(new Vector3(-(touchZero.deltaPosition.x) * -this.invertXValue * 0.01f * 0.5f, 0, 0), Space.Self);
                            target.Translate(new Vector3(0, (touchZero.deltaPosition.y) * -this.invertYValue * 0.01f * 0.5f, 0), Space.Self);
                        }                       
                        else
                        {
                            var prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                            var currentMagnitude = (touchZero.position - touchOne.position).magnitude;
                            var difference = currentMagnitude - prevMagnitude;

                            _cam.fieldOfView += -difference * 0.1f;
                            _cam.fieldOfView = Mathf.Clamp(_cam.fieldOfView, 5.0f, 110.0f);
                        }
                    }
                }
            }
            if (this.mouseControl)
            {
                if ((!this.clickToRotate || (this.leftClickToRotate && Input.GetMouseButton(0) && !isBlock)) || (this.rightClickToRotate && Input.GetMouseButton(0) && !isBlock))
                {
                    this.xVelocity += (Input.GetAxis(this.mouseAxisX) * this.xSpeed) * this.invertXValue;
                    this.yVelocity -= (Input.GetAxis(this.mouseAxisY) * this.ySpeed) * this.invertYValue;
                    this.spinning = false;
                }

                if (Input.GetMouseButton(1) && !isBlock)
                {
                    target.LookAt(transform);
                    xMoveSpeed = distance * 3.5f;
                    yMoveSpeed = distance * 3.5f;
                    target.Translate(new Vector3(-(Input.GetAxis(this.mouseAxisX) * this.xMoveSpeed) * -this.invertXValue * 0.01f, 0, 0), Space.Self);
                    target.Translate(new Vector3(0, (Input.GetAxis(this.mouseAxisY) * this.yMoveSpeed) * -this.invertYValue * 0.01f, 0), Space.Self);
                }

            }

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var zoomAxis = Input.GetAxis(this.mouseAxisZoom);
                if(zoomAxis > 0.0f && isZoomInAllowed || zoomAxis <= 0.0f)
                    this.zoomVelocity -= (zoomAxis * this.zoomSpeed) * this.invertZoomValue;
            }
                
        }
        if (this.keyboardControl)
        {
            if ((Input.GetAxis(this.kbPanAxisX) != 0f) || (Input.GetAxis(this.kbPanAxisY) != 0f))
            {
                this.xVelocity -= (Input.GetAxisRaw(this.kbPanAxisX) * (this.xSpeed / 2f)) * this.invertXValue;
                this.yVelocity += (Input.GetAxisRaw(this.kbPanAxisY) * (this.ySpeed / 2f)) * this.invertYValue;
                this.spinning = false;
            }
            if (this.kbUseZoomAxis)
            {
                this.zoomVelocity += (Input.GetAxis(this.kbZoomAxisName) * (this.zoomSpeed / 10f)) * this.invertXValue;
            }
            if (Input.GetKey(this.zoomInKey))
            {
                this.zoomVelocity -= (this.zoomSpeed / 10f) * this.invertZoomValue;
            }
            if (Input.GetKey(this.zoomOutKey))
            {
                this.zoomVelocity += (this.zoomSpeed / 10f) * this.invertZoomValue;
            }
        }
        if (this.SpinEnabled && ((this.mouseControl && this.clickToRotate) || this.keyboardControl))
        {
            if ((this.spinUseAxis && (Input.GetAxis(this.spinAxis) != 0f)) || (!this.spinUseAxis && Input.GetKey(this.spinKey)))
            {
                this.spinning = true;
                this.spinSpeed = Mathf.Min(this.xVelocity, this.maxSpinSpeed);
            }
            if (this.spinning)
            {
                this.xVelocity = this.spinSpeed;
            }
        }
        if (this.limitX)
        {
            if ((this.x + this.xVelocity) < (this.xMinLimit + this.xLimitOffset))
            {
                this.xVelocity = (this.xMinLimit + this.xLimitOffset) - this.x;
            }
            else if ((this.x + this.xVelocity) > (this.xMaxLimit + this.xLimitOffset))
            {
                this.xVelocity = (this.xMaxLimit + this.xLimitOffset) - this.x;
            }
            this.x += this.xVelocity;
            this.transform.Rotate(new Vector3(0f, this.xVelocity, 0f), Space.World);
        }
        else
        {
            this.transform.Rotate(new Vector3(0f, this.xVelocity, 0f), Space.World);
        }
        if (this.limitY)
        {
            if ((this.y + this.yVelocity) < (this.yMinLimit + this.yLimitOffset))
            {
                this.yVelocity = (this.yMinLimit + this.yLimitOffset) - this.y;
            }
            else if ((this.y + this.yVelocity) > (this.yMaxLimit + this.yLimitOffset))
            {
                this.yVelocity = (this.yMaxLimit + this.yLimitOffset) - this.y;
            }
            this.y += this.yVelocity;
            this.transform.Rotate(new Vector3(this.yVelocity, 0f, 0f), Space.Self);
        }
        else
        {
            this.transform.Rotate(new Vector3(this.yVelocity, 0f, 0f), Space.Self);
        }
        if ((this.targetDistance + this.zoomVelocity) < this.minDistance)
        {
            this.zoomVelocity = this.minDistance - this.targetDistance;
        }
        else if ((this.targetDistance + this.zoomVelocity) > this.maxDistance)
        {
            this.zoomVelocity = this.maxDistance - this.targetDistance;
        }
        this.targetDistance += this.zoomVelocity;
        this.distance = Mathf.Lerp(this.distance, this.targetDistance, this.smoothingZoom);
        this.transform.position = ((Vector3)(this.transform.rotation * new Vector3(0f, 0f, -this.distance))) + this.target.position;

        if (!this.SpinEnabled || !this.spinning)
        {
            this.xVelocity *= this.dampeningX;
        }
        this.yVelocity *= this.dampeningY;
        
        if (targetDistance < minDistance + 0.05f)
        {
            var speed = Mathf.Clamp(Math.Abs(zoomVelocity)*2.5f, 0.01f, 0.1f);
            target.LookAt(transform);
            target.Translate(new Vector3(0, 0, -speed), Space.Self);
            targetDistance = minDistance + 0.05f;
        }

        if ((Math.Abs(this.xVelocity) > 0.0001f || Math.Abs(this.yVelocity) > 0.0001f || isDragging || 
             Math.Abs(targetDistance - distance) > 0.05f) && !isMoving)
        {
            isMoving = true;
            onMovingChanged?.Invoke(true);
        }
        else if(Math.Abs(this.xVelocity) < 0.0001f && Math.Abs(this.yVelocity) < 0.0001f 
                                                   && !isDragging && Math.Abs(targetDistance - distance) < 0.05f && isMoving)
        {
            isMoving = false;
            onMovingChanged?.Invoke(false);
        }


        var distanceToTarget = Vector3.Distance(target.position, _targetPosition);
        if (isToFocus && distanceToTarget > 0.01f)
        {
            var speed = Mathf.Clamp(distanceToTarget*3.0f, 1.2f, 10.0f);
            target.position = Vector3.MoveTowards(target.position, _targetPosition, Time.deltaTime * speed);
            target.eulerAngles = Vector3.MoveTowards(target.eulerAngles, _targetRotation, Time.deltaTime * speed);
            transform.LookAt(target);
            UpdateYLimitReference();

            if (distanceToTarget < distance)
            {
                distance = Vector3.Distance(transform.position, target.position);
                targetDistance = distance;
            }
        }
        else
        {
            isToFocus = false;
        }
        
        this.zoomVelocity = 0f;
        this.zoomSpeed = this.distance / 2.0f;
        
    }
    
    private IEnumerator InputListener() 
    {
        while(true)
        {
            if(!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
                yield return ClickEvent(Input.mousePosition);
    
            yield return null;
        }
    }
    
    private IEnumerator ClickEvent(Vector3 mousePosition)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        var count = 0f;
        while(count < 0.30f)
        {
            if(Input.GetMouseButtonUp(0))
            {
                FocusOnObject();
                yield break;
            }
            count += Time.deltaTime;
            yield return null; 
        }
        
        GameManager.Instance.assetRaycastManager.OneClick(mousePosition);
    }
    
    private void FocusOnObject()
    {
        var ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 100.0f) && hit.transform != null)
        {
            _targetPosition = hit.point;
            isToFocus = true;
        }
    }

    public void SetTargetPositionAndRotation(Transform newTarget, bool isImmediately = false)
    {
        if (isImmediately)
            target.SetPositionAndRotation(newTarget.position, newTarget.rotation);
        
        _targetPosition = newTarget.position;
        _targetRotation = newTarget.eulerAngles;
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private (Direction, Direction) GetTouchDirection(Vector2 normalizedPos)
    {
        var directionX = normalizedPos.x > 0 ? Direction.Right : Direction.Left;
        var directionY = normalizedPos.y > 0 ? Direction.Down : Direction.Up;
        return (directionX, directionY);
    }

    public void ResetCam()
    {
        this.y = 0;
        this.x = 0;

        target.eulerAngles = Vector3.zero;
        transform.eulerAngles = Vector3.zero;

        target.position = Vector3.zero;
        transform.position = Vector3.zero;
    }

    private bool CameraCollision()
    {
        // if (Physics.SphereCast(this.transform.position, this.collisionRadius, this.transform.forward, out this.hit, 0.3f))
        // {
        //     var dst = Vector3.Distance(this.transform.position, this.hit.point);
        //     if (dst < 0.3f)
        //         return false;
        // }
        //
        // var hitColliders = new Collider[1];
        // var size = Physics.OverlapSphereNonAlloc(this.transform.position, this.collisionRadius, hitColliders);
        //
        // if (size > 0)
        //     return false;
        
        return true;
    }

    public void UpdateYLimitReference()
    {
        var xVal = transform.localEulerAngles.x;
        xVal = xVal > 180.0f ? xVal - 360.0f : xVal;
        this.y = xVal;
    }
}