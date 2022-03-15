using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Modules.SpeechKit
{
    public static class SpeechKit
    {
        #region Synthesis properties

        public enum SPLang
        {
            ru_RU,
            en_US,
            tr_TR
        }

        public static SPLang lang = SPLang.ru_RU;
        public static string Lang => lang.ToString().Replace("_", "-");

        public enum SPVoice
        {
            oksana,
            jane,
            omazh,
            zahar,
            ermil,
            silaerkan,
            erkanyavas,
            alyss,
            nick,
            alena,
            filipp
        }

        private static SPVoice _voice = SPVoice.alena;
        public static string Voice => _voice.ToString();

        public enum SPEmotion
        {
            neutral,
            evil,
            good
        }

        private static SPEmotion _emotion = SPEmotion.good;
        public static string Emotion => _emotion.ToString();

        public enum SPSpeed
        {
            x0_1,
            x0_5,
            x1_0,
            x1_5,
            x2_0,
            x2_5,
            x3_0,
        }

        private static SPSpeed _speed = SPSpeed.x1_0;
        public static string Speed => _speed.ToString().Replace("_", ".").Replace("x", "");

        public enum SPCodec
        {
            lpcm,
            oggopus
        }

        private static SPCodec _codec = SPCodec.lpcm;
        public static string Codec => _codec.ToString();

        public enum SPSampleRateHertz
        {
            _48000,
            _16000,
            _8000
        }

        private static SPSampleRateHertz _sampleRateHertz = SPSampleRateHertz._48000;
        public static string SampleRateHertz => _sampleRateHertz.ToString().Replace("_", "");

        #endregion

        #region Input and Output properties

        public enum SPStorageMode
        {
            files,
            memory
        }

        public static SPStorageMode StorageMode = SPStorageMode.files;

        #endregion

        #region API connection properties

        public enum SPAuthMode
        {
            IAMToken,
            APIKey
        }

        public static SPAuthMode AuthMode = SPAuthMode.APIKey;

        public static string AuthCode { set; get; } = "";

        public static string FolderID { set; get; } = "";

        #endregion

        #region Debug properties

        private static bool showDebugMessages = true;

        #endregion

        #region Private methods

        public static async Task<byte[]> TextToSpeech(string textToVoice, string localpath, SPVoice voice = default)
        {
            _voice = voice;
            if (showDebugMessages) Debug.Log("[SpeechKit] Try to speak: " + textToVoice + " File: " + localpath);

            var client = new HttpClient();

            try
            {
                if (showDebugMessages) Debug.Log("[SpeechKit] Authorization");

                client.DefaultRequestHeaders.Add("Authorization",
                    (AuthMode == SPAuthMode.APIKey ? "Api-Key " : "Bearer ") + AuthCode);

                var values = new Dictionary<string, string>
                {
                    {"text", textToVoice},
                    {"lang", Lang},
                    {"speed", Speed},
                    {"emotion", Emotion},
                    {"voice", Voice},
                    {"format", Codec},
                };

                if (AuthMode == SPAuthMode.IAMToken) values.Add("folderId", FolderID);
                if (_codec == SPCodec.lpcm) values.Add("sampleRateHertz", SampleRateHertz);

                var content = new FormUrlEncodedContent(values);

                if (showDebugMessages) Debug.Log("[SpeechKit] Response...");
                var response = await client.PostAsync("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize",
                    content);

                if (showDebugMessages) Debug.Log("[SpeechKit] Content...");
                var responseBytes = await response.Content.ReadAsByteArrayAsync();

                if (showDebugMessages) Debug.Log("[SpeechKit] Precess data...");

                if (StorageMode == SPStorageMode.files)
                {

                    try
                    {
                        if (showDebugMessages) Debug.Log("[SpeechKit] FILE: " + localpath);
                        File.WriteAllBytes(localpath, responseBytes);
                        return responseBytes;
                    }
                    catch
                    {
                        if (showDebugMessages) Debug.LogWarning("[SpeechKit] something wrong with file writing!");
                        throw;
                    }
                }
                else
                {
                    return responseBytes;
                }
            }
            catch
            {
                if (showDebugMessages) Debug.LogWarning("[SpeechKit] something wrong with API!");
                throw;
            }
            finally
            {
                client.Dispose();
                if (showDebugMessages) Debug.Log("[SpeechKit] Done.");
            }
        }

        #endregion
    }
}