using System;
using System.Collections.Generic;
using UnityEngine;

public static class EditorFactories
{
    public static Node NodeForType(System.Type type, NodeParams nodeParams) {
        Node newNode;
        if (type.IsSubclassOf(typeof(Task)) || type == typeof(Task)) {
            newNode = new TreeNode(nodeParams);
        } else if (type.IsSubclassOf(typeof(Property)) || type == typeof(Property)) {
            newNode = new PropertyNode(nodeParams);
        } else {
            newNode = new Node(nodeParams);
        }
        return newNode;
    }
}
