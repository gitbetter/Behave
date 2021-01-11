using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BehaviorTree", menuName = "Behavior Tree Editor/Behavior Tree", order = 1)]
public class TreeGraph : ScriptableObject {
    public Nodetionary nodes = new Nodetionary();
    
    public BehaviorTree Root { get {
        return GenerateBehaviorTree();
    }}

    BehaviorTree GenerateBehaviorTree() {
        NodeData rootNode = null;
        foreach (KeyValuePair<string, NodeData> entry in nodes) {
            NodeData nodeData = entry.Value;
            System.Type objectType = System.Type.GetType(nodeData.typeName);
            if (System.String.IsNullOrEmpty(nodeData.parentId) && (objectType.IsSubclassOf(typeof(Task)) || objectType == typeof(Task))) {
                rootNode = nodeData;
                break;
            }
        }

        if (rootNode != null) {
            Task root = GenerateTask(rootNode);
            return new BehaviorTree(root);
        }
        return new BehaviorTree();
    }

    Task GenerateTask(NodeData root) {
        System.Type rootObjectType = System.Type.GetType(root.typeName);
        Task task = (Task) System.Activator.CreateInstance(rootObjectType);
        FieldInfo[] fieldInfos = rootObjectType.GetFields();
        foreach (FieldInfo fInfo in fieldInfos) {
            if(root.fields.ContainsKey(fInfo.Name)) {
                fInfo.SetValue(task, root.fields[fInfo.Name].GetValue());
            }
        }
        foreach (string id in root.childrenIds) {
            NodeData node = nodes[id];
            System.Type nodeObjectType = System.Type.GetType(node.typeName);
            if (nodeObjectType.IsSubclassOf(typeof(Task)) || nodeObjectType == typeof(Task)) {
                task.AddChild(GenerateTask(node));
            }
        }
        return task;
    }

    public void AddNodeData(NodeData data) {
        if (!this.nodes.ContainsKey(data.id)) {
            this.nodes.Add(data.id, data);
            data.SetGraphData(this);
            Save();
        }
    }

    public void RemoveNodeData(NodeData data) {
        this.nodes.Remove(data.id);
        data.SetGraphData(null);
        Save(); 
    }

    public void Save() {
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    }
}
