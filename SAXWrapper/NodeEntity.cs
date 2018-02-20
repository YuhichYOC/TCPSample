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

        #region -- Private Fields --

        private string nodeName;

        private int nodeId;

        private int depth;

        private string nodeValue;

        private bool isComment;

        private List<AttributeEntity> attrList;

        private NodeEntity parent;

        private List<NodeEntity> children;

        #region -- Writer --

        private NodeEntity writerSetting;

        private bool newLineAfterOpeningBracket;

        private bool newLineAfterClosingBracket;

        private bool newLineAfterAttributes;

        private bool newLineAfterNodeValue;

        private int indentSize;

        #endregion -- Writer --

        #endregion -- Private Fields --

        #region -- Setter --

        public void SetNodeName(string arg) {
            nodeName = arg;
        }

        public void SetNodeID(int arg) {
            nodeId = arg;
        }

        public void SetDepth(int arg) {
            depth = arg;
        }

        public void SetNodeValue(string arg) {
            nodeValue = arg;
        }

        public void Comment(bool arg) {
            isComment = arg;
        }

        public void SetAttrList(List<AttributeEntity> arg) {
            attrList = arg;
        }

        public void AddAttr(AttributeEntity arg) {
            attrList.Add(arg);
        }

        public void AddAttr(string name, string value) {
            AttributeEntity addAttr = new AttributeEntity();
            addAttr.SetAttrName(name);
            addAttr.SetAttrValue(value);
            attrList.Add(addAttr);
        }

        public void SetParent(NodeEntity arg) {
            parent = arg;
        }

        public void SetChildren(List<NodeEntity> arg) {
            arg.ForEach(c => {
                c.SetParent(this);
            });
            children = arg;
        }

        public void AddChild(NodeEntity arg) {
            arg.SetParent(this);
            children.Add(arg);
        }

        public void AddChild(string name) {
            NodeEntity addNode = new NodeEntity();
            addNode.SetNodeName(name);
            addNode.SetDepth(depth + 1);
            addNode.Comment(false);
            children.Add(addNode);
        }

        public void AddComment() {
            NodeEntity addNode = new NodeEntity();
            addNode.SetNodeName(@"Comment");
            addNode.SetDepth(depth + 1);
            addNode.Comment(true);
            children.Add(addNode);
        }

        #region -- Writer --

        public void SetWriterSetting(NodeEntity arg) {
            writerSetting = arg;
            newLineAfterOpeningBracket = NewLineAfterOpeningBracket();
            newLineAfterClosingBracket = NewLineAfterClosingBracket();
            newLineAfterAttributes = NewLineAfterAttributes();
            newLineAfterNodeValue = NewLineAfterNodeValue();
            indentSize = IndentSize();
            if (children.Count > 0) {
                children.ForEach(c => {
                    c.SetWriterSetting(arg);
                });
            }
        }

        private bool NewLineAfterOpeningBracket() {
            if (writerSetting == null
                || writerSetting.Find(@"Writer") == null
                || writerSetting.Find(@"Writer").Find(@"Setting") == null
                || writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine") == null
                || writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine").Find(@"OpeningBracket") == null) {
                return true;
            }
            if (writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine").Find(@"OpeningBracket").GetNodeValue().Equals(@"YES")) {
                return true;
            } else {
                return false;
            }
        }

        private bool NewLineAfterClosingBracket() {
            if (writerSetting == null
                || writerSetting.Find(@"Writer") == null
                || writerSetting.Find(@"Writer").Find(@"Setting") == null
                || writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine") == null
                || writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine").Find(@"ClosingBracket") == null) {
                return true;
            }
            if (writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine").Find(@"ClosingBracket").GetNodeValue().Equals(@"YES")) {
                return true;
            } else {
                return false;
            }
        }

        private bool NewLineAfterAttributes() {
            if (writerSetting == null
                || writerSetting.Find(@"Writer") == null
                || writerSetting.Find(@"Writer").Find(@"Setting") == null
                || writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine") == null
                || writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine").Find(@"AfterAttrElements") == null) {
                return true;
            }
            if (writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine").Find(@"AfterAttrElements").GetNodeValue().Equals(@"YES")) {
                return true;
            } else {
                return false;
            }
        }

        private bool NewLineAfterNodeValue() {
            if (writerSetting == null
                || writerSetting.Find(@"Writer") == null
                || writerSetting.Find(@"Writer").Find(@"Setting") == null
                || writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine") == null
                || writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine").Find(@"AfterNodeValue") == null) {
                return true;
            }
            if (writerSetting.Find(@"Writer").Find(@"Setting").Find(@"NewLine").Find(@"AfterNodeValue").GetNodeValue().Equals(@"YES")) {
                return true;
            } else {
                return false;
            }
        }

        private int IndentSize() {
            if (writerSetting == null
                || writerSetting.Find(@"Writer") == null
                || writerSetting.Find(@"Writer").Find(@"Setting") == null
                || writerSetting.Find(@"Writer").Find(@"Setting").Find(@"IndentSize") == null) {
                return 2;
            }
            if (int.TryParse(writerSetting.Find(@"Writer").Find(@"Setting").Find(@"IndentSize").GetNodeValue(), out int ret)) {
                return ret;
            } else {
                return 2;
            }
        }

        #endregion -- Writer --

        #endregion -- Setter --

        #region -- Getter --

        public string GetNodeName() {
            return nodeName;
        }

        public int GetNodeID() {
            return nodeId;
        }

        public bool IDExists(int arg) {
            NodeEntity r = Root();
            if (r.GetNodeID() == arg) {
                return true;
            }
            return IDExists(r, arg);
        }

        private bool IDExists(NodeEntity node, int id) {
            bool ret = false;
            foreach (NodeEntity c in node.GetChildren()) {
                if (c.GetNodeID() == id) {
                    ret = true;
                }
                ret = IDExists(c, id);
                if (ret == true) {
                    return true;
                }
            }
            return ret;
        }

        public int TailID() {
            NodeEntity r = Root();
            return TailID(r);
        }

        private int TailID(NodeEntity arg) {
            int ret = arg.GetNodeID();
            int count = arg.GetChildren().Count;
            if (count > 0) {
                ret = TailID(arg.GetChildren()[count - 1]);
            }
            return ret;
        }

        public int GetDepth() {
            return depth;
        }

        public int TailDepth() {
            NodeEntity r = Root();
            return TailDepth(r);
        }

        private int TailDepth(NodeEntity arg) {
            int ret = arg.GetDepth();
            foreach (NodeEntity c in arg.GetChildren()) {
                int cd = TailDepth(c);
                if (ret < cd) {
                    ret = cd;
                }
            }
            return ret;
        }

        public string GetNodeValue() {
            return nodeValue;
        }

        public bool IsComment() {
            return isComment;
        }

        public List<AttributeEntity> GetAttrList() {
            return attrList;
        }

        public bool AttrExists(string name) {
            foreach (AttributeEntity a in attrList) {
                if (a.NameEquals(name)) {
                    return true;
                }
            }
            return false;
        }

        public string AttrByName(string name) {
            foreach (AttributeEntity a in attrList) {
                if (a.NameEquals(name)) {
                    return a.GetAttrValue();
                }
            }
            return string.Empty;
        }

        public NodeEntity GetParent() {
            return parent;
        }

        public NodeEntity Root() {
            if (parent == null) {
                return this;
            }
            return parent.Root();
        }

        public List<NodeEntity> GetChildren() {
            return children;
        }

        #endregion -- Getter --

        public NodeEntity() {
            nodeId = 0;
            depth = 0;
            isComment = false;
            attrList = new List<AttributeEntity>();
            parent = null;
            children = new List<NodeEntity>();
            writerSetting = null;
            newLineAfterOpeningBracket = true;
            newLineAfterClosingBracket = true;
            newLineAfterAttributes = true;
            newLineAfterNodeValue = true;
            indentSize = 2;
        }

        #region -- Public Methods --

        public void RemoveAttrByName(string name) {
            if (AttrExists(name)) {
                for (int i = 0; i < attrList.Count; i++) {
                    if (attrList[i].GetAttrName().Equals(name)) {
                        attrList.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        public bool NameEquals(string name) {
            if (nodeName.Equals(name)) {
                return true;
            } else {
                return false;
            }
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
            ret.SetDepth(depth);
            ret.SetNodeValue(nodeValue);
            ret.Comment(isComment);

            return ret;
        }

        public NodeEntity CloneWithoutChildren() {
            NodeEntity ret = new NodeEntity();

            attrList.ForEach(v => {
                ret.AddAttr(v.Clone());
            });

            ret.SetNodeName(nodeName);
            ret.SetNodeID(nodeId);
            ret.SetDepth(depth);
            ret.SetNodeValue(nodeValue);
            ret.Comment(isComment);

            return ret;
        }

        public void MoveUpByID(int arg) {
            NodeEntity p = FindByID(arg).GetParent();
            if (p == null) {
                return;
            }
            for (int i = 0; i < p.GetChildren().Count; i++) {
                if (p.GetChildren()[i].GetNodeID() == arg) {
                    if (i > 0) {
                        NodeEntity n = p.GetChildren()[i].Clone();
                        p.GetChildren().RemoveAt(i);
                        p.GetChildren().Insert(i - 1, n);
                        Refresh();
                        return;
                    }
                }
            }
        }

        public void MoveDownByID(int arg) {
            NodeEntity p = FindByID(arg).GetParent();
            if (p == null) {
                return;
            }
            for (int i = 0; i < p.GetChildren().Count; i++) {
                if (p.GetChildren()[i].GetNodeID() == arg) {
                    if (i < p.GetChildren().Count - 1) {
                        NodeEntity n = p.GetChildren()[i].Clone();
                        p.GetChildren().RemoveAt(i);
                        p.GetChildren().Insert(i + 1, n);
                        Refresh();
                        return;
                    }
                }
            }
        }

        public void MoveByID(int moveFrom, int moveTo) {
            if (moveFrom == moveTo) {
                return;
            }
            NodeEntity nf = FindByID(moveFrom);
            if (nf == null) {
                return;
            }
            NodeEntity pf = nf.GetParent();
            if (pf == null) {
                return;
            }
            NodeEntity pt = FindByID(moveTo).GetParent();
            if (pt == null) {
                return;
            }
            nf = nf.Clone();
            for (int j = 0; j < pf.GetChildren().Count; j++) {
                if (pf.GetChildren()[j].GetNodeID() == moveFrom) {
                    pf.GetChildren().RemoveAt(j);
                    break;
                }
            }
            for (int i = 0; i < pt.GetChildren().Count; i++) {
                if (pt.GetChildren()[i].GetNodeID() == moveTo) {
                    pt.GetChildren().Insert(i, nf);
                    break;
                }
            }
            Refresh();
        }

        public void RemoveByID(int arg) {
            NodeEntity p = FindByID(arg).GetParent();
            if (p == null) {
                return;
            }
            for (int i = 0; i < p.GetChildren().Count; i++) {
                if (p.GetChildren()[i].GetNodeID() == arg) {
                    p.GetChildren().RemoveAt(i);
                    Refresh();
                    return;
                }
            }
        }

        #region -- Find --

        public NodeEntity Find(string tagName) {
            foreach (NodeEntity c in children) {
                if (c.NameEquals(tagName)) {
                    return c;
                }
            }
            return null;
        }

        public NodeEntity Find(string tagName, string attrName, string attrValue) {
            foreach (NodeEntity c in children) {
                if (c.NameEquals(tagName) && c.AttrExists(attrName) && c.AttrByName(attrName).Equals(attrValue)) {
                    return c;
                }
            }
            return null;
        }

        public NodeEntity Find(string tagName, string attr1Name, string attr1Value, string attr2Name, string attr2Value) {
            foreach (NodeEntity c in children) {
                if (c.NameEquals(tagName)
                    && c.AttrExists(attr1Name)
                    && c.AttrByName(attr1Name).Equals(attr1Value)
                    && c.AttrExists(attr2Name)
                    && c.AttrByName(attr2Name).Equals(attr2Value)) {
                    return c;
                }
            }
            return null;
        }

        public NodeEntity FindByID(int id) {
            NodeEntity r = Root();
            return FindByID(r, id);
        }

        public NodeEntity FindTail(int depth) {
            NodeEntity ret = this;
            if (depth == 1) {
                return ret;
            }
            depth--;
            int count = ret.GetChildren().Count;
            return FindTail(ret.GetChildren()[count - 1], depth);
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

        #endregion -- Find --

        #region -- Writer --

        public override string ToString() {
            string ret = string.Empty;
            if (IsComment()) {
                ret += ToStringComment();
            } else if (children.Count > 0) {
                ret += ToStringStart();
                if (newLineAfterClosingBracket) {
                    ret += "\r\n";
                }
                foreach (NodeEntity item in children) {
                    ret += item.ToString() + "\r\n";
                }
                ret += ToStringEnd();
            } else if (nodeValue != null && !nodeValue.Equals(string.Empty)) {
                ret += ToStringStart();
                if (newLineAfterClosingBracket) {
                    ret += "\r\n";
                }
                ret += Indent(1) + nodeValue;
                if (newLineAfterNodeValue) {
                    ret += "\r\n";
                }
                ret += ToStringEnd();
            } else {
                ret += ToStringEmpty();
            }
            return ret;
        }

        #endregion -- Writer --

        #region -- Refresh --

        private int newDepth;

        private int newNodeId;

        public void Refresh() {
            NodeEntity r = Root();
            newDepth = 0;
            newNodeId = 0;
            r.SetDepth(newDepth);
            r.SetNodeID(newNodeId);
            Refresh(r);
        }

        #endregion -- Refresh --

        #endregion -- Public Methods --

        #region -- Private Methods --

        #region -- Find --

        private NodeEntity FindByID(NodeEntity node, int id) {
            foreach (NodeEntity c in node.GetChildren()) {
                if (c.GetNodeID() == id) {
                    return c;
                }
                NodeEntity ret = FindByID(c, id);
                if (ret != null) {
                    return ret;
                }
            }
            return null;
        }

        private NodeEntity FindTail(NodeEntity node, int depth) {
            if (depth == 1) {
                return node;
            } else {
                depth--;
                int count = node.GetChildren().Count;
                return FindTail(node.GetChildren()[count - 1], depth);
            }
        }

        #endregion -- Find --

        #region -- Writer --

        private string ToStringStart() {
            string ret = Indent(0);
            if (attrList.Count > 0) {
                ret += @"<" + nodeName;
                if (newLineAfterOpeningBracket) {
                    ret += "\r\n";
                }
                foreach (AttributeEntity item in attrList) {
                    ret += Indent(1) + item.ToString();
                    if (newLineAfterAttributes) {
                        ret += "\r\n";
                    }
                }
                ret += Indent(1) + @">";
            } else {
                ret += @"<" + nodeName + @">";
            }
            return ret;
        }

        private string ToStringEnd() {
            string ret = Indent(0);
            ret += @"</" + nodeName + @">";
            return ret;
        }

        private string ToStringEmpty() {
            string ret = Indent(0);
            if (attrList.Count > 0) {
                ret += @"<" + nodeName;
                if (newLineAfterOpeningBracket) {
                    ret += "\r\n";
                }
                foreach (AttributeEntity item in attrList) {
                    ret += Indent(1) + item.ToString();
                    if (newLineAfterAttributes) {
                        ret += "\r\n";
                    }
                }
                ret += Indent(1) + @"/>";
            } else {
                ret += @"<" + nodeName + @"/>";
            }
            return ret;
        }

        private string ToStringComment() {
            string ret = Indent(0);
            ret += @"<!-- " + nodeValue + @" -->";
            return ret;
        }

        private string Indent(int plus) {
            string ret = string.Empty;
            for (int i = 0; i < (depth + plus) * indentSize; i++) {
                ret += @" ";
            }
            return ret;
        }

        #endregion -- Writer --

        #region -- Refresh --

        private void Refresh(NodeEntity node) {
            newDepth++;
            foreach (NodeEntity c in node.GetChildren()) {
                c.SetDepth(newDepth);
                newNodeId++;
                c.SetNodeID(newNodeId);
                if (c.GetChildren().Count > 0) {
                    Refresh(c);
                    newDepth--;
                }
            }
        }

        #endregion -- Refresh --

        #endregion -- Private Methods --
    }
}