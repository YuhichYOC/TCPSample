/*
*
* Client.cs
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

    public class Client {

        #region -- Member fields --

        private SAXWrapper.SettingReader setting;

        private string serverAddr;

        private int portNumber;

        private string clientName;

        private int clientId;

        private int messageLength;

        private System.Collections.Generic.List<byte> message;

        private System.Threading.Timer timer;

        private System.Net.Sockets.TcpClient client;

        private System.Net.Sockets.NetworkStream stream;

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

        public void SetServerAddr(string arg) {
            serverAddr = arg;
        }

        public void SetPortNumber(int arg) {
            portNumber = arg;
        }

        public void SetClientName(string arg) {
            clientName = arg;
        }

        public void SetClientID(int arg) {
            clientId = arg;
        }

        public void SetMessageLength(int arg) {
            messageLength = arg;
        }

        public void SetMessage(System.Collections.Generic.List<byte> arg) {
            message = arg;
        }

        public void SetClient(System.Net.Sockets.TcpClient arg) {
            client = arg;
        }

        public void SetLog(Logger.LogSpooler arg) {
            log = arg;
        }

        private void ChangeStatus(WaitStatus arg) {
            switch (arg) {
                case WaitStatus.SUCCESS:
                    log.AppendInfo(@"Client#" + clientId.ToString() + @" : Status changed to SUCCESS");
                    break;

                case WaitStatus.FAILURE:
                    log.AppendInfo(@"Client#" + clientId.ToString() + @" : Status changed to FAILURE");
                    break;

                case WaitStatus.WAITING:
                    log.AppendInfo(@"Client#" + clientId.ToString() + @" : Status changed to WAITING");
                    break;

                default:
                    log.AppendInfo(@"Client#" + clientId.ToString() + @" : Status changed to NA");
                    break;
            }
            success = arg;
        }

        #endregion -- Setter methods --

        #region -- Getter methods --

        public int GetMessageLength() {
            return messageLength;
        }

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

        public Client(string fileName, string category) {
            ReadSetting(fileName);
            ApplySetting(category);
            message = new System.Collections.Generic.List<byte>();
            success = WaitStatus.NA;

            client = null;
            stream = null;
        }

        private void ReadSetting(string fileName) {
            setting = new SAXWrapper.SettingReader();
            setting.SetDirectory(System.IO.Path.GetDirectoryName(fileName));
            setting.SetFileName(System.IO.Path.GetFileName(fileName));
            setting.Parse();
        }

        private void ApplySetting(string category) {
            serverAddr = setting.GetNode().SubCategory(category).Attr(@"Server").GetNodeValue();
            portNumber = int.Parse(setting.GetNode().SubCategory(category).Attr(@"Port").GetNodeValue());
            clientName = setting.GetNode().SubCategory(category).Attr(@"ClientName").GetNodeValue();
            clientId = int.Parse(setting.GetNode().SubCategory(category).Attr(@"ID").GetNodeValue());
            messageLength = int.Parse(setting.GetNode().SubCategory(category).Attr(@"MessageLength").GetNodeValue());
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
                log.AppendInfo(@"Client#" + clientId.ToString() + @" status = " + GetStatus());

                switch (success) {
                    case WaitStatus.NA:
                        log.AppendInfo(@"Client#" + clientId.ToString() + @" : success = NA");
                        MethodInvoke();
                        break;

                    case WaitStatus.SUCCESS:
                        log.AppendInfo(@"Client#" + clientId.ToString() + @" : success = SUCCESS");
                        FindNextCommandOnSuccess();
                        MethodInvoke();
                        break;

                    case WaitStatus.FAILURE:
                        log.AppendInfo(@"Client#" + clientId.ToString() + @" : success = FAILURE");
                        FindNextCommandOnFailure();
                        MethodInvoke();
                        break;

                    default:
                        log.AppendInfo(@"Client#" + clientId.ToString() + @" : success = default");
                        break;
                }
                log.AppendInfo(@"Client on tick");
            }
            catch (System.Exception ex) {
                log.AppendError(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void MethodInvoke() {
            ChangeStatus(WaitStatus.WAITING);
            StartConnect();
            PrepareStream();
            SendMessage();
            StartReadMessage();
            QueueMessage();
            Disconnect();
            Dispose();
        }

        private void FindNextCommandOnSuccess() {
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

        #region -- C_00_CONNECT --

        public void StartConnect() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"StartConnect")) {
                return;
            }
            log.AppendInfo(@"Client#" + clientId.ToString() + @" : StartConnect called");
            client = new System.Net.Sockets.TcpClient(System.Net.Sockets.AddressFamily.InterNetwork);
            System.AsyncCallback callback = new System.AsyncCallback(ConnectedCallback);
            client.BeginConnect(serverAddr, portNumber, callback, client);
        }

        private void ConnectedCallback(System.IAsyncResult arg) {
            try {
                System.Net.Sockets.TcpClient c = (System.Net.Sockets.TcpClient)arg.AsyncState;
                c.EndConnect(arg);
                ChangeStatus(WaitStatus.SUCCESS);
            }
            catch (System.Net.Sockets.SocketException ex) {
                if (ex.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionRefused) {
                    log.AppendInfo(@"ConnectedCallback caught SocketException : Connection refused.");
                }
                else {
                    log.AppendInfo(@"ConnectedCallback caught SocketException");
                }
                ChangeStatus(WaitStatus.FAILURE);
            }
            catch (System.Exception ex) {
                log.AppendError(@"ConnectedCallback caught Exception : ");
                log.AppendError(ex.Message + "\r\n" + ex.StackTrace);
                ChangeStatus(WaitStatus.FAILURE);
            }
        }

        #endregion -- C_00_CONNECT --

        #region -- C_10_PREPARE --

        public void PrepareStream() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"PrepareStream")) {
                return;
            }
            log.AppendInfo(@"Client#" + clientId.ToString() + @" : PrepareStream called");
            stream = client.GetStream();
            log.AppendInfo(@"PrepareStream : NetworkStream prepared.");
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- C_10_PREPARE --

        #region -- C_20_SEND --

        public void SendMessage() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"SendMessage")) {
                return;
            }
            log.AppendInfo(@"Client#" + clientId.ToString() + @" : SendMessage called");
            string sendmessage = setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Message").GetNodeValue();
            log.AppendInfo(@"Client#" + clientId.ToString() + @" : message = " + sendmessage);
            byte[] sendbytearray = System.Text.Encoding.UTF8.GetBytes(sendmessage);
            stream.Write(sendbytearray, 0, sendbytearray.Length);
            stream.Flush();
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- C_20_SEND --

        #region -- C_30_READ --

        private byte[] bmessage;

        public void StartReadMessage() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"StartReadMessage")) {
                return;
            }
            log.AppendInfo(@"Client#" + clientId.ToString() + @" : StartReadMessage called");
            bmessage = new byte[messageLength];
            System.AsyncCallback callback = new System.AsyncCallback(ReadCallback);
            stream.BeginRead(bmessage, 0, messageLength, callback, stream);
        }

        private void ReadCallback(System.IAsyncResult arg) {
            try {
                System.Net.Sockets.NetworkStream s = (System.Net.Sockets.NetworkStream)arg.AsyncState;
                s.EndRead(arg);
                message.AddRange(bmessage);
                ChangeStatus(WaitStatus.SUCCESS);
            }
            catch (System.Exception ex) {
                log.AppendError(@"ReadCallback caught Exception : ");
                log.AppendError(ex.Message + "\r\n" + ex.StackTrace);
                ChangeStatus(WaitStatus.FAILURE);
            }
        }

        #endregion -- C_30_READ --

        #region -- S_50_QUEUEMESSAGE --

        public void QueueMessage() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"QueueMessage")) {
                return;
            }
            string s = System.Text.Encoding.UTF8.GetString(message.ToArray());
            log.AppendInfo(@"Client#" + clientId.ToString() + @" : Message " + s);
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- S_50_QUEUEMESSAGE --

        #region -- C_40_DISCONNECT --

        public void Disconnect() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"Disconnect")) {
                return;
            }
            log.AppendInfo(@"Client#" + clientId.ToString() + @" : Disconnect called");
            try {
                if (stream != null) {
                    stream.Close();
                }
            }
            catch (System.ObjectDisposedException) {
            }
            try {
                if (client != null) {
                    client.Close();
                }
            }
            catch (System.ObjectDisposedException) {
            }
            log.AppendInfo(@"Disconnected");
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- C_40_DISCONNECT --

        #region -- C_XX_DISPOSE --

        public void Dispose() {
            if (!setting.GetNode().SubCategory(command).SubCategory(current).Attr(@"Method").GetNodeValue().Equals(@"Dispose")) {
                return;
            }
            log.AppendInfo(@"Client#" + clientId.ToString() + @" : Dispose called");
            Disconnect();
            timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            timer.Dispose();
            log.AppendInfo(@"Disposed");
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- C_XX_DISPOSE --
    }
}