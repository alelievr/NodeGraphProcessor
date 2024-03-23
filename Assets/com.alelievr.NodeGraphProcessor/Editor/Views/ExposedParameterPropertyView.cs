using UnityEngine.UIElements;

namespace GraphProcessor
{
    public class ExposedParameterPropertyView : VisualElement
    {
        #region Fields
        protected BaseGraphView baseGraphView;
        #endregion

        #region Properties
        public ExposedParameter parameter { get; private set; }
        
        #endregion

        #region Constructors
        public ExposedParameterPropertyView(BaseGraphView graphView, ExposedParameter param)
        {
            baseGraphView = graphView;
            parameter = param;

            var field = graphView.exposedParameterFactory.GetParameterSettingsField(
                param,
                newValue =>
                {
                    param.settings = newValue as ExposedParameter.Settings;
                });

            Add(field);
        }
        #endregion
    }
}