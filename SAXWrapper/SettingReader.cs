/*
*
* SettingReader.cs
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
using System.Xml;

namespace SAXWrapper {

    public class SettingReader {

        #region -- プロパティ --

        private string directory;

        public void SetDirectory(string arg) {
            directory = arg;
        }

        private string fileName;

        public void SetFileName(string arg) {
            fileName = arg;
        }

        private string GetFullPath() {
            if (string.IsNullOrEmpty(directory)) {
                throw new ArgumentException(@"Directory is not assigned.");
            }
            if (string.IsNullOrEmpty(fileName)) {
                throw new ArgumentException(@"File is not assigned.");
            }
            return directory + @"\" + fileName;
        }

        private NodeEntity node;

        public NodeEntity GetNode() {
            return node;
        }

        private int depth;

        private int currentNodeId;

        #endregion -- プロパティ --

        public SettingReader() {
            depth = 0;
            currentNodeId = 0;
        }

        #region -- メソッド --

        public void Parse() {
            System.IO.StreamReader sr = new System.IO.StreamReader(GetFullPath());
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader reader = XmlReader.Create(sr, settings);
            try {
                while (reader.Read()) {
                    ParseElement(reader);
                    ParseText(reader);
                    ParseCDATA(reader);
                    ParseEndElement(reader);
                }
            }
            catch (Exception) {
                throw;
            }
            finally {
                if (reader != null) {
                    reader.Close();
                }
                if (sr != null) {
                    sr.Close();
                }
            }
        }

        private void ParseElement(XmlReader reader) {
            if (reader.NodeType != XmlNodeType.Element) { return; }

            string nodeName = reader.Name;
            NodeEntity newNode = new NodeEntity();
            newNode.SetNodeName(nodeName);
            newNode.SetNodeID(currentNodeId);
            currentNodeId++;
            ParseAttributes(reader, newNode);
            if (currentNodeId == 1) {
                node = newNode;
            }
            else {
                node.FindTail(depth).AddChild(newNode);
            }
            if (!reader.IsEmptyElement) {
                depth++;
            }
        }

        private void ParseText(XmlReader reader) {
            if (reader.NodeType != XmlNodeType.Text) { return; }

            string value = reader.Value;
            if (!string.IsNullOrEmpty(value)) {
                node.FindTail(depth).SetNodeValue(value);
            }
        }

        private void ParseCDATA(XmlReader reader) {
            if (reader.NodeType != XmlNodeType.CDATA) { return; }

            string value = reader.Value;
            if (!string.IsNullOrEmpty(value)) {
                node.FindTail(depth).SetNodeValue(value);
            }
        }

        private void ParseEndElement(XmlReader reader) {
            if (reader.NodeType != XmlNodeType.EndElement) { return; }
            depth--;
        }

        #endregion -- メソッド --

        #region -- private --

        private void ParseAttributes(XmlReader reader, NodeEntity currentNode) {
            int iLoopCount = reader.AttributeCount;
            for (int i = 0; i < iLoopCount; i++) {
                reader.MoveToAttribute(i);
                string attrName = reader.LocalName;
                string attrValue = reader.GetAttribute(attrName);
                AttributeEntity attr = new AttributeEntity();
                attr.SetAttrName(attrName);
                attr.SetAttrValue(attrValue);
                currentNode.AddAttr(attr);
            }
        }

        #endregion -- private --
    }
}