using System;
using UnityEngine;
using System.Collections.Generic;

namespace GraphProcessor
{
	public static class PortAdapters
	{
		static Dictionary< Type, List< Type > >	assignableTypes = new Dictionary< Type, List< Type > >();
		static Dictionary< Type, PortAdapter > inputPortAdapters = new Dictionary< Type, PortAdapter>();
		static Dictionary< Type, PortAdapter > outputPortAdapters = new Dictionary< Type, PortAdapter >();

		static PortAdapters()
		{
			LoadPortAdapters();
		}

		static void LoadPortAdapters()
		{
			foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
			{
				if (type.IsAbstract || type.ContainsGenericParameters)
					continue ;
				if (!(type.IsSubclassOf(typeof(PortAdapter))))
					continue ;

				Type adapterType = FindAdapterType(type);
				Type genericAdapterType = adapterType.GetGenericTypeDefinition();
				
				Type[] genericArguments = adapterType.GetGenericArguments();
				
				if (genericAdapterType == typeof(InputPortAdapter<,>))
				{
					inputPortAdapters[genericArguments[0]] = Activator.CreateInstance(type) as PortAdapter;
					AddAssignableTypes(genericArguments[0], genericArguments[1]);
					AddAssignableTypes(genericArguments[1], genericArguments[0]);
				}

				if (genericAdapterType == typeof(OutputPortAdapter<,>))
					outputPortAdapters[genericArguments[0]] = Activator.CreateInstance(type) as PortAdapter;
			}
		}

		static void AddAssignableTypes(Type fromType, Type toType)
		{
			if (!assignableTypes.ContainsKey(fromType))
				assignableTypes[fromType] = new List< Type >();

			assignableTypes[fromType].Add(toType);
		}

		static Type FindAdapterType(Type t)
		{
			for (int i = 0; i < 10; i++)
			{
				if (t.BaseType == typeof(PortAdapter))
					break ;
				t = t.BaseType;
			}

			return t;
		}

		public static bool IsAssignable(Type input, Type output)
		{
			if (assignableTypes.ContainsKey(input))
				return assignableTypes[input].Contains(output);
			return false;
		}
	}
}