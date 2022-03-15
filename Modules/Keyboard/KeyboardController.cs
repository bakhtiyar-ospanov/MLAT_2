using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Modules.Keyboard
{
    public class KeyboardController : MonoBehaviour
    {
        public string writtenText = "";
        public bool shifted = false;

        [SerializeField] TextMeshProUGUI writtentTextField;
        [SerializeField] GameObject keyboardPanel;

        [SerializeField] TextMeshProUGUI langText;
        [SerializeField] GameObject ruKeyboard;
        [SerializeField] GameObject engKeyboard;

        TMP_InputField toWriteIntoTMP;
        InputField toWriteInto;
        private Transform _vrCamera;

        private void Awake()
        {
            keyboardPanel.SetActive(false);
            
            if(XRSettings.enabled)
                _vrCamera = GameManager.Instance.starterController.GetFPСVR().GetCamera().transform;
        }

        public void AddString(string str)
        {
            writtenText += str;
            shifted = false;
        }

        private void Update()
        {
            writtentTextField.text = writtenText;
            if (toWriteInto)
                toWriteInto.text = writtenText;
            if (toWriteIntoTMP)
                toWriteIntoTMP.text = writtenText;
            if(toWriteIntoTMP != null && !toWriteIntoTMP.gameObject.activeInHierarchy)
                CloseButton();
        }

        public void ShiftPressed()
        {
            shifted = !shifted;
        }

        public void SpacePressed()
        {
            writtenText += " ";
        }
        
        public void EnterPressed()
        {
            writtenText += "\n";
        }

        public void CloseButton()
        {
            writtentTextField.gameObject.SetActive(false);
            keyboardPanel.SetActive(false);
            toWriteInto = null;
            toWriteIntoTMP = null;
        }

        public void OpenKeyboard()
        {
            toWriteInto = null;
            toWriteIntoTMP = null;
            CloseButton();
            writtentTextField.gameObject.SetActive(true);
            keyboardPanel.SetActive(true);
            writtenText = "";
        }

        public void OpenKeyboard(TMP_InputField tmpInput)
        {
            Debug.Log("OpenKeyboard");
            toWriteInto = null;
            toWriteIntoTMP = null;
            writtentTextField.gameObject.SetActive(true);
            keyboardPanel.SetActive(true);
            writtenText = "";
            toWriteIntoTMP = tmpInput;
            MoveCanvas();
        }

        public void OpenKeyboard(InputField input)
        {
            toWriteInto = null;
            toWriteIntoTMP = null;
            writtentTextField.gameObject.SetActive(true);
            keyboardPanel.SetActive(true);
            writtenText = "";
            toWriteInto = input;
        }

        public void BackspaceButton()
        {
            if(writtenText.Length == 0) return;
            writtenText = writtenText.Remove(writtenText.Length-1);
        }

        public void SubmitPressed()
        {
            if(toWriteInto)
                ExecuteEvents.Execute(toWriteInto.gameObject, null, ExecuteEvents.submitHandler);
            else if (toWriteIntoTMP)
                ExecuteEvents.Execute(toWriteIntoTMP.gameObject, null, ExecuteEvents.submitHandler);
            //CloseButton();
        }

        public void LangugeButton()
        {
            if (langText.text == "RU")
            {
                ruKeyboard.SetActive(true);
                engKeyboard.SetActive(false);
                langText.text = "ENG";
            }
            else if (langText.text == "ENG")
            {
                ruKeyboard.SetActive(false);
                engKeyboard.SetActive(true);
                langText.text = "RU";
            }
        }
        
        private void MoveCanvas()
        {
            var canvasTransform = transform;
            canvasTransform.position = _vrCamera.position + _vrCamera.forward * 0.5f - new Vector3(0.0f, 0.35f, 0.0f);
            canvasTransform.rotation = Quaternion.LookRotation(canvasTransform.position - _vrCamera.position);
        }
    }
}
