using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace GraphProcessor
{
	[Serializable]
	public struct JsonElement
	{
		public string		type;
		public string		jsonDatas;

		public override string ToString()
		{
			return "type: " + type + " | JSON: " + jsonDatas;
		}
	}

	public static class JsonSerializer
	{
		public static JsonElement	Serialize< T >(T obj)
		{
			JsonElement	elem = new JsonElement();

			elem.type = obj.GetType().FullName;
			elem.jsonDatas = JsonUtility.ToJson(obj);

			return elem;
		}

		public static T	Deserialize< T >(JsonElement e)
		{
			if (typeof(T) != Type.GetType(e.type))
				throw new ArgumentException("Deserializing type is not the same than Json element type");
			
			return JsonUtility.FromJson< T >(e.jsonDatas);
		}

		public static BaseNode	DeserializeNode(JsonElement e)
		{
			var baseNodeType = Type.GetType(e.type);

			return JsonUtility.FromJson(e.jsonDatas, baseNodeType) as BaseNode;
		}
	}
}