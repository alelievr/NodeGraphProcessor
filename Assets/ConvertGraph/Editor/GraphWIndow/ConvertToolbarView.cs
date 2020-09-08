namespace Cr7Sund.ConvertGraph
{
    using UnityEngine;
    using GraphProcessor;
    using UnityEditor;
    using Status = UnityEngine.UIElements.DropdownMenuAction.Status;
    using System;
    using System.Collections.Generic;
    using UnityEditor.UIElements;

    public class ConvertGraphToolbarView : ToolbarView
    {
        public ConvertGraphToolbarView(BaseGraphView graphView) : base(graphView) { }
        protected override void AddButtons()
        {
            // Left
            // Add the hello world button on the left of the toolbar
            AddButton("Save", () => ((ConvertGraphView)graphView).Save(), left: false);
            AddButton("Save As ...", () => ((ConvertGraphView)graphView).SaveAsWithCopy(), left: false);
            AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph), false);

            AddButton("Center", graphView.ResetPositionAndZoom);
            bool conditianlProcessorVisible = graphView.GetPinnedElementStatus<ConvertGraphProcessorView>() != Status.Hidden;
            AddToggle("Show Converter Processor", conditianlProcessorVisible, (v) => graphView.ToggleView<ConvertGraphProcessorView>());
        }

    }
}