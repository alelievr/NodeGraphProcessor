using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GraphProcessor
{
    // We need a drawer to display Vector4 on a single line because by default it's a toggle
    [CustomPropertyDrawer(typeof(Vector4))]
    public class IngredientDrawerUIE : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var vectorField = new Vector4Field() { value = property.vector4Value };
            vectorField.RegisterValueChangedCallback(e => {
                property.vector4Value = e.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            return vectorField;
        }
    }
}