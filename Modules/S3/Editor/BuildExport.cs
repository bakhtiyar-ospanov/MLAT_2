#if UNITY_CLOUD_BUILD || UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Modules.WDCore;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class BuildExport : MonoBehaviour
{
    private static string version;
    
#if UNITY_CLOUD_BUILD
    public static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
    {
        Debug.Log("PRE EXPORT STARTS !!!!!!");
        var newVersion =  Environment.GetEnvironmentVariable("BUILD_VERSION_PREFIX") + manifest.GetValue<int>("buildNumber");
        Debug.Log("VERSION SET TO " + newVersion);
        UnityEditor.PlayerSettings.bundleVersion = newVersion;
        var path = Directory.GetParent(Application.dataPath) + "/InnoSetup_UCB/version.txt";
        FileHandler.WriteTextFile(path, newVersion);
        
        var request = new LoginWithCustomIDRequest { 
            CustomId = SystemInfo.deviceUniqueIdentifier, 
            CreateAccount = true,
        };
            
        PlayFabClientAPI.LoginWithCustomID(request, Debug.Log, Debug.Log);

        version = newVersion;
        
        Debug.Log("PRE EXPORT ENDS !!!!!! ");
    }
#endif

    public static void PostExport(string exportPath)
    {
        Debug.Log("POST EXPORT STARTS !!!!!!" + version);
        
        PlayFabClientAPI.ExecuteCloudScript(
            new ExecuteCloudScriptRequest{ 
                FunctionName = "updateWinBetaVersion", 
                FunctionParameter = new {BuildVer = version}
            }, Debug.Log, Debug.Log);
        
        Debug.Log("POST EXPORT ENDS !!!!!! ");
    }
    

#endif
}
