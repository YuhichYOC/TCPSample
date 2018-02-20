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

    public class Client : JobInfrastructure.AbstractClass {

        #region -- Private Fields --

        private SAXWrapper.XReader setting;

        private string serverAddr;

        private int portNumber;

        private string clientName;

        private int clientId;

        private int messageLength;

        private System.Collections.Generic.List<byte> message;

        private System.Net.Sockets.TcpClient client;

        private System.Net.Sockets.NetworkStream stream;

        #endregion -- Private Fields --

        #region -- Setter --

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

        #endregion -- Setter --

        #region -- Getter --

        public int GetMessageLength() {
            return messageLength;
        }

        #endregion -- Getter --

        #region -- Init --

        public Client(string file, string category) : base(file, category) {
            ReadSetting(file);
            ApplySetting(category);
            message = new System.Collections.Generic.List<byte>();

            client = null;
            stream = null;
        }

        private void ReadSetting(string file) {
            setting = new SAXWrapper.XReader();
            setting.SetDirectory(System.IO.Path.GetDirectoryName(file));
            setting.SetFileName(System.IO.Path.GetFileName(file));
            setting.Parse();
        }

        private void ApplySetting(string category) {
            serverAddr = setting.GetNode().SubCategory(category).Attr(@"Server").GetNodeValue();
            portNumber = int.Parse(setting.GetNode().SubCategory(category).Attr(@"Port").GetNodeValue());
            clientName = setting.GetNode().SubCategory(category).Attr(@"ClientName").GetNodeValue();
            clientId = int.Parse(setting.GetNode().SubCategory(category).Attr(@"ID").GetNodeValue());
            messageLength = int.Parse(setting.GetNode().SubCategory(category).Attr(@"MessageLength").GetNodeValue());
        }

        #endregion -- Init --

        #region -- Timer Tick --

        protected override void MInvoke() {
            base.MInvoke();
            StartConnect();
            PrepareStream();
            SendMessage();
            StartReadMessage();
            QueueMessage();
            Disconnect();
            Dispose();
        }

        #endregion -- Timer Tick --

        #region -- C_00_CONNECT --

        public void StartConnect() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"StartConnect")) {
                return;
            }
            LogInfo(@"Client#" + clientId.ToString() + @" : StartConnect called");
            client = new System.Net.Sockets.TcpClient(System.Net.Sockets.AddressFamily.InterNetwork);
            System.AsyncCallback callback = new System.AsyncCallback(ConnectedCallback);
            client.BeginConnect(serverAddr, portNumber, callback, client);
        }

        private void ConnectedCallback(System.IAsyncResult arg) {
            try {
                System.Net.Sockets.TcpClient c = (System.Net.Sockets.TcpClient)arg.AsyncState;
                c.EndConnect(arg);
                ChangeStatus(WaitStatus.SUCCESS);
            } catch (System.Net.Sockets.SocketException ex) {
                if (ex.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionRefused) {
                    LogInfo(@"ConnectedCallback caught SocketException : Connection refused.");
                } else {
                    LogInfo(@"ConnectedCallback caught SocketException");
                }
                ChangeStatus(WaitStatus.FAILURE);
            } catch (System.Exception ex) {
                LogError(@"ConnectedCallback caught Exception : ");
                LogError(ex.Message + "\r\n" + ex.StackTrace);
                ChangeStatus(WaitStatus.FAILURE);
            }
        }

        #endregion -- C_00_CONNECT --

        #region -- C_10_PREPARE --

        public void PrepareStream() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"PrepareStream")) {
                return;
            }
            LogInfo(@"Client#" + clientId.ToString() + @" : PrepareStream called");
            stream = client.GetStream();
            LogInfo(@"PrepareStream : NetworkStream prepared.");
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- C_10_PREPARE --

        #region -- C_20_SEND --

        public void SendMessage() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"SendMessage")) {
                return;
            }
            LogInfo(@"Client#" + clientId.ToString() + @" : SendMessage called");
            string sendmessage = CommandCurrent().Attr(@"Message").GetNodeValue();
            LogInfo(@"Client#" + clientId.ToString() + @" : message = " + sendmessage);
            byte[] sendbytearray = System.Text.Encoding.UTF8.GetBytes(sendmessage);
            stream.Write(sendbytearray, 0, sendbytearray.Length);
            stream.Flush();
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- C_20_SEND --

        #region -- C_30_READ --

        private byte[] bmessage;

        public void StartReadMessage() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"StartReadMessage")) {
                return;
            }
            LogInfo(@"Client#" + clientId.ToString() + @" : StartReadMessage called");
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
            } catch (System.Exception ex) {
                LogError(@"ReadCallback caught Exception : ");
                LogError(ex.Message + "\r\n" + ex.StackTrace);
                ChangeStatus(WaitStatus.FAILURE);
            }
        }

        #endregion -- C_30_READ --

        #region -- S_50_QUEUEMESSAGE --

        public void QueueMessage() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"QueueMessage")) {
                return;
            }
            string s = System.Text.Encoding.UTF8.GetString(message.ToArray());
            LogInfo(@"Client#" + clientId.ToString() + @" : Message " + s);
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- S_50_QUEUEMESSAGE --

        #region -- C_40_DISCONNECT --

        public void Disconnect() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"Disconnect")) {
                return;
            }
            LogInfo(@"Client#" + clientId.ToString() + @" : Disconnect called");
            try {
                if (stream != null) {
                    stream.Close();
                }
            } catch (System.ObjectDisposedException) {
            }
            try {
                if (client != null) {
                    client.Close();
                }
            } catch (System.ObjectDisposedException) {
            }
            LogInfo(@"Disconnected");
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- C_40_DISCONNECT --

        #region -- C_XX_DISPOSE --

        public void Dispose() {
            if (!CommandCurrent().Attr(@"Method").GetNodeValue().Equals(@"Dispose")) {
                return;
            }
            LogInfo(@"Client#" + clientId.ToString() + @" : Dispose called");
            Disconnect();
            KillTimer();
            LogInfo(@"Disposed");
            ChangeStatus(WaitStatus.SUCCESS);
        }

        #endregion -- C_XX_DISPOSE --
    }
}