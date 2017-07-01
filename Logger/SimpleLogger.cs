/*
*
* SimpleLogger.cs
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

namespace Logger {

    public class SimpleLogger {
        private log4net.ILog log;

        public SimpleLogger() {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(@"./SaveLog.config"));
        }

        public void Error(string arg) {
            log = log4net.LogManager.GetLogger(@"ErrorLog");
            log.Error(arg);
        }

        public void Warn(string arg) {
            log = log4net.LogManager.GetLogger(@"WarnLog");
            log.Warn(arg);
        }

        public void Info(string arg) {
            log = log4net.LogManager.GetLogger(@"InfoLog");
            log.Info(arg);
        }
    }
}