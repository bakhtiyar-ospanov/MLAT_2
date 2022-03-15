using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

namespace Modules.WDCore
{
    public static class DecipherHelper
    {
        public static IEnumerator DecodeToFile(string pathIn, string pathOut, string password)
        {
            var unpacking = Task.Run(() =>
            {
                byte[] rawBytes1 = null;

                while (rawBytes1 == null)
                {
                    FileHandler.ReadBytesFile(pathIn, out var response1);
                    rawBytes1 = response1;
                    if(rawBytes1 == null)
                        Task.Delay(1000).Wait();
                }
                
                while (!File.Exists(pathOut))
                {
                    var bytes1 = Decipher.DecryptBytesFromFile_OpenSSL_Des(password, rawBytes1);
                    FileHandler.WriteBytesFile(pathOut, bytes1);
                }
            });
            
            while(!unpacking.IsCompleted) { yield return null; }
        }
        
        public static IEnumerator DecodeToBytes(string pathIn, string password, Action<byte[]> decodedResponse)
        {
            var unpacking = Task.Run(() =>
            {
                byte[] rawBytes1 = null;
                while (rawBytes1 == null)
                {
                    FileHandler.ReadBytesFile(pathIn, out var response1);
                    rawBytes1 = response1;
                    if(rawBytes1 == null)
                        Task.Delay(1000).Wait();
                }

                byte[] decodedBytes = null;
                while (decodedBytes == null)
                {
                    decodedBytes = Decipher.DecryptBytesFromFile_OpenSSL_Des(password, rawBytes1);
                    if(decodedBytes == null)
                        Task.Delay(1000).Wait();
                }

                return decodedBytes;
            });
            
            while(!unpacking.IsCompleted) { yield return null; }
            
            decodedResponse?.Invoke(unpacking.Result);
        }

    }
}
