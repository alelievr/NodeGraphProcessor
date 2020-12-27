using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
	[Serializable]
	public class SerializableType : ISerializationCallbackReceiver
	{
		static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
		static Dictionary<Type, string> typeNameCache = new Dictionary<Type, string>();

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
			{
				if (!typeCache.TryGetValue(serializedType, out type))
				{
					type = Type.GetType(serializedType);
					typeCache[serializedType] = type;
				}
			}
        }

        public void OnBeforeSerialize()
        {
			if (type != null)
			{
				if (!typeNameCache.TryGetValue(type, out serializedType))
				{
					serializedType = type.AssemblyQualifiedName;
					typeNameCache[type] = serializedType;
				}
			}
        }
    }
}