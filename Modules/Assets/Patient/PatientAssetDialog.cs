using System.Collections;
using System.Linq;
using Modules.WDCore;
using Modules.SpeechKit;
using UnityEngine;
using UnityEngine.Events;

namespace Modules.Assets.Patient
{
    public partial class PatientAsset
    {
        public bool isDialogInProgress;
        private AudioSource _audioSource;
        
        public void GeneralTalk()
        {
            if(isDialogInProgress || !TextToSpeech.Instance.IsFinishedSpeaking()) return;
            
            var checkTable = GameManager.Instance.checkTableController.GetCheckTable();
            var replicas = checkTable.actions.Where(x => !string.IsNullOrEmpty(x.nameButton)).Select(y => (y.id, y.nameButton, y.speech, y.answer)).ToList();
            var actionIds = replicas.Select(x => x.id).ToList();
            actionIds.Add("Back");
            var actions = replicas.Select(patientDialog => 
                (UnityAction) (() =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    DialogWithPatient(patientDialog.id, patientDialog.speech,
                        patientDialog.answer);
                })).ToList();
            actions.Add(() => GameManager.Instance.assetMenuController.Init(this));
            var actionNames = replicas.Select(x => x.nameButton).ToList();
            actionNames.Add(TextData.Get(76));
            
            GameManager.Instance.assetMenuController.InitMenu(actionIds, actionNames, actions, assetName);
        }
        
        public void DialogWithPatient(string id, string playerText, string patientText)
        {
            StartCoroutine(DialogRoutine(id, playerText, patientText));
        }

        public IEnumerator DialogRoutine(string id, string playerText, string patientText)
        {
            yield return new WaitUntil(() => !isDialogInProgress);

            isDialogInProgress = true;
            FollowPlayer(true, true);

            // if (!string.IsNullOrEmpty(playerText))
            // {
            //     playerText = GameManager.Instance.scenarioLoader.ReplaceCaseVariables(playerText);
            //     TextToSpeech.Instance.SetText(playerText, DirectoryPath.Audio, playerText.GetHashCode().ToString(), TextToSpeech.Character.Doctor);
            //     
            //     yield return new WaitUntil(() => TextToSpeech.Instance.IsFinishedSpeaking());
            //     yield return new WaitForSeconds(1.0f);
            // }

            if (!string.IsNullOrEmpty(patientText))
            {
                patientText = GameManager.Instance.scenarioLoader.ReplaceCaseVariables(patientText);

                TextToSpeech.Instance.Speak(patientText, TextToSpeech.Character.Patient, true, _audioSource);
                yield return new WaitUntil(() => TextToSpeech.Instance.IsFinishedSpeaking());
                yield return new WaitForSeconds(1.0f);
            }
            
            FollowPlayer(true, false);
            
            if(!string.IsNullOrEmpty(id))
                GameManager.Instance.checkTableController.RegisterTriggerInvoke("dialog_" + id);
            isDialogInProgress = false;
        }
    }
}
