using System.Collections;
using System.Collections.Generic;
using Modules.Scenario;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.GS_Auth
{
    public class GSAuthView : MonoBehaviour
    {
        public GameObject root;
        public TMP_InputField activationInput;
        public TextMeshProUGUI deviceIdTxt;
        public Button activationSubmit;
        public Button closeActivation;
        public Button buyLicence;

        [Header("Key Panel")]
        public List<GroupTxt> keyInfos;
        
        [Header("Error Form")] 
        public GameObject errorRoot;
        public TextMeshProUGUI errorTxt;
        private Coroutine _errorCor;
        
        public void ShowError(string val)
        {
            if(_errorCor != null)
                StopCoroutine(_errorCor);
            if(!errorTxt.text.Contains(val))
                errorTxt.text += val + "\n";
            _errorCor = StartCoroutine(ShowErrorCor());
        }

        private IEnumerator ShowErrorCor()
        {
            errorRoot.SetActive(true);
            yield return new WaitForSeconds(5.0f);
            errorRoot.SetActive(false);
            errorTxt.text = "";
        }
        
        public void SetKeyInfo(List<GSAuthController.Activation> activations)
        {
            CheckGroupTxts(activations.Count);

            for(var i = 0; i < activations.Count; i++)
            {
                keyInfos[i].txts[0].text = activations[i].key;
                keyInfos[i].txts[1].text = activations[i].organization;
                keyInfos[i].txts[2].text = activations[i].expirationDate;
                keyInfos[i].txts[3].text = activations[i].licenceName;
                keyInfos[i].txts[4].text = string.Join(", ",activations[i].libraryCaptures);
                keyInfos[i].gameObject.SetActive(true);
            }
        }
        
        private void CheckGroupTxts(int requiredSize)
        {
            var currentSize = keyInfos.Count;
            if (requiredSize > currentSize)
            {
                var parent = keyInfos[0].transform.parent;
                var obj = keyInfos[0].gameObject;
            
                for (var i = 0; i < requiredSize - currentSize; i++)
                {
                    keyInfos.Add(Instantiate(obj, parent).GetComponent<GroupTxt>());
                }
            }
        
            foreach (var keyInfo in keyInfos)
            {
                keyInfo.gameObject.SetActive(false);
            }
        }
    }
}
