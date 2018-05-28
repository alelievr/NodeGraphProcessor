using System;
using UnityEngine;
using System.Collections.Generic;

namespace GraphProcessor
{
	public static class GraphIO
	{
		static Dictionary< Type, List< Type > >	assignableTypes = new Dictionary< Type, List< Type > >();

		static GraphIO()
		{
			foreach (Type type in AppDomain.CurrentDomain.GetAllTypes())
			{
				if (!type.BaseType.IsGenericType)
					continue ;
				
				Type[] genericArguments = type.BaseType.GetGenericArguments();

				if (type.IsSubclassOf(typeof(DataPull<,>)))
					AddAssignableType(genericArguments[0], genericArguments[1]);
				if (type.IsSubclassOf(typeof(MultiDataPull<,>)))
					AddAssignableType(genericArguments[0], genericArguments[1]);
				if (type.IsSubclassOf(typeof(DataPush<,>)))
					AddAssignableType(genericArguments[1], genericArguments[0]);
			}
		}

		static void AddAssignableType(Type fromType, Type toType)
		{
			if (assignableTypes.ContainsKey(fromType))
				assignableTypes[fromType] = new List< Type >();
			
			assignableTypes[fromType].Add(toType);
		}

		public static bool IsAssignable(Type input, Type output)
		{
			if (assignableTypes.ContainsKey(input))
				return assignableTypes[input].Contains(output);
			return false;
		}

		public static IDataPull GetPullMethod(Type type)
		{
			return null;
		}

		public static IMultiDataPull GetMultiPullMethod(Type type)
		{
			return null;
		}

		public static IDataPush GetPushMethod(Type type)
		{
			return null;
		}

	}
}