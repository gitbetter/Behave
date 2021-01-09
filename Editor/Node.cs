﻿using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public struct NodeParams {
    public Vector2 position;
    public Vector2 dimensions;
    public GUISkin skin;
    public NodeData data;
    public Action<ConnectionPoint> OnClickInPoint;
    public Action<ConnectionPoint> OnClickOutPoint;
}

public class Node
{
    public Rect rect;
    public string title;
    public bool isDragged;
    public bool isSelected;

    public List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();

    public GUISkin skin;

    public Action<Node> OnRemoveNode;

    public NodeData data;

    protected Rect contentAreaRect;

    protected GUIStyle currentStyle;
    protected GUIStyle defaultNodeStyle;
    protected GUIStyle selectedNodeStyle;

    public Node(NodeParams nodeParams) {
        rect = new Rect(nodeParams.data.editorPosition.x, nodeParams.data.editorPosition.y, nodeParams.dimensions.x, nodeParams.dimensions.y);
        this.skin = nodeParams.skin;
        this.data = nodeParams.data;
        this.contentAreaRect = new Rect(rect.x + 25, rect.y + 35f, rect.width - 50, rect.height - 35f);
    }

    public void Drag(Vector2 delta) {
        rect.position += delta;
        contentAreaRect.position += delta;
        data.editorPosition += delta;
    }

    public void Draw() {
        BTEditorAttribute editorAttributes = data.GetEditorAttributes();
        GUI.Box(rect, editorAttributes.title, currentStyle);
        if (editorAttributes.texturePath != null) {
            //GUI.Box(rect, data.editorAttributes.texture);
        }
        GUILayout.BeginArea(this.contentAreaRect);
        DrawContents();
        GUILayout.EndArea();
        foreach (ConnectionPoint point in connectionPoints) {
            point.Draw(this.contentAreaRect);
        }
    }

    public bool ProcessEvents(Event e) {
        switch (e.type) {
        case EventType.MouseDown:
            if (e.button == 0) {
                if (rect.Contains(e.mousePosition)) {
                    isDragged = true;
                    isSelected = true;
                    GUI.changed = true;
                    currentStyle = selectedNodeStyle;
                } else {
                    GUI.changed = false;
                    isSelected = false;
                    currentStyle = defaultNodeStyle;
                }
            }
            if (e.button == 1 && rect.Contains(e.mousePosition)) {
                ProcessContextMenu();
                e.Use();
            }
            break;

        case EventType.MouseUp:
            isDragged = false;
            break;

        case EventType.MouseDrag:
            if (e.button == 0 && isDragged) {
                Drag(e.delta);
                e.Use();
                return true;
            }
            break;
        }
        return false;
    }

    public ConnectionPoint FirstInPoint() {
        foreach (ConnectionPoint p in connectionPoints) {
            if (p.type == ConnectionPoint.Type.In)
                return p;
        }
        return null;
    }

    public ConnectionPoint FirstMatchingInPoint(ConnectionPoint point) {
        foreach (ConnectionPoint p in connectionPoints) {
            if (p.type == ConnectionPoint.Type.In && p.GetType().IsAssignableFrom(point.GetType()))
                return p;
        }
        return null;
    }

    public ConnectionPoint FirstOutPoint() {
        foreach (ConnectionPoint p in connectionPoints) {
            if (p.type == ConnectionPoint.Type.Out)
                return p;
        }
        return null;
    }

    public ConnectionPoint FirstMatchingOutPoint(ConnectionPoint point) {
        foreach (ConnectionPoint p in connectionPoints) {
            if (p.type == ConnectionPoint.Type.Out && p.GetType().IsAssignableFrom(point.GetType()))
                return p;
        }
        return null;
    }

    public bool HasConnectionPoint(ConnectionPoint point) {
        foreach (ConnectionPoint p in connectionPoints) {
            if (point == p)
                return true;
        }
        return false;
    }

    private void ProcessContextMenu() {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove Node"), false, OnClickRemoveNode);
        ContextMenuLoad(genericMenu);
        genericMenu.ShowAsContext();
    }

    private void OnClickRemoveNode() {
        if (OnRemoveNode != null) {
            OnRemoveNode(this);
        }
    }

    protected virtual void ContextMenuLoad(GenericMenu contextMenu) { }

    protected virtual void DrawContents() { }
}

public class TreeNode : Node {
    public Dictionary<string, Property> properties;

    FieldInfo[] editableFields;

