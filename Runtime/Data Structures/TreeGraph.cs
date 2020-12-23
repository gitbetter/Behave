using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BehaviorTree", menuName = "Behavior Tree Editor/Behavior Tree", order = 1)]
public class TreeGraph : ScriptableObject {
    public BehaviorTree Root { get {
        return GenerateBehaviorTree();
    }}
    public Nodetionary nodes = new Nodetionary();

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
                fInfo.SetValue(task, root.fields[fInfo.Name]);
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
}
