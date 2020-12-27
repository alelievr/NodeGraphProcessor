using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace GraphProcessor
{
    public class GraphInspector : Editor
    {
        protected VisualElement root;
        protected BaseGraph     graph;

        VisualElement   parameterContainer;

        protected virtual void OnEnable()
        {
            graph = target as BaseGraph;
            graph.onExposedParameterListChanged += UpdateExposedParameters;
            graph.onExposedParameterModified += UpdateExposedParameters;
        }

        protected virtual void OnDisable()
        {
            graph.onExposedParameterListChanged -= UpdateExposedParameters;
            graph.onExposedParameterModified -= UpdateExposedParameters;
        }

        public sealed override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            CreateInspector();
            return root;
        }

        protected virtual void CreateInspector()
        {
            parameterContainer = new VisualElement{
                name = "ExposedParameters"
            };
            FillExposedParameters(parameterContainer);

            root.Add(parameterContainer);
        }

        protected void FillExposedParameters(VisualElement parameterContainer)
        {
            if (graph.exposedParameters.Count != 0)
                parameterContainer.Add(new Label("Exposed Parameters:"));
            
            var exposedParameterDrawers = TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>();

            // foreach (var drawerType in exposedParameterDrawers)
            // {
            //     var drawers = drawerType.GetCustomAttributes<CustomPropertyDrawer>();

            //     foreach (var drawer in drawers)
            //     {
            //         drawer.Match.
            //     }
            // }

            SerializedObject so = new SerializedObject(graph);

            // foreach (var param in graph.exposedParameters)
            {
                var exposedParameters = so.FindProperty(nameof(graph.exposedParameters));

                for (int i = 0; i < exposedParameters.arraySize; i++)
                {
                    var param = exposedParameters.GetArrayElementAtIndex(i);
                    var value = graph.exposedParameters[i];
                    if(value.settings.isHidden)
                        continue;

                    // PropertyField f = new PropertyField(param);
                    // var name = param.FindPropertyRelative("name").stringValue;

                    var e = new PropertyField(param);

                    // param.type.

                    // var im = new IMGUIContainer(() => {
                    //     EditorGUIUtility.wideMode = true;
                    //     EditorGUI.BeginChangeCheck();
                    //     EditorGUILayout.PropertyField(param, new GUIContent(name), true);
                    //     if (EditorGUI.EndChangeCheck())
                    //     {
                    //         so.ApplyModifiedProperties();
                    //     }
                    // });
                    // var im = new PropertyField(param.FindPropertyRelative("name"), "HUFIWH");
                    // var referenceField = new PropertyField(property.FindPropertyRelative("value.referenceValue"), name);
                    // var objectField = new PropertyField(property.FindPropertyRelative("value.unityObjectValue"), name);

                    // prop.style.display = DisplayStyle.Flex;
                    parameterContainer.Add(e);
                }
                // PropertyField field = param.value.value;
                // VisualElement prop = new VisualElement();
                // prop.style.display = DisplayStyle.Flex;
                // Type paramType = Type.GetType(param.type);
                // var field = FieldFactory.CreateField(paramType, param.value.value, (newValue) => {
				// 	Undo.RegisterCompleteObjectUndo(graph, "Changed Parameter " + param.name + " to " + newValue);
                //     param.value.value = newValue;
                // }, param.name);
                // prop.Add(field);
                // parameterContainer.Add(prop);
            }
        }

        void UpdateExposedParameters(string guid) => UpdateExposedParameters();
		
        void UpdateExposedParameters()
        {
            parameterContainer.Clear();
            FillExposedParameters(parameterContainer);
        }

        // Don't use ImGUI
        public sealed override void OnInspectorGUI() {}

    }
}