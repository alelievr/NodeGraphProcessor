using System;
using UnityEngine;
using System.Collections.Generic;

namespace GraphProcessor
{
	public static class PortAdapters
	{
		static Dictionary< Type, List< Type > >	assignableTypes = new Dictionary< Type, List< Type > >();
		static Dictionary< Type, PortAdapter > inputPortAdapters = new Dictionary< Type, PortAdapter>();

		static PortAdapters()
		{
			LoadPortAdapters();
		}

		static void LoadPortAdapters()
		{
			foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
			{
				
			}
		}
	}
}