using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GraphProcessor;
using NodeGraphProcessor.Examples;

public class ConditionalProcessorView : PinnedElementView
{
    ConditionalProcessor	processor;
    BaseGraphView           graphView;

    public ConditionalProcessorView() => title = "Conditional Processor";

    protected override void Initialize(BaseGraphView graphView)
    {
        processor = new ConditionalProcessor(graphView.graph);
        this.graphView = graphView;

        graphView.computeOrderUpdated += processor.UpdateComputeOrder;

        Button runButton = new Button(OnPlay) { name = "ActionButton", text = "Run" };
        Button stepButton = new Button(OnStep) { name = "ActionButton", text = "Step" };

        content.Add(runButton);
        content.Add(stepButton);
    }

    void OnPlay() => processor.Run();

    void OnStep()
    {
        BaseNodeView view;

        if (processor.currentGraphExecution != null)
        {
            // Unhighlight the last executed node
            view = graphView.nodeViews.Find(v => v.nodeTarget == processor.currentGraphExecution.Current);
            view.UnHighlight();
        }

        processor.Step();

        // Display debug infos, currentGraphExecution is modified in the Step() function above
        if (processor.currentGraphExecution != null)
        {
            view = graphView.nodeViews.Find(v => v.nodeTarget == processor.currentGraphExecution.Current);
            view.Highlight();
        }
    }
}
