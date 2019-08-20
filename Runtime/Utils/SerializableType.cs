using System;
using UnityEngine;

namespace GraphProcessor
{
	[Serializable]
	public class SerializableType : ISerializationCallbackReceiver
	{
		[SerializeField]
		public string	serializedType;

		[NonSerialized]
		public Type		type;

		public SerializableType(Type t)
		{
			type = t;
		}

        public void OnAfterDeserialize()
        {
			if (!String.IsNullOrEmpty(serializedType))
				type = Type.GetType(serializedType);
        }

        public void OnBeforeSerialize()
        {
			if (type != null)
				serializedType = type.AssemblyQualifiedName;
        }
    }
}