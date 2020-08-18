namespace Cr7Sund.ConvertGraph
{
    using UnityEngine;
    using GraphProcessor;
    using UnityEditor;
    using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

    public class ConvertGraphToolbarView : ToolbarView
    {
        public ConvertGraphToolbarView(BaseGraphView graphView) : base(graphView) { }

        protected override void AddButtons()
        {

            // Left
            // Add the hello world button on the left of the toolbar
            AddButton("Save As New !", () => Debug.Log("Hello World"), left: false);
            AddButton("Apply For All !", () => graphView.SaveGraphToDisk(), left: false);
            AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph), false);

            AddButton("Center", graphView.ResetPositionAndZoom);
            bool conditianlProcessorVisible = graphView.GetPinnedElementStatus<ConvertGraphProcessorView>() != Status.Hidden;
            AddToggle("Show Converter Processor", conditianlProcessorVisible, (v) => graphView.ToggleView<ConvertGraphProcessorView>());
        }
    }
}