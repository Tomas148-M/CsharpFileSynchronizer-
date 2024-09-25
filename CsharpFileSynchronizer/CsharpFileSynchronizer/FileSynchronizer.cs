using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace CsharpFileSynchronizer
{
    public class FileSynchronizer
    {
        private string _source;
        private string _backup;
        private string _log_file;

        public FileSynchronizer() 
        {
            SetupLogging();


        }  
        
        private void SetupLogging()
        {

        }

        public string CalculateMD5(string filePath)
        {
            // Open the file as a FileStream
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    // Compute the MD5 hash
                    byte[] hashBytes = md5.ComputeHash(stream);

                    // Convert the byte array to a hex string
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

    }
}