    public TreeNode(NodeParams nodeParams) : base(nodeParams) {
        this.defaultNodeStyle = this.skin.GetStyle("DefaultTreeNode");
        this.selectedNodeStyle = this.skin.GetStyle("SelectedTreeNode");
        this.currentStyle = this.defaultNodeStyle;
        this.connectionPoints.Add(new TreeConnectionPoint(ConnectionPoint.Type.In, this, nodeParams.OnClickInPoint));
        this.connectionPoints.Add(new TreeConnectionPoint(ConnectionPoint.Type.Out, this, nodeParams.OnClickOutPoint));

        editableFields = Type.GetType(this.data.typeName).GetFields().Where(
            field => Attribute.IsDefined(field, typeof(BTEditableAttribute))
        ).ToArray();
        for (int i = 0; i < editableFields.Length; i++) {
            this.connectionPoints.Add(new PropertyConnectionPoint(
                ConnectionPoint.Type.In,
                this,
                editableFields[i].Name,
                (float) (i + 1) / (editableFields.Length + 1),
                nodeParams.OnClickInPoint));
        }
        properties = new Dictionary<string, Property>();
    }

    protected override void DrawContents() {
        // TODO: Draw Task type image, available in the data.editorAttributes.texture field
        for (int i = 0; i < editableFields.Length; i++) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(editableFields[i].Name);
            GUILayout.Button(properties.ContainsKey(editableFields[i].Name) ? properties[editableFields[i].Name].GetType().FullName : "None", GUILayout.MaxWidth(150));
            GUILayout.EndHorizontal();
        }
    }

    public void SetProperty(string propName, Property prop) {
        prop.name = propName;
        properties[prop.name] = prop;
        this.data.SetField(prop.name, prop);
    }

    public void UnsetProperty(Property prop) {
        this.data.SetField(prop.name, null);
        properties.Remove(prop.name);
        prop.name = null;
    }
}

public class PropertyNode : Node {
    public Property prop;
    
    public PropertyNode(NodeParams nodeParams) : base(nodeParams) {
        this.defaultNodeStyle = this.skin.GetStyle("DefaultPropertyNode");
        this.selectedNodeStyle = this.skin.GetStyle("SelectedPropertyNode");
        this.currentStyle = this.defaultNodeStyle;
        this.connectionPoints.Add(new PropertyConnectionPoint(
            ConnectionPoint.Type.Out,
            this,
            null,
            0.5f,
            nodeParams.OnClickOutPoint));
        this.prop = Property.FromString(this.data.GetEditorAttributes().title);
    }

    protected override void DrawContents() {
        GUILayout.BeginHorizontal();
        if (prop is IntProperty) {
            IntProperty intProperty = prop as IntProperty;
            GUILayout.Label("value");
            intProperty.SetValue<int>(EditorGUILayout.IntField(intProperty.GetValue(), GUILayout.MaxWidth(150)));
        } else if (prop is FloatProperty) {
            FloatProperty floatProperty = prop as FloatProperty;
            GUILayout.Label("value");
            floatProperty.SetValue<float>(EditorGUILayout.FloatField(floatProperty.GetValue(), GUILayout.MaxWidth(150)));
        } else if (prop is BoolProperty) {
            BoolProperty boolProperty = prop as BoolProperty;
            GUILayout.Label("true");
            boolProperty.SetValue<bool>(EditorGUILayout.Toggle(boolProperty.GetValue(), GUILayout.MaxWidth(150)));
        } else if (prop is StringProperty) {
            StringProperty stringProperty = prop as StringProperty;
            GUILayout.Label("text");
            stringProperty.SetValue<string>(EditorGUILayout.TextField(stringProperty.GetValue(), GUILayout.MaxWidth(150)));
        } else if (prop is Vector3Property) {
            Vector3Property vector3Property = prop as Vector3Property;
            vector3Property.SetValue<Vector3>(EditorGUILayout.Vector3Field("", vector3Property.GetValue(), GUILayout.MaxWidth(150)));
        } else if (prop is SemaphoreProperty) {
            
        }
        GUILayout.EndHorizontal();
    }

    protected override void ContextMenuLoad(GenericMenu contextMenu) {
        contextMenu.AddItem(new GUIContent("Paste Values"), false, OnClickPasteValues);
    }

    private void OnClickPasteValues() {
        if (!String.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer)) {
            prop.SetValue(EditorGUIUtility.systemCopyBuffer);
        }
    }
}
