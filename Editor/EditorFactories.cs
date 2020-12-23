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

    public static Property PropertyFromString(string propName) {
        Property prop;
        switch (propName) {
        case "Int":
            prop = new IntProperty();
            break;
        case "Float":
            prop = new FloatProperty();
            break;
        case "Bool":
            prop = new BoolProperty();
            break;
        case "String":
            prop = new StringProperty();
            break;
        case "Semaphore":
            prop = new SemaphoreProperty();
            break;
        default:
            prop = new IntProperty();
            break;
        }
        return prop;
    }
}
