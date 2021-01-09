using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeData {
    public string id;
    public string parentId;
    public string typeName;
    public Vector2 editorPosition;    
    public List<string> childrenIds = new List<string>();
    public PropertyMap fields = new PropertyMap();

    private BTEditorAttribute editorAttributes;
    private TreeGraph graphData;

    public NodeData(System.Type objectType) : this() {
        this.typeName = objectType.AssemblyQualifiedName;
    }

    public NodeData() {
        Initialize();
    }

    public void Initialize() {
        id = System.Guid.NewGuid().ToString();
    }

    public void AddChild(string id) {
        if (childrenIds.Contains(id)) return;
        childrenIds.Add(id);
        Save();
    }

    public void RemoveChild(string id) {
        childrenIds.Remove(id);
        Save();
    }

    public void SetParentId(string id) {
        parentId = id;
        Save();
    }

    public void SetField(string id, Property val) {
        if (val == null && fields.ContainsKey(id)) {
            fields.Remove(id);
        } else {
            fields[id] = val;
        }
        Save();
    }

    public Property GetField(string id) {
        if (fields.ContainsKey(id)) {
            return fields[id];
        }
        return null;
    }

    public void SetEditorPosition(Vector2 pos) {
        this.editorPosition = pos;
        Save();
    }

    public void SetGraphData(TreeGraph graphData) {
        this.graphData = graphData;
    }

    public void Save() {
        if (this.graphData != null) {
            this.graphData.Save();
        }
    }

    public BTEditorAttribute GetEditorAttributes() {
        if (this.editorAttributes == null) {
            this.editorAttributes = (BTEditorAttribute) System.Attribute.GetCustomAttribute(System.Type.GetType(this.typeName), typeof(BTEditorAttribute));
        }
        return this.editorAttributes;
    }
}

[System.Serializable]
public class Nodetionary : SerializableDictionary<string, NodeData> { }

[System.Serializable]
public class PropertyMap : SerializableDictionary<string, Property> { }
