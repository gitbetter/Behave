using System;
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

    public void DragDone() {
        data.SetEditorPosition(data.editorPosition);
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
            if (point is TreeConnectionPoint) {
                point.Draw(this.contentAreaRect);
            }
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
            DragDone();
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

    public virtual ConnectionPoint FirstInPoint(Func<ConnectionPoint, bool> filter = null) {
        return connectionPoints.Where(p => p.type == ConnectionPoint.Type.In && (filter != null ? filter(p) : true)).FirstOrDefault();
    }

    public virtual ConnectionPoint FirstMatchingInPoint(ConnectionPoint point, Func<ConnectionPoint, bool> filter = null) {
        return FirstInPoint(p => p.GetType().IsAssignableFrom(point.GetType()) && (filter != null ? filter(p) : true));
    }

    public virtual ConnectionPoint FirstOutPoint(Func<ConnectionPoint, bool> filter = null) {
        return connectionPoints.Where(p => p.type == ConnectionPoint.Type.Out && (filter != null ? filter(p) : true)).FirstOrDefault();
    }

    public virtual ConnectionPoint FirstMatchingOutPoint(ConnectionPoint point, Func<ConnectionPoint, bool> filter = null) {
        return FirstOutPoint(p => p.GetType().IsAssignableFrom(point.GetType()) && (filter != null ? filter(p) : true));
    }

    public bool HasConnectionPoint(ConnectionPoint point) {
        ConnectionPoint foundPoint = connectionPoints.Where(x => x == point).FirstOrDefault();
        return foundPoint != null;
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
                ConnectionPoint.Type.Out,
                this,
                editableFields[i].Name,
                (float) (i) / (editableFields.Length),
                nodeParams.OnClickOutPoint));
        }
        properties = new Dictionary<string, Property>();
    }

    protected override void DrawContents() {
        // TODO: Draw Task type image, available in the data.editorAttributes.texture field
        for (int i = 0; i < editableFields.Length; i++) {
            GUILayout.BeginHorizontal();
            PropertyConnectionPoint point = (PropertyConnectionPoint) FirstOutPoint(x => x is PropertyConnectionPoint && ((PropertyConnectionPoint)x).field == editableFields[i].Name);
            if (point != null) {
                point.Draw(this.contentAreaRect);
            }
            GUILayout.Label(editableFields[i].Name);
            GUILayout.Button(editableFields[i].FieldType.Name, GUILayout.MaxWidth(150));
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
            ConnectionPoint.Type.In,
            this,
            null,
            0.5f,
            nodeParams.OnClickInPoint));
        this.prop = Property.FromString(this.data.GetEditorAttributes().title);
    }

    protected override void DrawContents() {
        GUILayout.BeginHorizontal();
        prop.Draw();
        PropertyConnectionPoint point = (PropertyConnectionPoint) FirstInPoint(x => x is PropertyConnectionPoint);
        if (point != null) {
            point.Draw(this.contentAreaRect);
        }
        GUILayout.EndHorizontal();
    }

    protected override void ContextMenuLoad(GenericMenu contextMenu) {
        contextMenu.AddItem(new GUIContent("Paste Values"), false, OnClickPasteValues);
    }

    public void SetProperty(Property prop) {
        this.prop = prop;
        SetPropertyFieldName(prop.name);
    }

    public void SetPropertyFieldName(string field) {
        PropertyConnectionPoint inPoint = (PropertyConnectionPoint) FirstInPoint();
        if (inPoint != null) {
            inPoint.field = field;
        }
        this.data.SetAssociatedField(field);
    }

    private void OnClickPasteValues() {
        if (!String.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer)) {
            prop.SetValue(EditorGUIUtility.systemCopyBuffer);
        }
    }
}
