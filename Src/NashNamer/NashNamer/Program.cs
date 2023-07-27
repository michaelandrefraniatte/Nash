using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NashNamer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string folderpath = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(@"file:\", "").Replace(Process.GetCurrentProcess().ProcessName + ".exe", "").Replace(@"\", "/").Replace(@"//", "");
            string[] fileEntries = Directory.GetFiles(folderpath);
            foreach (string fileEntrieImg in fileEntries)
            {
                if (fileEntrieImg.EndsWith("-audio.jpg") | fileEntrieImg.EndsWith("-video.jpg"))
                {
                    string fileEntrieID = fileEntrieImg.Replace("-audio.jpg", "").Replace("-video.jpg", "").Replace(folderpath, "");
                    foreach (string fileEntrieMedia in fileEntries)
                    {
                        if ((fileEntrieMedia.EndsWith(".mp3") | fileEntrieMedia.EndsWith(".3gp") | fileEntrieMedia.EndsWith(".flv") | fileEntrieMedia.EndsWith(".m4a") | fileEntrieMedia.EndsWith(".mp4") | fileEntrieMedia.EndsWith(".ogg") | fileEntrieMedia.EndsWith(".webm")) & fileEntrieMedia.Contains(fileEntrieID))
                        {
                            string newFileEntrieImg = fileEntrieMedia.Replace(".mp3", ".jpg").Replace(".3gp", ".jpg").Replace(".flv", ".jpg").Replace(".m4a", ".jpg").Replace(".mp4", ".jpg").Replace(".ogg", ".jpg").Replace(".webm", ".jpg");
                            File.Move(fileEntrieImg, newFileEntrieImg.Replace("'", "").Replace("#", ""));
                            if (fileEntrieImg.EndsWith("-audio.jpg"))
                            {
                                File.Move(fileEntrieMedia, newFileEntrieImg.Replace(".jpg", ".mp3").Replace("'", "").Replace("#", ""));
                            }
                            if (fileEntrieImg.EndsWith("-video.jpg"))
                            {
                                File.Move(fileEntrieMedia, fileEntrieMedia.Replace("'", "").Replace("#", ""));
                            }
                        }
                    }
                }
            }
            System.Threading.Thread.Sleep(5000);
            fileEntries = Directory.GetFiles(folderpath);
            foreach (string fileEntrieImg in fileEntries)
            {
                if (fileEntrieImg.EndsWith("-audio.jpg") | fileEntrieImg.EndsWith("-video.jpg"))
                {
                    File.Delete(fileEntrieImg);
                }
            }
        }
    }
}
