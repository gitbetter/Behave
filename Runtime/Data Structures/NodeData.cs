using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeData {
    public string id;
    public string parentId;
    public string typeName;
    public string associatedField;
    public Vector2 editorPosition;    
    public List<string> childrenIds = new List<string>();
    public PropertyMap fields = new PropertyMap();

    private BTEditorAttribute editorAttributes;
    private TreeGraph graphData;

    public NodeData(System.Type objectType) {
        this.typeName = objectType.AssemblyQualifiedName;
        Initialize();
    }

    public NodeData() {
        Initialize();
    }

    public void Initialize() {
        id = System.Guid.NewGuid().ToString();
        if (this.typeName != null) {
            id = this.typeName.Substring(0, this.typeName.IndexOf(',')) + "-" + id.Substring(0, id.IndexOf('-', id.IndexOf('-') + 1));
        }
    }

    public void AddChild(string id) {
        if (!childrenIds.Contains(id)) {
            childrenIds.Add(id);
            Save();
        };
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

    public void SetAssociatedField(string field) {
        this.associatedField = field;
        Save();
    }

    public void SetGraphData(TreeGraph graphData) {
        this.graphData = graphData;
    }

    public void Save() {
        Debug.Log("Here");
        if (this.graphData != null) {
            Debug.Log("And here");
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
