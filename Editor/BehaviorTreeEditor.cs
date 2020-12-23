using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class BehaviorTreeEditor : EditorWindow {
    private List<Node> nodes;
    private List<Connection> connections;

    private GUISkin skin;

    private ConnectionPoint selectedInPoint;
    private ConnectionPoint selectedOutPoint;

    private Vector2 drag;
    private Vector2 offset;

    private System.Type[] nodeTypes;

    private TreeGraph graphData;


    [MenuItem("Window/Behavior Tree Editor")]
    private static void ShowWindow() {
        var window = GetWindow<BehaviorTreeEditor>();
        window.titleContent = new GUIContent("BehaviorTreeEditor Editor");
    }

    private void OnEnable() {
        skin = (GUISkin) Resources.Load("GraphSkin");

        System.Type[] taskTypes = System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(Task));
        System.Type[] propertyTypes = System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(Property));
        nodeTypes = new System.Type[taskTypes.Length + propertyTypes.Length];
        System.Array.Copy(taskTypes, nodeTypes, taskTypes.Length);
        System.Array.Copy(propertyTypes, 0, nodeTypes, taskTypes.Length, propertyTypes.Length);
    }

    private void OnGUI() {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);
        DrawNodes();
        DrawConnections();
        DrawConnectionLine(Event.current);
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);
        if (GUI.changed) Repaint();
    }

    private void DrawGrid(float spacing, float opacity, Color color) {
        int widthDivs = Mathf.CeilToInt(position.width / spacing);
        int heightDivs = Mathf.CeilToInt(position.height / spacing);

        Handles.BeginGUI();
        Handles.color = new Color(color.r, color.g, color.b, opacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % spacing, offset.y % spacing, 0);

        for (int i = 0; i < widthDivs; i++) {
            Handles.DrawLine(new Vector3(spacing * i, -spacing, 0) + newOffset, new Vector3(spacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++) {
            Handles.DrawLine(new Vector3(-spacing, spacing * j, 0) + newOffset, new Vector3(position.width, spacing * j, 0) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes() {
        if (nodes != null) {
            foreach (Node node in nodes) {
                node.Draw();
            }
        }
    }

    private void DrawConnections() {
        if (connections != null) {
            List<Connection> connectionsToDraw = new List<Connection>(connections);
            foreach (Connection connection in connectionsToDraw) {
                connection.Draw();
            }
        }
    }

    private void DrawConnectionLine(Event e) {
        if (selectedInPoint != null && selectedOutPoint == null) {
            GraphHelpers.DrawLine(selectedInPoint.rect.center, e.mousePosition, GraphHelpers.ConnectionStyle.Line);
        }

        if (selectedOutPoint != null && selectedInPoint == null) {
            GraphHelpers.DrawLine(selectedOutPoint.rect.center, e.mousePosition, GraphHelpers.ConnectionStyle.Line);
        }
    }

    private void ProcessEvents(Event e) {
        drag = Vector2.zero;

        switch (e.type) {
        case EventType.MouseDown:
            if (e.button == 0) {
                if ((selectedOutPoint != null && selectedInPoint == null) ||
                    (selectedInPoint != null && selectedOutPoint == null)) {
                    ProcessContextMenu(e.mousePosition);
                } else {
                    ClearConnectionSelection();
                }
            }

            if (e.button == 1) {
                ProcessContextMenu(e.mousePosition);
            }
            break;
            
        case EventType.MouseDrag:
            if (e.button == 0) {
                OnDrag(e.delta);
            }
            break;
        }
    }

    private void ProcessNodeEvents(Event e) {
        if (nodes != null) {
            foreach (Node node in nodes) {
                bool guiChanged = node.ProcessEvents(e);
                if (guiChanged) {
                    GUI.changed = true;
                }
            }
        }
    }

    private void ProcessContextMenu(Vector2 position) {
        GenericMenu genericMenu = new GenericMenu();
        foreach (System.Type type in nodeTypes) {
            NodeData data = new NodeData(type);
            data.editorPosition = position;
            genericMenu.AddItem(new GUIContent("Add Node/" + data.GetEditorAttributes().menuPath), false, () => OnClickAddNode(data));
        }
        genericMenu.ShowAsContext();
    }

    private void OnClickAddNode(NodeData data) {
        CreateNode(data);
    }

    private void OnClickInPoint(ConnectionPoint inPoint) {
        selectedInPoint = inPoint;
        if (selectedOutPoint != null) {
            if (selectedOutPoint.node != selectedInPoint.node) {
                CreateConnection(selectedInPoint, selectedOutPoint);
            }
            ClearConnectionSelection();
        }
    }

    private void OnClickOutPoint(ConnectionPoint outPoint) {
        selectedOutPoint = outPoint;
        if (selectedInPoint != null) {
            if (selectedOutPoint.node != selectedInPoint.node) {
                CreateConnection(selectedInPoint, selectedOutPoint);
            }
            ClearConnectionSelection();
        }
    }

    private void OnClickRemoveConnection(Connection connection) {
        DeleteConnection(connection);
    }

    private void OnClickRemoveNode(Node node) {
        DeleteNode(node);
    }

    private void OnDrag(Vector2 delta) {
        drag = delta;

        if (nodes != null) {
            foreach (Node node in nodes) {
                node.Drag(delta);
            }
        }

        GUI.changed = true;
    }

    private Node CreateNode(NodeData data) {
        if (nodes == null) {
            nodes = new List<Node>();
        }
        NodeParams nodeParams = new NodeParams() {
            dimensions = new Vector2(200, 90),
            skin = skin,
            data = data,
            OnClickInPoint = OnClickInPoint,
            OnClickOutPoint = OnClickOutPoint
        };
        Node newNode = EditorFactories.NodeForType(Type.GetType(data.typeName), nodeParams);       
        newNode.OnRemoveNode = OnClickRemoveNode;
        nodes.Add(newNode);

        if (graphData != null && !graphData.nodes.ContainsKey(data.id)) {
            graphData.nodes.Add(data.id, data);
        }

        if (selectedOutPoint != null && selectedInPoint == null) {
            selectedInPoint = newNode.FirstMatchingInPoint(selectedOutPoint);
            CreateConnection(selectedInPoint, selectedOutPoint);
        } else if (selectedInPoint != null && selectedOutPoint == null) {
            selectedOutPoint = newNode.FirstMatchingOutPoint(selectedInPoint);
            CreateConnection(selectedInPoint, selectedOutPoint);
        }
        ClearConnectionSelection();

        return newNode;
    }

    private void DeleteNode(Node node) {
        if (connections != null) {
            List<Connection> connectionsToRemove = new List<Connection>();
            foreach (Connection connection in connections) {
                if (node.HasConnectionPoint(connection.inPoint) || node.HasConnectionPoint(connection.outPoint)) {
                    connectionsToRemove.Add(connection);
                }
            }
            foreach (Connection connection in connectionsToRemove) {
                DeleteConnection(connection);
            }
            connectionsToRemove = null;
        }

        nodes.Remove(node);

        if (graphData != null) {
            graphData.nodes.Remove(node.data.id);
        }
    }

    private void CreateConnection(ConnectionPoint inPoint, ConnectionPoint outPoint) {
        if (outPoint == null || inPoint == null) {
            return;
        }
        if (connections == null) {
            connections = new List<Connection>();
        }
        outPoint.node.data.AddChild(inPoint.node.data.id);
        inPoint.node.data.SetParentId(outPoint.node.data.id);

        if (inPoint is TreeConnectionPoint && outPoint is TreeConnectionPoint) {
            connections.Add(new TreeConnection(inPoint as TreeConnectionPoint, outPoint as TreeConnectionPoint, OnClickRemoveConnection));
        } else if (inPoint is PropertyConnectionPoint && outPoint is PropertyConnectionPoint) {
            connections.Add(new PropertyConnection(inPoint as PropertyConnectionPoint, outPoint as PropertyConnectionPoint, OnClickRemoveConnection));
        }
    }

    private void DeleteConnection(Connection connection) {
        connection.outPoint.node.data.RemoveChild(connection.inPoint.node.data.id);
        connection.inPoint.node.data.SetParentId(null);
        connections.Remove(connection);
    }

    private void ClearConnectionSelection() {
        selectedInPoint = null;
        selectedOutPoint = null;
    }

    public void SetGraphData(TreeGraph graphData) {
        this.graphData = graphData;
        Dictionary<string, Node> created = new Dictionary<string, Node>();
        foreach (KeyValuePair<string, NodeData> entry in graphData.nodes) {
            if (!created.ContainsKey(entry.Key)) {
                created.Add(entry.Key, CreateNode(entry.Value)); 
            }
            Node currentNode = created[entry.Key];

            foreach (string id in entry.Value.childrenIds) {
                if (!created.ContainsKey(id)) {
                    selectedOutPoint = currentNode.FirstOutPoint();
                    NodeData childData = graphData.nodes[id];
                    created.Add(id, CreateNode(childData));
                } else {
                    ConnectionPoint outPoint = currentNode.FirstOutPoint();
                    ConnectionPoint inPoint = created[id].FirstMatchingInPoint(outPoint);
                    CreateConnection(inPoint, outPoint);
                }
            }
        }
    }

    [OnOpenAssetAttribute(1)]
    public static bool EditorOpen(int instanceID, int line) {
        UnityEngine.Object graphDataObject = EditorUtility.InstanceIDToObject(instanceID);
        if (graphDataObject is TreeGraph) {
            TreeGraph graphData = (TreeGraph) graphDataObject;
            BehaviorTreeEditor editor = (BehaviorTreeEditor) EditorWindow.GetWindow(typeof(BehaviorTreeEditor), false, graphData.name);
            editor.SetGraphData(graphData);
            editor.Show();
            return true;
        }
        return false;
    }
}
