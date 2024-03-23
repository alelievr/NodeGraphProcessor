using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    public class ExposedParameterFieldView : BlackboardField
    {
        #region Fields
        protected BaseGraphView graphView;
        #endregion

        #region Properties
        public ExposedParameter parameter
        {
            get;
        }
        #endregion

        #region Constructors
        public ExposedParameterFieldView(BaseGraphView graphView, ExposedParameter param) : base(null, param.name, param.ShortType)
        {
            this.graphView = graphView;
            parameter = param;
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            this.Q("icon").AddToClassList("parameter-" + param.ShortType);
            this.Q("icon").visible = true;

            (this.Q("textField") as TextField).RegisterValueChangedCallback(
                e =>
                {
                    text = e.newValue;
                    graphView.graph.UpdateExposedParameterName(param, e.newValue);
                });
        }
        #endregion

        #region Methods
        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Rename", a => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Delete", a => graphView.graph.RemoveExposedParameter(parameter), DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
        #endregion
    }
}