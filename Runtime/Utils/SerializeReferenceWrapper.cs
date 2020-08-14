using UnityEngine;

namespace GraphProcessor
{
    /// <summary>
    /// This class allows to serialize both UnityEngine.Object and abstract classes with the SerializeReference attribute
    /// </summary>
    [System.Serializable]
    public struct SerializeReferenceWrapper
    {
        [SerializeReference]
        System.Object       referenceValue;
        [SerializeField]
        UnityEngine.Object  unityObjectValue;

        public object value
        {
            get => referenceValue ?? unityObjectValue;
            set
            {
                if (value is UnityEngine.Object uo)
                {
                    unityObjectValue = uo;
                    referenceValue = null;
                    Debug.Log(unityObjectValue);
                }
                else
                {
                    unityObjectValue = null;
                    referenceValue = value;
                }
            }
        }

        public SerializeReferenceWrapper(object value)
        {
            unityObjectValue = null;
            referenceValue = null;

            this.value = value;
        }
    }
}