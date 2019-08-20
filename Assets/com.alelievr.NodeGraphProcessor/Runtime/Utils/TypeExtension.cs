using UnityEngine;
using System;
using System.Linq.Expressions;

namespace GraphProcessor
{
	public static class TypeExtension
	{
		public static bool IsReallyAssignableFrom(this Type type, Type otherType)
		{
			if (type.IsAssignableFrom(otherType))
				return true;
			if (otherType.IsAssignableFrom(type))
				return true;

			try
			{
				var v = Expression.Variable(otherType);
				var expr = Expression.Convert(v, type);
				return expr.Method != null && expr.Method.Name != "op_Implicit";
			}
			catch (InvalidOperationException)
			{
				return false;
			}
		}

	}
}