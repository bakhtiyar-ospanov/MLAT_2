using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cursor = Modules.Starter.Cursor;

namespace Modules.AssetMenu
{
    public class AssetMenuRadialView : MonoBehaviour
    {
        public GameManager.Product product;
        public GameObject canvas;
        public GameObject root;
        
        [SerializeField] private List<TxtButton> txtButtons;
        [SerializeField] private Button btnBack;
        [SerializeField] private Button btnPrev;
        [SerializeField] private Button btnNext;
        public Button closeArea;

        private Canvas _canvas;
        private int _currentBtnCount;
        private (bool, bool, bool) _backPrevNextState;
        private bool _isAnimInProgress;
        private int _maxElements = 6;
        private int _startIndex;
        private int _endIndex;
        private List<(int, int)> _pageIntervals = new List<(int, int)>();
        private int _currentInterval;
        private Vector3 MousePosition { set; get; }

        private void Awake()
        {
            btnNext.onClick.AddListener(SetupNextButtons);
            btnPrev.onClick.AddListener(SetupPrevButtons);
            _canvas = canvas.GetComponent<Canvas>();
            canvas.SetActive(false);
            root.SetActive(false);
        }

        public void SetValues(List<string> ids, List<string> texts, List<UnityAction> calls)
        {
            _currentBtnCount = calls.Count;
            _backPrevNextState.Item1 = false;
            _backPrevNextState.Item2 = false;
            _backPrevNextState.Item3 = false;
        
            var localCalls = new List<UnityAction>(calls);
            var localText = new List<string>(texts);
            var localIds = new List<string>(ids);
        
            for (var i = 0; i < _currentBtnCount; ++i)
            {
                if (localIds[i] != "Back") continue;
                btnBack.name = localIds[i];
                btnBack.onClick.RemoveAllListeners();
                btnBack.onClick.AddListener(localCalls[i]);
                _backPrevNextState.Item1 = true;
                localCalls.RemoveAt(i);
                localText.RemoveAt(i);
                localIds.RemoveAt(i);
                --_currentBtnCount;
                break;
            }
        
            CheckButtons(_currentBtnCount);
        
            for (var i = 0; i < _currentBtnCount; ++i)
            {
                var id = localIds[i];
                txtButtons[i].tmpText.text = SpliceText(localText[i], 40);
                txtButtons[i].name = id;
                txtButtons[i].button.onClick.RemoveAllListeners();
                txtButtons[i].button.onClick.AddListener(localCalls[i]);
            }

            _startIndex = 0;
            _endIndex = _currentBtnCount;
            
            if (_currentBtnCount > _maxElements)
                SplitMenu();

            Reorder();
        }
        
