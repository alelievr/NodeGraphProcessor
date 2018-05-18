using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using System;
using System.Linq;
using System.Reflection;

namespace GraphProcessor
{
	public static class FieldFactory
	{
		static Dictionary< Type, Type >		fieldDrawers = new Dictionary< Type, Type >();

		static FieldFactory()
		{
			foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
			{
				var drawerAttribute = type.GetCustomAttributes(typeof(FieldDrawerAttribute), false).FirstOrDefault() as FieldDrawerAttribute;

				if (drawerAttribute == null)
					continue ;
				
				AddDrawer(drawerAttribute.fieldType, type);
			}

			// щ(ºДºщ) ...
			AddDrawer(typeof(int), typeof(IntegerField));
			AddDrawer(typeof(float), typeof(DoubleField));
			AddDrawer(typeof(double), typeof(DoubleField));
			AddDrawer(typeof(string), typeof(TextField));
			AddDrawer(typeof(Bounds), typeof(BoundsField));
			AddDrawer(typeof(Color), typeof(ColorField));
			AddDrawer(typeof(Vector2), typeof(Vector2Field));
			AddDrawer(typeof(Vector3), typeof(Vector3Field));
			AddDrawer(typeof(Vector4), typeof(Vector4Field));
			AddDrawer(typeof(AnimationCurve), typeof(CurveField));
			AddDrawer(typeof(Enum), typeof(EnumField));
			AddDrawer(typeof(Gradient), typeof(GradientField));
		}

		static void AddDrawer(Type fieldType, Type drawerType)
		{
			if (!drawerType.IsSubclassOf(typeof(INotifyValueChanged<>)))
			{
				Debug.LogError("The custom field drawer " + drawerType + " does not implements INotifyValueChanged< T >");
				return ;
			}

			fieldDrawers[fieldType] = drawerType;
		}

		public static INotifyValueChanged< T > CreateField< T >()
		{
			Type drawerType;

			fieldDrawers.TryGetValue(typeof(T), out drawerType);

			if (drawerType == null)
			{
				throw new ArgumentException("Can't find field drawer for type" + typeof(T));
			}

			var field = Activator.CreateInstance(drawerType);
			return field as INotifyValueChanged< T >;
		}

		public static VisualElement CreateField(FieldInfo field)
		{
			//TODO: Create new field drawer and attach chnage events to this field
			return null;
		}
	}
}