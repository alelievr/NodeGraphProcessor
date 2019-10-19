using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;

public class ConditionalProcessorView : PinnedElementView
{
    BaseGraphProcessor	processor;

    public ConditionalProcessorView() => title = "Conditional Processor";

    protected override void Initialize(BaseGraphView graphView)
    {
        processor = new ProcessGraphProcessor(graphView.graph);

        graphView.computeOrderUpdated += processor.UpdateComputeOrder;

        Button	b = new Button(OnPlay) { name = "ActionButton", text = "Play !" };

        content.Add(b);
    }

    void OnPlay() => processor.Run();
}
