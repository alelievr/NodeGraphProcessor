namespace Cr7Sund.ConvertGraph
{
    using GraphProcessor;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    [NodeCustomEditor(typeof(SubGraphNode))]
    public class SubGraphNodeView : GraphNodeView
    {
        private SubGraphNode subGraphNode;


        public override void Enable()
        {
            subGraphNode = nodeTarget as SubGraphNode;
            var objectField = new ObjectField("SubGraph");
            objectField.value = subGraphNode.SubConvertGraph;
            // objectField.SetEnabled(false);

            objectField.RegisterCallback<MouseDownEvent>((evt) =>
            {
                Debug.Log("Don't try to change me");
                var subGraphWindow = EditorWindow.GetWindow<SubGraphWindow>();
                subGraphWindow.InitializeGraph(subGraphNode.SubConvertGraph);
            });

            controlsContainer.Add(objectField);
        }
    }
}