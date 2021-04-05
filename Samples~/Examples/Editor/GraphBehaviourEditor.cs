using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(GraphBehaviour))]
public class GraphBehaviourEditor : Editor
{
    Editor graphEditor;
    GraphBehaviour behaviour => target as GraphBehaviour;

    void OnEnable()
    {
        graphEditor = Editor.CreateEditor(behaviour.graph);
    }

    void OnDisable()
    {
        DestroyImmediate(graphEditor);
    }

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var graphContainer = graphEditor != null ? graphEditor.CreateInspectorGUI().Q("ExposedParameters") : null;

        root.Add(new Button(() => EditorWindow.GetWindow<AllGraphWindow>().InitializeGraph(behaviour.graph))
        {
            text = "Open"
        });

        root.Add(graphContainer);

        return root;
    }
}