using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TunrSync
{
    public static class AudioHash
    {
        /// <summary>
        /// Uses ffmpeg to calculate the audio MD5 hash of the given file
        /// </summary>
        /// <param name="fileName">Path of audio file to hash</param>
        /// <returns>MD5 hash in string form</returns>
        public static string HashAudio(string fileName)
        {
            // Calculate MD5 hash of raw audio
            string audioMd5Hash = "";
            using (System.Diagnostics.Process ffhash = new System.Diagnostics.Process())
            {
                string args = "-v quiet -i \"" + fileName + "\" -acodec copy -vn -f md5 - ";
                ffhash.StartInfo = new System.Diagnostics.ProcessStartInfo("Misc\\ffmpeg.exe", args);
                ffhash.StartInfo.RedirectStandardOutput = true;
                ffhash.StartInfo.UseShellExecute = false;
                ffhash.StartInfo.CreateNoWindow = true;
                StringBuilder ffhashOutput = new StringBuilder();
                ffhash.Start();
                while (!ffhash.StandardOutput.EndOfStream)
                {
                    ffhashOutput.Append(ffhash.StandardOutput.ReadLine());
                }
                audioMd5Hash = ffhashOutput.ToString();
                audioMd5Hash = audioMd5Hash.Substring(audioMd5Hash.IndexOf("MD5=") + 4, 32);
                return audioMd5Hash;
            }
        }
    }
}
