using System;
using UnityEngine;

public class ConnectionPoint
{
    public enum Type { In, Out }

    public Rect rect;
    public Type type;
    public Node node;
    public Action<ConnectionPoint> OnClickConnectionPoint;

    protected GUIStyle style;

    public ConnectionPoint(Type type, Node node, Action<ConnectionPoint> OnClickConnectionPoint) {
        this.type = type;
        this.node = node;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
    }

    public void Draw(Rect inRect) {    
        Prepare(inRect);
        if (GUI.Button(rect, "", style)) {
            if (OnClickConnectionPoint != null) {
                OnClickConnectionPoint(this);
            }
        }
    }

    protected virtual void Prepare(Rect inRect) { }
}

public class TreeConnectionPoint : ConnectionPoint {
    public TreeConnectionPoint(Type type, Node node, Action<ConnectionPoint> OnClickConnectionPoint) : base (type, node, OnClickConnectionPoint) {
        rect = new Rect(0, 0, 75f, 10f);
        if (this.type == Type.In) {
            this.style = this.node.skin.GetStyle("TreeInPoint");
        } else {
            this.style = this.node.skin.GetStyle("TreeOutPoint");
        }
    }

    protected override void Prepare(Rect inRect) {
        rect.x = node.rect.x + (node.rect.width * 0.5f) - rect.width * 0.5f;

        switch (type) {
        case Type.In:
            rect.y = node.rect.y - rect.height * 0.85f;
            break;
        case Type.Out:
            rect.y = node.rect.y + node.rect.height - rect.height * 0.15f;
            break;
        }
    }
}

public class PropertyConnectionPoint : ConnectionPoint {
    public float yOffsetPct;
    public string field;
    public PropertyConnectionPoint(Type type, Node node, string field, float yOffsetPct, Action<ConnectionPoint> OnClickConnectionPoint) : base (type, node, OnClickConnectionPoint) {
        rect = new Rect(0, 0, 20f, 20f);
        if (this.type == Type.In) {
            this.style = this.node.skin.GetStyle("PropertyInPoint");
        } else {
            this.style = this.node.skin.GetStyle("PropertyOutPoint");
        }
        this.yOffsetPct = yOffsetPct;
        this.field = field;
    }

    protected override void Prepare(Rect inRect)
    {
        rect.y = inRect.y;
        
        switch (type) {
        case Type.In:
            rect.x = inRect.x - rect.width;
            break;
        case Type.Out:
            rect.x = inRect.x + inRect.width;
            break;
        }
    }
}
