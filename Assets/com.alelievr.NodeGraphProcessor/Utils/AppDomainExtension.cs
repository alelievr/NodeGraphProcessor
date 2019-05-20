using System.Collections.Generic;
using System.Collections;
using System;

namespace GraphProcessor
{
	public static class AppDomainExtension
	{
		public static IEnumerable< Type >	GetAllTypes(this AppDomain domain)
		{
            foreach (var assembly in domain.GetAssemblies())
            {
				Type[] types = {};
				
                try {
					types = assembly.GetTypes();
				} catch {
					//just ignore it ...
				}

				foreach (var type in types)
					yield return type;
			}
		}
	}
}
