﻿using System;
using UnityEngine;
using UnityEditor;

public class Connection
{
    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;
    public Action<Connection> OnClickRemoveConnection;

    public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> OnClickRemoveConnection) {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
    }

    public virtual void Prepare() { }

    public void Draw() {
        Prepare();
        if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap)) {
            if (OnClickRemoveConnection != null) {
                OnClickRemoveConnection(this);
            }
        }
    }
}

public class TreeConnection : Connection {
    public TreeConnection(TreeConnectionPoint inPoint, TreeConnectionPoint outPoint, Action<Connection> OnClickRemoveConnection)
    : base(inPoint, outPoint, OnClickRemoveConnection) { }

    public override void Prepare() {
        GraphHelpers.DrawLine(inPoint.rect.center, outPoint.rect.center, GraphHelpers.ConnectionStyle.Line);
    }
}

public class PropertyConnection : Connection {
    public PropertyConnection(PropertyConnectionPoint inPoint, PropertyConnectionPoint outPoint, Action<Connection> OnClickRemoveConnection)
    : base(inPoint, outPoint, OnClickRemoveConnection) {
        if (outPoint.field != null) {
            PropertyNode propertyNode = inPoint.node as PropertyNode;
            TreeNode treeNode = outPoint.node as TreeNode;
            if (treeNode.data.GetField(outPoint.field) != null) {
                propertyNode.SetProperty(Property.FromSavedProperty(treeNode.data.GetField(outPoint.field)));
            } else {
                propertyNode.SetPropertyFieldName(outPoint.field);
            }
            treeNode.SetProperty(outPoint.field, propertyNode.prop);
        }
    }

    public override void Prepare() {
        GraphHelpers.DrawLine(outPoint.rect.center, inPoint.rect.center, GraphHelpers.ConnectionStyle.Bezier);
    }
}
