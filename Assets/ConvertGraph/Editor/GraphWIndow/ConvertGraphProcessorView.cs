namespace Cr7Sund.ConvertGraph
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;
    using GraphProcessor;

    public class ConvertGraphProcessorView : PinnedElementView
    {
        ConvertGraphProcessor processor;
        BaseGraphView graphView;

        public ConvertGraphProcessorView() => title = "Convert Processor";

        protected override void Initialize(BaseGraphView graphView)
        {

            processor = new ConvertGraphProcessor(graphView.graph);
            this.graphView = graphView;

            graphView.computeOrderUpdated += processor.UpdateComputeOrder;

            Button runButton = new Button(OnPlay) { name = "ActionButton", text = "Run" };

            content.Add(runButton);

        }

        void OnPlay() => processor.Run();

    }
}