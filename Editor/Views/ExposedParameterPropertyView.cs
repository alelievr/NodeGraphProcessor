using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;

namespace GraphProcessor
{
	public class ExposedParameterPropertyView : VisualElement
	{
		protected BaseGraphView baseGraphView;

		public ExposedParameter parameter { get; private set; }

		public Toggle     hideInInspector { get; private set; }

		public ExposedParameterPropertyView(BaseGraphView graphView, ExposedParameter param, SerializedProperty serializedParam)
		{
			baseGraphView = graphView;
			parameter      = param;

			var settings = serializedParam.FindPropertyRelative("settings");

			var field = new PropertyField(settings);
			field.Bind(serializedParam.serializedObject);
			Add(field);
		}
	}
} 