/*
*
* TaskInfrastructure
* AbstractClass.cs
*
* Copyright 2017 Yuichi Yoshii
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
using System.IO;
using System.Threading;

using Logger;
using SAXWrapper;

namespace TaskInfrastructure {

    class AbstractClass {

        #region -- Member fields --

        private SettingReader setting;

        private Timer timer;

        public enum WaitStatus {
            NA, WAITING, SUCCESS, FAILURE,
        }

        private WaitStatus success;

        private string command;

        private string current;

        private string onSuccess;

        private string onFailure;

        private LogSpooler log;

        #endregion -- Member fields --

        #region -- Setter methods --

        public void SetLog(LogSpooler arg) {
            log = arg;
        }

        public void ChangeStatus(WaitStatus arg) {
            switch (arg) {
                case WaitStatus.SUCCESS:
                    log.AppendInfo(@"Status changed to SUCCESS");
                    break;

                case WaitStatus.FAILURE:
                    log.AppendInfo(@"Status changed to FAILURE");
                    break;

                case WaitStatus.WAITING:
                    log.AppendInfo(@"Status changed to WAITING");
                    break;

                default:
                    log.AppendInfo(@"Status changed to NA");
                    break;
            }
            success = arg;
        }

        #endregion -- Setter methods --

        #region -- Getter methods --

        public string GetStatus() {
            switch (success) {
                case WaitStatus.SUCCESS:
                    return @"SUCCESS";

                case WaitStatus.FAILURE:
                    return @"FAILURE";

                case WaitStatus.WAITING:
                    return @"WAITING";

                default:
                    return @"NA";
            }
        }

        #endregion -- Getter methods --

        public AbstractClass(string file, string category) {
            ReadSetting(file);
            ApplySetting(category);
        }

        private void ReadSetting(string arg) {
            setting = new SettingReader();
            setting.SetDirectory(Path.GetDirectoryName(arg));
            setting.SetFileName(Path.GetFileName(arg));
            setting.Parse();
        }

        private void ApplySetting(string arg) {
            command = setting.GetNode().SubCategory(arg).Attr(@"StartCommand").GetNodeValue();
            foreach (NodeEntity n in setting.GetNode().SubCategory(command).GetChildren()) {
                if (n.GetNodeName().Equals(@"Category") && n.AttrExists(@"description") && n.AttrByName(@"description").Equals(@"root")) {
                    current = n.AttrByName(@"name");
                    onSuccess = n.Attr(@"OnSuccess").GetNodeValue();
                    onFailure = n.Attr(@"OnFailure").GetNodeValue();
                }
            }
        }

        public void Start() {
            TimerCallback callback = new TimerCallback(onUpdate);
            timer = new Timer(callback, null, 1000, 1000);
        }

        private void onUpdate(object arg) {
            try {
                switch (success) {
                    case WaitStatus.SUCCESS:
                        FindNextCommandOnSuccess();
                        break;

                    case WaitStatus.FAILURE:
                        FindNextCommandOnFailure();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex) {
                log.AppendError(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void FindNextCommandOnSuccess() {
            foreach (NodeEntity n in setting.GetNode().SubCategory(command).GetChildren()) {
                if (n.GetNodeName().Equals(@"Category") && n.AttrExists(@"name") && n.AttrByName(@"name").Equals(onSuccess)) {
                    current = n.AttrByName(@"name");
                    onSuccess = n.Attr(@"OnSuccess").GetNodeValue();
                    onFailure = n.Attr(@"OnFailure").GetNodeValue();
                    return;
                }
            }
        }

        private void FindNextCommandOnFailure() {
            foreach (NodeEntity n in setting.GetNode().SubCategory(command).GetChildren()) {
                if (n.GetNodeName().Equals(@"Category") && n.AttrExists(@"name") && n.AttrByName(@"name").Equals(onFailure)) {
                    current = n.AttrByName(@"name");
                    onSuccess = n.Attr(@"OnSuccess").GetNodeValue();
                    onFailure = n.Attr(@"OnFailure").GetNodeValue();
                    return;
                }
            }
        }
    }
}
