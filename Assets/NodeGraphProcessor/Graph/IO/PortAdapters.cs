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
				if (!type.IsSubclassOf(typeof(PortAdapter)))
					continue ;

				Type adapterType = FindAdapterType(type).GetGenericTypeDefinition();
				
				Type[] genericArguments = adapterType.GetGenericArguments();
				
				Debug.Log("adapter type: " + adapterType);

				if (adapterType == typeof(InputPortAdapter<,>))
				{
					inputPortAdapters[genericArguments[0]] = Activator.CreateInstance(type) as PortAdapter;
				}

				if (adapterType == typeof(OutputPortAdapter<,>))
					outputPortAdapters[genericArguments[0]] = Activator.CreateInstance(type) as PortAdapter;
			}
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