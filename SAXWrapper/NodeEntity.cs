/*
*
* NodeEntity.cs
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

using System.Collections.Generic;

namespace SAXWrapper {

    public class NodeEntity {

        #region -- プロパティ --

        private string nodeName;

        public void SetNodeName(string arg) {
            nodeName = arg;
        }

        public string GetNodeName() {
            return nodeName;
        }

        private int nodeId;

        public void SetNodeID(int arg) {
            nodeId = arg;
        }

        public int GetNodeID() {
            return nodeId;
        }

        private string nodeValue;

        public void SetNodeValue(string arg) {
            nodeValue = arg;
        }

        public string GetNodeValue() {
            return nodeValue;
        }

        private List<AttributeEntity> attrList;

        public void SetAttrList(List<AttributeEntity> arg) {
            attrList = arg;
        }

        public List<AttributeEntity> GetAttrList() {
            return attrList;
        }

        public void AddAttr(AttributeEntity arg) {
            attrList.Add(arg);
        }

        private List<NodeEntity> children;

        public void SetChildren(List<NodeEntity> arg) {
            children = arg;
        }

        public List<NodeEntity> GetChildren() {
            return children;
        }

        public void AddChild(NodeEntity arg) {
            children.Add(arg);
        }

        #endregion -- プロパティ --

        public NodeEntity() {
            attrList = new List<AttributeEntity>();
            children = new List<NodeEntity>();
        }

        #region -- メソッド --

        public bool AttrExists(string name) {
            bool ret = false;
            attrList.ForEach(v => {
                if (v.NameEquals(name)) {
                    ret = true;
                }
            });
            return ret;
        }

        public string AttrByName(string name) {
            string ret = @"";
            attrList.ForEach(v => {
                if (v.NameEquals(name)) {
                    ret = v.GetAttrValue();
                }
            });
            return ret;
        }

        public bool NameEquals(string name) {
            if (nodeName.Equals(name)) {
                return true;
            }
            else {
                return false;
            }
        }

        public NodeEntity Find(string tagName) {
            NodeEntity ret = null;
            children.ForEach(v => {
                if (v.NameEquals(tagName)) {
                    ret = v;
                }
            });
            return ret;
        }

        public NodeEntity Find(string tagName, string attrName, string attrValue) {
            NodeEntity ret = null;
            children.ForEach(v => {
                if (v.NameEquals(tagName) && v.AttrExists(attrName) && v.AttrByName(attrName).Equals(attrValue)) {
                    ret = v;
                }
            });
            return ret;
        }

        public NodeEntity Find(string tagName, string attr1Name, string attr1Value, string attr2Name, string attr2Value) {
            NodeEntity ret = null;
            children.ForEach(v => {
                if (v.NameEquals(tagName)
                 && v.AttrExists(attr1Name)
                 && v.AttrByName(attr1Name).Equals(attr1Value)
                 && v.AttrExists(attr2Name)
                 && v.AttrByName(attr2Name).Equals(attr2Value)) {
                    ret = v;
                }
            });
            return ret;
        }

        public NodeEntity FindTail(int depth) {
            NodeEntity ret = this;
            if (depth == 1) { return ret; }
            depth--;
            int count = ret.GetChildren().Count;
            return FindTail(ret.GetChildren()[count - 1], depth);
        }

        public NodeEntity Clone() {
            NodeEntity ret = new NodeEntity();

            attrList.ForEach(v => {
                ret.AddAttr(v.Clone());
            });
            children.ForEach(v => {
                ret.AddChild(v.Clone());
            });

            ret.SetNodeName(nodeName);
            ret.SetNodeID(nodeId);
            ret.SetNodeValue(nodeValue);

            return ret;
        }

        #region -- Derivative Find --

        /// <summary>
        /// 自分自身の子ノードから type が Dir かつ name が引数に一致する最初のノードを返す
        /// </summary>
        /// <param name="name">
        /// 取得したいタグの name 属性値</param>
        /// <returns>
        /// ノード</returns>
        public NodeEntity Dir(string name) {
            return Find(@"Item", @"type", @"Dir", @"name", name);
        }

        /// <summary>
        /// 自分自身の子ノードから type が File かつ name が引数に一致する最初のノードを返す
        /// </summary>
        /// <param name="name">
        /// 取得したいタグの name 属性値</param>
        /// <returns>
        /// ノード</returns>
        public NodeEntity File(string name) {
            return Find(@"Item", @"type", @"File", @"name", name);
        }

        /// <summary>
        /// 自分自身の子ノードから type が Tag かつ name が引数に一致する最初のノードを返す
        /// </summary>
        /// <param name="name">
        /// 取得したいタグの name 属性値</param>
        /// <returns>
        /// ノード</returns>
        public NodeEntity Tag(string name) {
            return Find(@"Item", @"type", @"Tag", @"name", name);
        }

        /// <summary>
        /// 自分自身の子ノードから type が Attr かつ name が引数に一致する最初のノードを返す
        /// </summary>
        /// <param name="name">
        /// 取得したいタグの name 属性値</param>
        /// <returns>
        /// ノード</returns>
        public NodeEntity Attr(string name) {
            return Find(@"Item", @"type", @"Attr", @"name", name);
        }

        /// <summary>
        /// 自分自身の子ノードから name が引数に一致する最初の Category を返す
        /// </summary>
        /// <param name="name">
        /// 取得したいタグの name 属性値</param>
        /// <returns>
        /// ノード</returns>
        public NodeEntity SubCategory(string name) {
            return Find(@"Category", @"name", name);
        }

        /// <summary>
        /// 自分自身の孫ノードから name が引数に一致する最初の Category を返す
        /// </summary>
        /// <param name="par1Name">
        /// 取得したい子タグの name 属性値</param>
        /// <param name="par2Name">
        /// 取得したい孫タグの name 属性値</param>
        /// <returns>
        /// ノード</returns>
        public NodeEntity SubCategory(string par1Name, string par2Name) {
            return Find(@"Category", @"name", par1Name).Find(@"Category", @"name", par2Name);
        }

        /// <summary>
        /// 自分自身の曾孫ノードから name が引数に一致する最初の Category を返す
        /// </summary>
        /// <param name="par1Name">
        /// 取得したい子タグの name 属性値</param>
        /// <param name="par2Name">
        /// 取得したい孫タグの name 属性値</param>
        /// <param name="par3Name">
        /// 取得したい曾孫タグの name 属性値</param>
        /// <returns>
        /// ノード</returns>
        public NodeEntity SubCategory(string par1Name, string par2Name, string par3Name) {
            return Find(@"Category", @"name", par1Name).Find(@"Category", @"name", par2Name).Find(@"Category", @"name", par3Name);
        }

        /// <summary>
        /// 自分自身の子ノードから name が引数に一致する最初の Command を返す
        /// </summary>
        /// <param name="name">
        /// 取得したい子タグの name 属性値</param>
        /// <returns>
        /// ノード</returns>
        public NodeEntity Command(string name) {
            return Find(@"Command", @"name", name);
        }

        /// <summary>
        /// 自分自身の子ノードから name が引数に一致する最初の Param を返す
        /// </summary>
        /// <param name="name">
        /// 取得したい子タグの name 属性値</param>
        /// <returns>
        /// ノード</returns>
        public NodeEntity Param(string name) {
            return Find(@"Param", @"name", name);
        }

        /// <summary>
        /// 自分自身の子ノードから name が引数に一致する最初の Command から name が引数に一致する最初の Param を返す
        /// </summary>
        /// <param name="par1Name">
        /// 取得したい Command の name 属性値</param>
        /// <param name="par2Name">
        /// 取得したい Param の name 属性値</param>
        /// <returns>
        /// ノード</returns>
        public NodeEntity Param(string par1Name, string par2Name) {
            return Command(par1Name).Param(par2Name);
        }

        #endregion -- Derivative Find --

        #endregion -- メソッド --

        #region -- private --

        private NodeEntity FindTail(NodeEntity node, int depth) {
            if (depth == 1) {
                return node;
            }
            else {
                depth--;
                int count = node.GetChildren().Count;
                return FindTail(node.GetChildren()[count - 1], depth);
            }
        }

        #endregion -- private --
    }
}