using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using System.Reflection;
using System.Globalization;

namespace GraphProcessor
{
	[Obsolete("Field Factory is not necessary anymore. You can use a SerializedProperty directly instead.")]
	public static class FieldFactory
	{
		static readonly Dictionary< Type, Type >    fieldDrawers = new Dictionary< Type, Type >();

		static readonly MethodInfo	        		createFieldMethod = typeof(FieldFactory).GetMethod("CreateFieldSpecific", BindingFlags.Static | BindingFlags.Public);

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
            AddDrawer(typeof(bool), typeof(Toggle));
            AddDrawer(typeof(int), typeof(IntegerField));
            AddDrawer(typeof(long), typeof(LongField));
            AddDrawer(typeof(float), typeof(FloatField));
			AddDrawer(typeof(double), typeof(DoubleField));
			AddDrawer(typeof(string), typeof(TextField));
			AddDrawer(typeof(Bounds), typeof(BoundsField));
			AddDrawer(typeof(Color), typeof(ColorField));
			AddDrawer(typeof(Vector2), typeof(Vector2Field));
			AddDrawer(typeof(Vector2Int), typeof(Vector2IntField));
			AddDrawer(typeof(Vector3), typeof(Vector3Field));
			AddDrawer(typeof(Vector3Int), typeof(Vector3IntField));
			AddDrawer(typeof(Vector4), typeof(Vector4Field));
			AddDrawer(typeof(AnimationCurve), typeof(CurveField));
			AddDrawer(typeof(Enum), typeof(EnumField));
			AddDrawer(typeof(Gradient), typeof(GradientField));
			AddDrawer(typeof(UnityEngine.Object), typeof(ObjectField));
			AddDrawer(typeof(Rect), typeof(RectField));
		}

		static void AddDrawer(Type fieldType, Type drawerType)
		{
			var iNotifyType = typeof(INotifyValueChanged<>).MakeGenericType(fieldType);

			if (!iNotifyType.IsAssignableFrom(drawerType))
			{
				Debug.LogWarning("The custom field drawer " + drawerType + " does not implements INotifyValueChanged< " + fieldType + " >");
				return ;
			}

			fieldDrawers[fieldType] = drawerType;
		}

		public static INotifyValueChanged< T > CreateField< T >(T value, string label = null)
		{
			return CreateField(value != null ? value.GetType() : typeof(T), label) as INotifyValueChanged< T >;
		}

		public static VisualElement CreateField(Type t, string label)
		{
			Type drawerType;

			fieldDrawers.TryGetValue(t, out drawerType);

			if (drawerType == null)
				drawerType = fieldDrawers.FirstOrDefault(kp => kp.Key.IsReallyAssignableFrom(t)).Value;

			if (drawerType == null)
			{
				Debug.LogWarning("Can't find field drawer for type: " + t);
				return null;
			}

			// Call the constructor that have a label
			object field;
			
			if (drawerType == typeof(EnumField))
			{
				field = new EnumField(label, Activator.CreateInstance(t) as Enum);
			}
			else
			{
				try {
					field = Activator.CreateInstance(drawerType,
						BindingFlags.CreateInstance |
						BindingFlags.Public |
						BindingFlags.NonPublic |
						BindingFlags.Instance | 
						BindingFlags.OptionalParamBinding, null,
						new object[]{ label, Type.Missing }, CultureInfo.CurrentCulture);
				} catch {
					field = Activator.CreateInstance(drawerType,
						BindingFlags.CreateInstance |
						BindingFlags.Public |
						BindingFlags.NonPublic |
						BindingFlags.Instance | 
						BindingFlags.OptionalParamBinding, null,
						new object[]{ label }, CultureInfo.CurrentCulture);
				}
			}

			// For mutiline
			switch (field)
			{
				case TextField textField:
					textField.multiline = true;
					break;
				case ObjectField objField:
					objField.allowSceneObjects = true;
					objField.objectType = typeof(UnityEngine.Object);
					break;
			}
			
			return field as VisualElement;
		}

		public static INotifyValueChanged< T > CreateFieldSpecific< T >(T value, Action< object > onValueChanged, string label)
		{
			var fieldDrawer = CreateField< T >(value, label);

			if (fieldDrawer == null)
				return null;

			fieldDrawer.value = value;
			fieldDrawer.RegisterValueChangedCallback((e) => {
				onValueChanged(e.newValue);
			});

			return fieldDrawer as INotifyValueChanged< T >;
		}

		public static VisualElement CreateField(Type fieldType, object value, Action< object > onValueChanged, string label)
		{
			if (typeof(Enum).IsAssignableFrom(fieldType))
				fieldType = typeof(Enum);

			VisualElement field = null;

			// Handle special cases here
			if (fieldType == typeof(LayerMask))
			{
				// LayerMasks inherit from INotifyValueChanged<int> instead of INotifyValueChanged<LayerMask>
				// so we can't register it inside our factory system :(
				var layerField = new LayerMaskField(label, ((LayerMask)value).value);
				layerField.RegisterValueChangedCallback(e => {
					onValueChanged(new LayerMask{ value = e.newValue});
				});

				field = layerField;
			}
			else
			{
				try
				{
					var createFieldSpecificMethod = createFieldMethod.MakeGenericMethod(fieldType);
					try
					{
						field = createFieldSpecificMethod.Invoke(null, new object[]{value, onValueChanged, label}) as VisualElement;
					} catch {}

					// handle the Object field case
					if (field == null && (value == null || value is UnityEngine.Object))
					{
						createFieldSpecificMethod = createFieldMethod.MakeGenericMethod(typeof(UnityEngine.Object));
						field = createFieldSpecificMethod.Invoke(null, new object[]{value, onValueChanged, label}) as VisualElement;
						if (field is ObjectField objField)
						{
							objField.objectType = fieldType;
							objField.value = value as UnityEngine.Object;
						}
					}
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}

			return field;
		}
	}
}