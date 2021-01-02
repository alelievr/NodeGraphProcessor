using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;

namespace GraphProcessor
{
    // So, this is a workaround class to add a wrapper around PropertyFields applied on [SerializeReference].
    // Because Property Fields binding being extremely slow (https://forum.unity.com/threads/propertyfield-extremely-slow.966191/)
    // and AppliedModifiedProperties() re-creating the ScriptableObject when called (which in NGP causes the graph to be re-built)
    // we can't use PropertyFields directly. This class provides a set of function to create PropertyFields for Exposed Parameters
    // but without being attached to the graph, so when we call AppliedModifiedProperties, the graph is not re-built.
    // The drawback is that we have to check ourselves for value changes and then apply them on the graph parameters,
    // but it's far better than having to re-create the graph every time a parameter or a setting is changed.
    public class ExposedParameterFieldFactory : IDisposable
    {
        [Serializable]
        public class ExposedParameterWorkaround : ScriptableObject
        {
            [SerializeReference]
            public List<ExposedParameter>   parameters = new List<ExposedParameter>();
            public BaseGraph                graph;

            public ExposedParameterWorkaround()
            {
                Debug.Log("re-create workaround");
            }
        }

        BaseGraph graph;
        [SerializeField]
        ExposedParameterWorkaround  exposedParameterObject;
        SerializedObject            serializedObject;
        SerializedProperty          serializedParameters;

        public ExposedParameterFieldFactory(BaseGraph graph)
        {
            this.graph = graph;

            exposedParameterObject = ScriptableObject.CreateInstance<ExposedParameterWorkaround>();
            exposedParameterObject.graph = graph;
            serializedObject = new SerializedObject(exposedParameterObject);
            serializedParameters = serializedObject.FindProperty(nameof(ExposedParameterWorkaround.parameters));
            UpdateSerializedProperties();
        }

        void UpdateSerializedProperties()
        {
            exposedParameterObject.parameters = graph.exposedParameters;
            serializedObject.Update();
            serializedParameters = serializedObject.FindProperty(nameof(ExposedParameterWorkaround.parameters));
            Debug.Log(serializedParameters.arraySize);
        }

        public PropertyField GetParameterValueField(ExposedParameter parameter, Action<object> valueChangedCallback)
        {
            serializedObject.Update();
            int propIndex = FindPropertyIndex(parameter);
            var field = new PropertyField(serializedParameters.GetArrayElementAtIndex(propIndex));

            field.schedule.Execute(() => {
                if (!parameter.value.Equals(exposedParameterObject.parameters[propIndex].value))
                    valueChangedCallback(exposedParameterObject.parameters[propIndex].value);
            }).Every(50);
            field.Bind(serializedObject);

            return field;
        }

        int FindPropertyIndex(ExposedParameter param) => exposedParameterObject.parameters.FindIndex(p => p == param);

        public void Dispose()
        {
            GameObject.DestroyImmediate(exposedParameterObject);
        }
    }
}