        private static string SpliceText(string text, int lineLength) {
            if(string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
                return "";
            var words = text.Split(' ');
            var part = string.Empty;
            var splited = string.Empty;
            foreach (var word in words)
            {
                if (part.Length + word.Length < lineLength)
                {
                    part += string.IsNullOrEmpty(part) ? word : " " + word;
                }
                else
                {
                    splited += part + "\n";
                    part = word;
                }
            }
            splited += part + "\n";
            return splited;
        }

        private void SplitMenu()
        {
            _endIndex = _maxElements;
            _backPrevNextState.Item3 = true;
            _currentInterval = 0;
            _pageIntervals.Clear();

            var i = _maxElements;
            while (i < _currentBtnCount)
            {
                _pageIntervals.Add((i - _maxElements, i));
                i += _maxElements;
            }
            _pageIntervals.Add((i - _maxElements, _currentBtnCount));
        }

        private void SetupNextButtons()
        {
            if(_isAnimInProgress)
                return;
            ;
            if(_pageIntervals.Count > _currentInterval + 1)
                _currentInterval++;
            
            _startIndex = _pageIntervals[_currentInterval].Item1;
            _endIndex = _pageIntervals[_currentInterval].Item2;

            if(_pageIntervals.Count == _currentInterval + 1)
                _backPrevNextState.Item3 = false;
            
            _backPrevNextState.Item2 = true;
            Reorder();
            StopAllCoroutines();
            StartCoroutine(SequentialAppear());
        }
        
        private void SetupPrevButtons()
        {
            if(_isAnimInProgress)
                return;
            
            _backPrevNextState.Item2 = true;
            
            if(-1 < _currentInterval - 1)
                _currentInterval--;

            _startIndex = _pageIntervals[_currentInterval].Item1;
            _endIndex = _pageIntervals[_currentInterval].Item2;

            if(-1 == _currentInterval - 1)
                _backPrevNextState.Item2 = false;
            
            _backPrevNextState.Item3 = true;
            StopAllCoroutines();
            StartCoroutine(SequentialAppear());
        }

        private void Reorder()
        {
            MousePosition = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f);
            var localCurrentButtonCount = (_endIndex - _startIndex);
            var radius = Mathf.Clamp(40.0f * localCurrentButtonCount, 160.0f, 400.0f) * _canvas.scaleFactor;
            var sectionCount = localCurrentButtonCount % 2 == 0 ? localCurrentButtonCount : localCurrentButtonCount + 1;
            var j = 0;
            for (var i = _startIndex; i < _endIndex; i++)
            {
                var angle = j * Mathf.PI*2f / sectionCount;
                if (localCurrentButtonCount == 5 && j == 4)
                {
                    angle = Mathf.PI * 3f / 2f;
                } else if (localCurrentButtonCount == 7 && j == 6)
                {
                    angle = (j+1) * Mathf.PI * 2f / sectionCount;
                }
                
                var newPos = MousePosition + new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius, 0.0f);

                var pivotX = Math.Abs(angle - Mathf.PI/2.0f) < 0.01f ? 0.5f : 
                    Math.Abs(angle - 3.0f*Mathf.PI/2.0f) < 0.01f ? 0.5f :
                    angle < Mathf.PI / 2.0f || angle > 3.0f*Mathf.PI/2.0f ? 0.0f : 1.0f;
                var pivotY = 0.5f;
            
                txtButtons[i].GetComponent<RectTransform>().pivot = new Vector2(pivotX, pivotY);
                txtButtons[i].transform.position = newPos;
                ++j;
            }

            btnBack.transform.parent.position = MousePosition;
        }
        

        private void CheckButtons(int requiredSize)
        {
            var currentSize = txtButtons.Count;
            if (requiredSize > currentSize)
            {
                var parent = txtButtons[0].transform.parent;
                var obj = txtButtons[0].gameObject;
            
                for (var i = 0; i < requiredSize - currentSize; i++)
                {
                    txtButtons.Add(Instantiate(obj, parent).GetComponent<TxtButton>());
                    txtButtons.Last().button.interactable = true;
                }
            }
        
            foreach (var txtButton in txtButtons)
            {
                txtButton.gameObject.SetActive(false);
            }
        }

        public void SetActivePanel(bool val)
        {
            canvas.SetActive(val);
            root.SetActive(val);
            Reorder();
            if (val)
                StartCoroutine(SequentialAppear());
            if(!val)
                Cursor.ActivateCursor(false);
        }

        public IEnumerator SequentialAppear()
        {
            if(_isAnimInProgress) yield break;
            _isAnimInProgress = true;
        
            btnBack.gameObject.SetActive(_backPrevNextState.Item1);
            btnPrev.gameObject.SetActive(_backPrevNextState.Item2);
            btnNext.gameObject.SetActive(_backPrevNextState.Item3);

            foreach (var txtButton in txtButtons)
            {
                txtButton.gameObject.SetActive(false);
            }
        
            for (var i = _startIndex; i < _endIndex; ++i)
            {
                yield return new WaitForSeconds(0.06f);
                txtButtons[i].gameObject.SetActive(true);
            }

            _isAnimInProgress = false;
        }
    }
}
