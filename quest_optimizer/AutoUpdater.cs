using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;

namespace quest_optimizer
{
    class AutoUpdater
    {
        #region Object constructor
        /// <summary>
        /// Starts the update thread.
        /// </summary>
        public static void StartUpdate()
        {
            var tUpdateThread = new Thread(UpdateThread);
            tUpdateThread.Start();
        }
        #endregion

        #region Object methods
        /// <summary>
        /// Checks if there's a pending update.
        /// </summary>
        private static void UpdateThread()
        {
            try
            {
                var request = WebRequest.Create($"https://api.github.com/repos/{General.Repository}/releases/latest") as HttpWebRequest;

                if(request != null)
                {
                    request.UserAgent = "cssbhop";
                    request.Method = "GET";

                    using(var responseReader = new StreamReader(request.GetResponse().GetResponseStream()))
                    {
                        dynamic dReleaseInfo = new JavaScriptSerializer().Deserialize<dynamic>(responseReader.ReadToEnd());
                        float fLatestVersion = float.Parse(dReleaseInfo["tag_name"]);

                        if(fLatestVersion > General.Version)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine(Environment.NewLine + $"--- There's a pending update! (from {General.Version} to {fLatestVersion}" + Environment.NewLine);
                            Console.ForegroundColor = ConsoleColor.White;
                            
                            Process.Start($"https://github.com/{General.Repository}/releases/latest");
                        }
                    }
                }
            }

            catch(Exception)
            {
                // keep quiet
            }
        }
        #endregion
    }
}
