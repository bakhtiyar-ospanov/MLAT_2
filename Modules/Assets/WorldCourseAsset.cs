using System.Collections;
using System.Collections.Generic;
using Modules.Books;
using Modules.S3;
using Modules.WDCore;
using UnityEngine;
using VargatesOpenSDK;

namespace Modules.Assets
{
    public class WorldCourseAsset : Asset
    {
        private WorldCoursesInfo info;
        public string uniqueId;
        public string courseListPath;
        public string courseFolderPath;
        public string previewsFolderPath;
        public string catalogPath;
        public string accessKey;
        public string secretKey;
        public string serverUrl;

        public override void Init()
        {
            base.Init();
            assetMenu = new List<MedicalBase.MenuItem>();
            
            info = GetComponent<WorldCoursesInfo>();

            StartCoroutine(InitRoutine());
        }

        private IEnumerator InitRoutine()
        {
            AddressablesS3.WorldInfo worldInfo = null;

            yield return StartCoroutine(GameManager.Instance.addressablesS3.GetWorldInfo(info.uniqueId, s => worldInfo = s));

            if(worldInfo == null) yield break;

            uniqueId = info.uniqueId;
            courseListPath = info.courseListPath;
            courseFolderPath = info.courseFolderPath;
            previewsFolderPath = info.previewsFolderPath;
            catalogPath = info.catalogPath;
            accessKey = worldInfo.accessKey;
            secretKey = worldInfo.secretKey;
            serverUrl = worldInfo.serverUrl;

            var item = new MedicalBase.MenuItem { 
                name = "",
                call = () =>
                {
                    GameManager.Instance.wCourseSelectorController.Init(this);
                }};
            
            assetMenu.Add(item);
        }
    }
}
