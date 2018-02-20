/*
*
* XReader.cs
*
* Copyright 2018 Yuichi Yoshii
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

    public class XReader {

        #region -- Private Fields --

        private string directory;

        private string fileName;

        private NodeEntity node;

        private int depth;

        private int currentNodeId;

        #endregion -- Private Fields --

        #region -- Setter --

        public void SetDirectory(string arg) {
            directory = arg;
        }

        public void SetFileName(string arg) {
            fileName = arg;
        }

        #endregion -- Setter --

        #region -- Getter --

        protected string GetFullPath() {
            if (string.IsNullOrEmpty(directory)) {
                throw new ArgumentException(@"Directory is not assigned.");
            }
            if (string.IsNullOrEmpty(fileName)) {
                throw new ArgumentException(@"File is not assigned.");
            }
            return directory + @"\" + fileName;
        }

        public NodeEntity GetNode() {
            return node;
        }

        #endregion -- Getter --

        public XReader() {
            depth = 1;
            currentNodeId = 1;
        }

        #region -- Public Methods --

        public void Parse() {
            System.IO.StreamReader sr = new System.IO.StreamReader(GetFullPath());
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            XmlReader reader = XmlReader.Create(sr, settings);
            try {
                node = new NodeEntity();
                node.SetNodeName(fileName);
                node.SetNodeID(0);
                node.SetDepth(0);
                node.Comment(false);
                while (reader.Read()) {
                    ParseElement(reader);
                    ParseText(reader);
                    ParseCDATA(reader);
                    ParseEndElement(reader);
                    ParseComment(reader);
                }
            } catch (Exception) {
                throw;
            } finally {
                if (reader != null) {
                    reader.Close();
                }
                if (sr != null) {
                    sr.Close();
                }
            }
        }

        protected void ParseElement(XmlReader reader) {
            if (reader.NodeType != XmlNodeType.Element) {
                return;
            }

            string nodeName = reader.Name;
            NodeEntity newNode = new NodeEntity();
            newNode.SetNodeName(nodeName);
            newNode.SetNodeID(currentNodeId);
            newNode.SetDepth(depth);
            newNode.Comment(false);
            currentNodeId++;
            node.FindTail(depth).AddChild(newNode);
            ParseAttributes(reader, newNode);

            if (!reader.IsEmptyElement) {
                depth++;
            }
        }

        protected void ParseText(XmlReader reader) {
            if (reader.NodeType != XmlNodeType.Text) {
                return;
            }

            string value = reader.Value;
            if (!string.IsNullOrEmpty(value)) {
                node.FindTail(depth).SetNodeValue(value.Trim());
            }
        }

        protected void ParseCDATA(XmlReader reader) {
            if (reader.NodeType != XmlNodeType.CDATA) {
                return;
            }

            string value = reader.Value;
            if (!string.IsNullOrEmpty(value)) {
                node.FindTail(depth).SetNodeValue(value.Trim());
            }
        }

        protected void ParseEndElement(XmlReader reader) {
            if (reader.NodeType != XmlNodeType.EndElement) {
                return;
            }

            depth--;
        }

        protected void ParseComment(XmlReader reader) {
            if (reader.NodeType != XmlNodeType.Comment) {
                return;
            }

            NodeEntity newNode = new NodeEntity();
            newNode.SetNodeName(@"Comment");
            newNode.SetNodeID(currentNodeId);
            newNode.SetDepth(depth);
            newNode.SetNodeValue(reader.Value.Trim());
            newNode.Comment(true);
            currentNodeId++;
            node.FindTail(depth).AddChild(newNode);
        }

        #endregion -- Public Methods --

        #region -- Private Methods --

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

        #endregion -- Private Methods --
    }
}