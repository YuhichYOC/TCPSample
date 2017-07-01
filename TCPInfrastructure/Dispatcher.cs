/*
*
* Dispatcher.cs
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

namespace TCPInfrastructure {

    public class Dispatcher {

        #region -- Member fields --

        private SAXWrapper.SettingReader setting;

        private string dispatcherAddr;

        private int portNumber;

        private string dispatcherName;

        private int dispatcherId;

        private System.Threading.Timer timer;

        private System.Net.Sockets.TcpListener listener;

        private System.Net.Sockets.TcpClient client;

        private System.Collections.Generic.List<Client> clients;

        private enum WaitStatus {
            NA, WAITING, SUCCESS, FAILURE,
        }

        private WaitStatus success;

        private string command;

        private string current;

        private string onSuccess;

        private string onFailure;

        private Logger.LogSpooler log;

        #endregion -- Member fields --

        #region -- Setter methods --

        public void SetDispatcherAddr(string arg) {
            dispatcherAddr = arg;
        }

        public void SetPortNumber(int arg) {
            portNumber = arg;
        }

        public void SetDispatcherName(string arg) {
            dispatcherName = arg;
        }

        public void SetDispatcherID(int arg) {
            dispatcherId = arg;
        }

        public void SetLog(Logger.LogSpooler arg) {
            log = arg;
        }

        private void ChangeStatus(WaitStatus arg) {
            switch (arg) {
                case WaitStatus.SUCCESS:
                    log.AppendInfo(@"Dispatcher : Status changed to SUCCESS");
                    break;

                case WaitStatus.FAILURE:
                    log.AppendInfo(@"Dispatcher : Status changed to FAILURE");
                    break;

                case WaitStatus.WAITING:
                    log.AppendInfo(@"Dispatcher : Status changed to WAITING");
                    break;

                default:
                    log.AppendInfo(@"Dispatcher : Status changed to NA");
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

        #region -- Init --

        public Dispatcher(string fileName, string category) {
            ReadSetting(fileName);
            ApplySetting(category);
            success = WaitStatus.NA;

            clients = new System.Collections.Generic.List<Client>();
        }

        private void ReadSetting(string fileName) {
            setting = new SAXWrapper.SettingReader();
            setting.SetDirectory(System.IO.Path.GetDirectoryName(fileName));
            setting.SetFileName(System.IO.Path.GetFileName(fileName));
            setting.Parse();
        }

        private void ApplySetting(string category) {
            dispatcherAddr = setting.GetNode().SubCategory(category).Attr(@"Dispatcher").GetNodeValue();
            portNumber = int.Parse(setting.GetNode().SubCategory(category).Attr(@"Port").GetNodeValue());
            dispatcherName = setting.GetNode().SubCategory(category).Attr(@"DispatcherName").GetNodeValue();
            dispatcherId = int.Parse(setting.GetNode().SubCategory(category).Attr(@"ID").GetNodeValue());
            DetectRoot(category);
        }

        private void DetectRoot(string category) {
            command = setting.GetNode().SubCategory(category).Attr(@"StartCommand").GetNodeValue();
            foreach (SAXWrapper.NodeEntity item in setting.GetNode().SubCategory(command).GetChildren()) {
                if (item.GetNodeName().Equals(@"Category") && item.AttrExists(@"description") && item.AttrByName(@"description").Equals(@"root")) {
                    current = item.AttrByName(@"name");
                    onSuccess = item.Attr(@"OnSuccess").GetNodeValue();
                    onFailure = item.Attr(@"OnFailure").GetNodeValue();
                }
            }
        }

        public void Start() {
            System.Threading.TimerCallback callback = new System.Threading.TimerCallback(onUpdate);
            timer = new System.Threading.Timer(callback, null, 1000, 1000);
        }

        #endregion -- Init --

        #region -- Tick --

        private void onUpdate(object arg) {
            try {
                log.AppendInfo(@"Dispatcher : status = " + GetStatus());

                switch (success) {
                    case WaitStatus.NA:
                        log.AppendInfo(@"Dispatcher : success = NA");
                        MethodInvoke();
                        break;

                    case WaitStatus.SUCCESS:
                        log.AppendInfo(@"Dispatcher : success = SUCCESS");
                        FindNextCommandOnSuccess();
                        MethodInvoke();
                        break;

                    case WaitStatus.FAILURE:
                        log.AppendInfo(@"Dispatcher : success = FAILURE");
                        FindNextCommandOnFailure();
                        MethodInvoke();
                        break;

                    default:
                        log.AppendInfo(@"Dispatcher : success = default");
                        break;
                }
                log.AppendInfo(@"Dispatcher on tick");
            }
            catch (System.Exception ex) {
                log.AppendError(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void MethodInvoke() {
            log.AppendInfo(@"Dispatcher : MethodInvoke called");
            ChangeStatus(WaitStatus.WAITING);
            StartConnect();
            StartListener();
            AcceptAnyConnection();
            Dispatch();
            StopListener();
            Dispose();
        }

        private void FindNextCommandOnSuccess() {
            log.AppendInfo(@"Dispatcher : FindNextCommandOnSuccess called");
            foreach (SAXWrapper.NodeEntity item in setting.GetNode().SubCategory(command).GetChildren()) {
                if (item.GetNodeName().Equals(@"Category") && item.AttrExists(@"name") && item.AttrByName(@"name").Equals(onSuccess)) {
                    current = item.AttrByName(@"name");
                    onSuccess = item.Attr(@"OnSuccess").GetNodeValue();
                    onFailure = item.Attr(@"OnFailure").GetNodeValue();
                    return;
                }
            }
        }

        private void FindNextCommandOnFailure() {
            log.AppendInfo(@"Dispatcher : FindNextCommandOnFailure called");
            foreach (SAXWrapper.NodeEntity item in setting.GetNode().SubCategory(command).GetChildren()) {
                if (item.GetNodeName().Equals(@"Category") && item.AttrExists(@"name") && item.AttrByName(@"name").Equals(onFailure)) {
                    current = item.AttrByName(@"name");
                    onSuccess = item.Attr(@"OnSuccess").GetNodeValue();
                    onFailure = item.Attr(@"OnFailure").GetNodeValue();
                    return;
                }
            }
        }

        #endregion -- Tick --

        #region -- L_00_PORTCHECK --

        public void StartConnect() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"StartConnect")) {
                return;
            }
            log.AppendInfo(@"Dispatcher : StartConnect called, starting port check");
            client = new System.Net.Sockets.TcpClient(System.Net.Sockets.AddressFamily.InterNetwork);
            System.AsyncCallback callback = new System.AsyncCallback(ConnectedCallback);
            client.BeginConnect(dispatcherAddr, portNumber, callback, client);
        }

        private void ConnectedCallback(System.IAsyncResult arg) {
            try {
                System.Net.Sockets.TcpClient c = (System.Net.Sockets.TcpClient)arg.AsyncState;
                c.EndConnect(arg);
                log.AppendInfo(@"Dispatcher : ConnectedCallback called, any socket are existing");
            }
            catch (System.Net.Sockets.SocketException ex) {
                log.AppendInfo(@"Dispatcher : No listener waiting for Connection. Listener will be started.");
                log.AppendInfo(ex.Message);
                ChangeStatus(WaitStatus.SUCCESS);
            }
            catch (System.Exception ex) {
                log.AppendError(@"Dispatcher : ConnectedCallback caught Exception : ");
                log.AppendError(ex.Message + "\r\n" + ex.StackTrace);
                ChangeStatus(WaitStatus.FAILURE);
            }
        }

        #endregion -- L_00_PORTCHECK --

        #region -- L_00_START --

        public void StartListener() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"StartListener")) {
                return;
            }
            log.AppendInfo(@"Dispatcher : StartListener called");
            listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, portNumber);
            listener.Start();
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- L_00_START --

        #region -- L_00_ACCEPT --

        public void AcceptAnyConnection() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"AcceptAnyConnection")) {
                return;
            }
            log.AppendInfo(@"Dispatcher : AcceptAnyConnection called");
            ChangeStatus(WaitStatus.NA);
            if (listener.Pending()) {
                dispatcherId++;
                client = listener.AcceptTcpClient();
                log.AppendInfo(@"Dispatcher : any client Accepted");
                if (!listener.Pending()) {
                    ChangeStatus(WaitStatus.SUCCESS);
                    log.AppendInfo(@"Dispatcher : no client are waiting");
                }
            }
        }

        #endregion -- L_00_ACCEPT --

        #region -- S_00_DISPATCH --

        public void Dispatch() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"Dispatch")) {
                return;
            }
            log.AppendInfo(@"Dispatcher : Dispatch called");
            ChangeStatus(WaitStatus.SUCCESS);
            Client c = new Client(@"./Setting.config", @"ServerDefault");
            c.SetClient(client);
            c.SetLog(log);
            c.Start();
            clients.Add(c);
        }

        #endregion -- S_00_DISPATCH --

        #region -- L_00_STOP --

        public void StopListener() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"StopListener")) {
                return;
            }
            log.AppendInfo(@"Dispatcher : StopListener called");
            try {
                if (listener != null) {
                    listener.Stop();
                }
            }
            catch (System.ObjectDisposedException) {
            }
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- L_00_STOP --

        #region -- L_00_DISPOSE --

        public void Dispose() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"Dispose")) {
                return;
            }
            log.AppendInfo(@"Dispatcher : Dispose called");
            StopListener();
            timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            timer.Dispose();
            log.AppendInfo(@"Disposed");
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- L_00_DISPOSE --
    }
}