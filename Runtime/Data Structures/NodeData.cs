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
    public AnyMap fields = new AnyMap();

    private BTEditorAttribute editorAttributes;

    public NodeData(System.Type objectType) {
        this.typeName = objectType.AssemblyQualifiedName;
        Initialize();
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
    }

    public void RemoveChild(string id) {
        childrenIds.Remove(id);
    }

    public void SetParentId(string id) {
        parentId = id;
    }

    public void SetField(string id, dynamic val) {
        fields[id] = val;
    }

    public dynamic GetField(string id) {
        if (fields.ContainsKey(id)) {
            return fields[id];
        }
        return null;
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
public class AnyMap : SerializableDictionary<string, dynamic> { }
