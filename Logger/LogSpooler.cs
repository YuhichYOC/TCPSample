/*
*
* LogSpooler.cs
*
* Copyright 2016 Yuichi Yoshii
*     吉井雄一 @ 吉井産業  you.65535.kir@gmail.com
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*     http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Logger {

    public class LogSpooler {
        private log4net.ILog logClient;

        private System.Threading.Timer timer;

        private List<string> errorLogLines;
        private List<string> warnLogLines;
        private List<string> infoLogLines;

        private int ticks;
        private int safeTicks;

        public void SetSafe(int arg) {
            safeTicks = arg;
        }

        public LogSpooler() {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(@"./SaveLog.config"));

            errorLogLines = new List<string>();
            warnLogLines = new List<string>();
            infoLogLines = new List<string>();

            ticks = 0;
            safeTicks = 0;
        }

        public void Start() {
            System.Threading.TimerCallback callback = new System.Threading.TimerCallback(onUpdate);
            timer = new System.Threading.Timer(callback, null, 1000, 1000);
        }

        private void onUpdate(object arg) {
            FlushError();
            FlushWarn();
            FlushInfo();
            ticks++;
            Console.WriteLine(@"LogOperator#onUpdate on Tick " + ticks.ToString());
            Console.WriteLine(@"SafeTicks " + safeTicks.ToString());
            if (ticks >= safeTicks) {
                Dispose();
            }
        }

        private void FlushError() {
            if (errorLogLines.Count > 0) {
                StringBuilder message = new StringBuilder();
                message.AppendLine(@"\r\n");
                int lastIndex = errorLogLines.Count - 1;
                if (lastIndex > 0) {
                    for (int i = 0; i < lastIndex; i++) {
                        message.AppendLine(errorLogLines[i]);
                    }
                    logClient = log4net.LogManager.GetLogger(@"ErrorLog");
                    logClient.Error(message.ToString());
                    errorLogLines.RemoveRange(0, lastIndex);
                }
            }
        }

        private void FlushWarn() {
            if (warnLogLines.Count > 0) {
                StringBuilder message = new StringBuilder();
                message.AppendLine(@"\r\n");
                int lastIndex = warnLogLines.Count - 1;
                if (lastIndex > 0) {
                    for (int i = 0; i < lastIndex; i++) {
                        message.AppendLine(warnLogLines[i]);
                    }
                    logClient = log4net.LogManager.GetLogger(@"WarnLog");
                    logClient.Warn(message.ToString());
                    warnLogLines.RemoveRange(0, lastIndex);
                }
            }
        }

        private void FlushInfo() {
            if (infoLogLines.Count > 0) {
                StringBuilder message = new StringBuilder();
                message.AppendLine(@"\r\n");
                int lastIndex = infoLogLines.Count - 1;
                if (lastIndex > 0) {
                    for (int i = 0; i < lastIndex; i++) {
                        message.AppendLine(infoLogLines[i]);
                    }
                    logClient = log4net.LogManager.GetLogger(@"InfoLog");
                    logClient.Info(message.ToString());
                    infoLogLines.RemoveRange(0, lastIndex);
                }
            }
        }

        public void Dispose() {
            timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            FlushError();
            FlushWarn();
            FlushInfo();
        }

        public void AppendError(string message) {
            try {
                errorLogLines.Add(message);
            }
            catch (Exception ex) {
                logClient = log4net.LogManager.GetLogger(@"LogOperatorLog");
                logClient.Error(@"Exception during AppendError\r\n" + message, ex);
            }
        }

        public void AppendWarn(string message) {
            try {
                warnLogLines.Add(message);
            }
            catch (Exception ex) {
                logClient = log4net.LogManager.GetLogger(@"LogOperatorLog");
                logClient.Error(@"Exception during AppendWarn\r\n" + message, ex);
            }
        }

        public void AppendInfo(string message) {
            try {
                infoLogLines.Add(message);
            }
            catch (Exception ex) {
                logClient = log4net.LogManager.GetLogger(@"LogOperatorLog");
                logClient.Error(@"Exception during AppendInfo\r\n" + message, ex);
            }
        }
    }
}