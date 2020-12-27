using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace GraphProcessor
{
    [CustomPropertyDrawer(typeof(ExposedParameter))]
    public class ExposedParameterDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create property container element.
            var container = new VisualElement();

            container.Add(CreateValProperty(property));

            return container;
        }

        protected VisualElement CreateValProperty(SerializedProperty property)
        {
            var val = property.FindPropertyRelative("val");
            var name = property.FindPropertyRelative("name").stringValue;

            return new PropertyField(val, name);
        }
    }
}