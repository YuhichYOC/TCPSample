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

    public class Dispatcher : JobInfrastructure.AbstractClass {

        #region -- Private Fields --

        private SAXWrapper.XReader setting;

        private string dispatcherAddr;

        private int portNumber;

        private string dispatcherName;

        private int dispatcherId;

        private System.Net.Sockets.TcpListener listener;

        private System.Net.Sockets.TcpClient client;

        private System.Collections.Generic.List<Client> clients;

        #endregion -- Private Fields --

        #region -- Setter --

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

        #endregion -- Setter --

        #region -- Init --

        public Dispatcher(string file, string category) : base(file, category) {
            ReadSetting(file);
            ApplySetting(category);

            clients = new System.Collections.Generic.List<Client>();
        }

        private void ReadSetting(string file) {
            setting = new SAXWrapper.XReader();
            setting.SetDirectory(System.IO.Path.GetDirectoryName(file));
            setting.SetFileName(System.IO.Path.GetFileName(file));
            setting.Parse();
        }

        private void ApplySetting(string category) {
            dispatcherAddr = setting.GetNode().SubCategory(category).Attr(@"Dispatcher").GetNodeValue();
            portNumber = int.Parse(setting.GetNode().SubCategory(category).Attr(@"Port").GetNodeValue());
            dispatcherName = setting.GetNode().SubCategory(category).Attr(@"DispatcherName").GetNodeValue();
            dispatcherId = int.Parse(setting.GetNode().SubCategory(category).Attr(@"ID").GetNodeValue());
        }

        #endregion -- Init --

        #region -- Timer Tick --

        protected override void MInvoke() {
            base.MInvoke();
            StartConnect();
            StartListener();
            AcceptAnyConnection();
            Dispatch();
            StopListener();
            Dispose();
        }

        #endregion -- Timer Tick --

        #region -- L_00_PORTCHECK --

        public void StartConnect() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"StartConnect")) {
                return;
            }
            LogInfo(@"Dispatcher : StartConnect called, starting port check");
            client = new System.Net.Sockets.TcpClient(System.Net.Sockets.AddressFamily.InterNetwork);
            System.AsyncCallback callback = new System.AsyncCallback(ConnectedCallback);
            client.BeginConnect(dispatcherAddr, portNumber, callback, client);
        }

        private void ConnectedCallback(System.IAsyncResult arg) {
            try {
                System.Net.Sockets.TcpClient c = (System.Net.Sockets.TcpClient)arg.AsyncState;
                c.EndConnect(arg);
                LogInfo(@"Dispatcher : ConnectedCallback called, any socket are existing");
            } catch (System.Net.Sockets.SocketException ex) {
                LogInfo(@"Dispatcher : No listener waiting for Connection. Listener will be started.");
                LogInfo(ex.Message);
                ChangeStatus(WaitStatus.SUCCESS);
            } catch (System.Exception ex) {
                LogError(@"Dispatcher : ConnectedCallback caught Exception : ");
                LogError(ex.Message + "\r\n" + ex.StackTrace);
                ChangeStatus(WaitStatus.FAILURE);
            }
        }

        #endregion -- L_00_PORTCHECK --

        #region -- L_00_START --

        public void StartListener() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"StartListener")) {
                return;
            }
            LogInfo(@"Dispatcher : StartListener called");
            listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, portNumber);
            listener.Start();
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- L_00_START --

        #region -- L_00_ACCEPT --

        public void AcceptAnyConnection() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"AcceptAnyConnection")) {
                return;
            }
            LogInfo(@"Dispatcher : AcceptAnyConnection called");
            ChangeStatus(WaitStatus.NA);
            if (listener.Pending()) {
                dispatcherId++;
                client = listener.AcceptTcpClient();
                LogInfo(@"Dispatcher : any client Accepted");
                if (!listener.Pending()) {
                    ChangeStatus(WaitStatus.SUCCESS);
                    LogInfo(@"Dispatcher : no client are waiting");
                }
            }
        }

        #endregion -- L_00_ACCEPT --

        #region -- S_00_DISPATCH --

        public void Dispatch() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"Dispatch")) {
                return;
            }
            LogInfo(@"Dispatcher : Dispatch called");
            ChangeStatus(WaitStatus.SUCCESS);
            Client c = new Client(@"./Setting.config", @"ServerDefault");
            c.SetClient(client);
            c.SetLog(GetLog());
            c.Start();
            clients.Add(c);
        }

        #endregion -- S_00_DISPATCH --

        #region -- L_00_STOP --

        public void StopListener() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"StopListener")) {
                return;
            }
            LogInfo(@"Dispatcher : StopListener called");
            try {
                if (listener != null) {
                    listener.Stop();
                }
            } catch (System.ObjectDisposedException) {
            }
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- L_00_STOP --

        #region -- L_00_DISPOSE --

        public void Dispose() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"Dispose")) {
                return;
            }
            LogInfo(@"Dispatcher : Dispose called");
            StopListener();
            KillTimer();
            LogInfo(@"Disposed");
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- L_00_DISPOSE --
    }
}