using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

namespace GraphProcessor
{
    /// <summary>
    /// Stack node view implementation, can be used to stack multiple node inside a context like VFX graph does.
    /// </summary>
    public class BaseStackNodeView : StackNode
    {
        /// <summary>
        /// StackNode data from the graph
        /// </summary>
        protected internal BaseStackNode    stackNode;
        protected BaseGraphView owner;
        readonly string         styleSheet = "GraphProcessorStyles/BaseStackNodeView";

        public BaseStackNodeView(BaseStackNode stackNode)
        {
            this.stackNode = stackNode;
            styleSheets.Add(Resources.Load<StyleSheet>(styleSheet));
        }

        /// <inheritdoc />
        protected override void OnSeparatorContextualMenuEvent(ContextualMenuPopulateEvent evt, int separatorIndex)
        {
            // TODO: write the context menu for stack node
        }

        /// <summary>
        /// Called after the StackNode have been added to the graph view
        /// </summary>
        public virtual void Initialize(BaseGraphView graphView)
        {
            owner = graphView;
            headerContainer.Add(new Label(stackNode.title));

            SetPosition(new Rect(stackNode.position, Vector2.one));

            InitializeInnerNodes();
        }

        void InitializeInnerNodes()
        {
            // Sanitize the GUID list in case some nodes were removed
            stackNode.nodeGUIDs.RemoveAll(nodeGUID =>
            {
                if (owner.graph.nodesPerGUID.ContainsKey(nodeGUID))
                {
                    var node = owner.graph.nodesPerGUID[nodeGUID];
                    AddElement(owner.nodeViewsPerNode[node]);
                    return false;
                }
                else
                {
                    return true; // remove the entry as the GUID doesn't exist anymore
                }
            });
        }

        /// <inheritdoc />
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            stackNode.position = newPos.position;
        }

        /// <inheritdoc />
        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            bool accept = base.AcceptsElement(element, ref proposedIndex, maxIndex);

            if (accept && element is BaseNodeView nodeView)
            {
                var index = Mathf.Clamp(proposedIndex, 0, stackNode.nodeGUIDs.Count - 1);

                if (stackNode.nodeGUIDs.Contains(nodeView.nodeTarget.GUID))
                    stackNode.nodeGUIDs.Remove(nodeView.nodeTarget.GUID);

                stackNode.nodeGUIDs.Insert(index, nodeView.nodeTarget.GUID);
            }

            return accept;
        }

        public override bool DragLeave(DragLeaveEvent evt, IEnumerable<ISelectable> selection, IDropTarget leftTarget, ISelection dragSource)
        {
            foreach (var elem in selection)
            {
                if (elem is BaseNodeView nodeView)
                    stackNode.nodeGUIDs.Remove(nodeView.nodeTarget.GUID);
            }
            return base.DragLeave(evt, selection, leftTarget, dragSource);
        }
    }
}