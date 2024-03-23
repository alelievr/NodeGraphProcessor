using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
    [Serializable]
    public abstract class ExposedParameter : ISerializationCallbackReceiver
    {
        #region Nested type: Settings
        [Serializable]
        public class Settings
        {
            #region Fields
            public bool isHidden;
            public bool expanded;

            [SerializeField]
            internal string guid;
            #endregion

            #region Methods
            public override bool Equals(object obj)
            {
                if (obj is Settings s)
                {
                    return Equals(s);
                }
                return false;
            }

            public virtual bool Equals(Settings param)
            {
                return isHidden == param.isHidden && expanded == param.expanded;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            #endregion
        }
        #endregion

        #region Fields
        private static Dictionary<Type, Type> exposedParameterTypeCache = new();

        public string guid; // unique id to keep track of the parameter
        public string name;
        [Obsolete("Use GetValueType()")]
        public string type;
        [Obsolete("Use value instead")]
        public SerializableObject serializedValue;
        public bool input = true;
        [SerializeReference]
        public Settings             settings;
        #endregion

        #region Properties
        public virtual string ShortType => ValueType.Name;

        public virtual object Value { get; set; }
        public abstract Type ValueType { get; }
        #endregion

        #region ISerializationCallbackReceiver Members
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // SerializeReference migration step:
#pragma warning disable CS0618
            if (serializedValue?.value != null) // old serialization system can't serialize null values
            {
                Value = serializedValue.value;
                Debug.Log("Migrated: " + serializedValue.value + " | " + serializedValue.serializedName);
                serializedValue.value = null;
            }
#pragma warning restore CS0618
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        #endregion

        #region Methods
        public void Initialize(string name, object value)
        {
            guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter
            settings = CreateSettings();
            settings.guid = guid;
            this.name = name;
            this.Value = value;
        }

        protected virtual Settings CreateSettings()
        {
            return new Settings();
        }

        internal ExposedParameter Migrate()
        {
            if (exposedParameterTypeCache.Count == 0)
            {
                foreach (var curType in AppDomain.CurrentDomain.GetAllTypes())
                {
                    if (curType.IsSubclassOf(typeof(ExposedParameter)) && !curType.IsAbstract)
                    {
                        var paramType = Activator.CreateInstance(curType) as ExposedParameter;
                        exposedParameterTypeCache[paramType.ValueType] = curType;
                    }
                }
            }
#pragma warning disable CS0618 // Use of obsolete fields
            var oldType = Type.GetType(type);
#pragma warning restore CS0618
            if (oldType == null || !exposedParameterTypeCache.TryGetValue(oldType, out var newParamType))
            {
                return null;
            }

            var newParam = Activator.CreateInstance(newParamType) as ExposedParameter;

            newParam.guid = guid;
            newParam.name = name;
            newParam.input = input;
            newParam.settings = newParam.CreateSettings();
            newParam.settings.guid = guid;

            return newParam;
        }

        public static bool operator ==(ExposedParameter param1, ExposedParameter param2)
        {
            if (ReferenceEquals(param1, null) && ReferenceEquals(param2, null))
            {
                return true;
            }
            if (ReferenceEquals(param1, param2))
            {
                return true;
            }
            if (ReferenceEquals(param1, null))
            {
                return false;
            }
            if (ReferenceEquals(param2, null))
            {
                return false;
            }

            return param1.Equals(param2);
        }

        public static bool operator !=(ExposedParameter param1, ExposedParameter param2)
        {
            return !(param1 == param2);
        }

        public bool Equals(ExposedParameter parameter)
        {
            return guid == parameter.guid;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            return Equals((ExposedParameter)obj);
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }

        public ExposedParameter Clone()
        {
            var clonedParam = Activator.CreateInstance(GetType()) as ExposedParameter;

            clonedParam.guid = guid;
            clonedParam.name = name;
            clonedParam.input = input;
            clonedParam.settings = settings;
            clonedParam.Value = Value;

            return clonedParam;
        }
        #endregion
    }

    // Due to polymorphic constraints with [SerializeReference] we need to explicitly create a class for
    // every parameter type available in the graph (i.e. templating doesn't work)
    [Serializable]
    public class ColorParameter : ExposedParameter
    {
        #region Nested type: ColorSettings
        [Serializable]
        public class ColorSettings : Settings
        {
            #region Fields
            public ColorMode mode;
            #endregion

            #region Methods
            public override bool Equals(Settings param)
            {
                return base.Equals(param) && mode == ((ColorSettings)param).mode;
            }
            #endregion
        }
        #endregion

        #region ColorMode enum
        public enum ColorMode
        {
            Default,
            HDR
        }
        #endregion

        #region Fields
        [SerializeField] private Color val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Color)value; }
        public override Type ValueType => typeof(Color);
        #endregion

        #region Methods
        protected override Settings CreateSettings()
        {
            return new ColorSettings();
        }
        #endregion
    }

    [Serializable]
    public class FloatParameter : ExposedParameter
    {
        #region Nested type: FloatSettings
        [Serializable]
        public class FloatSettings : Settings
        {
            #region Fields
            public FloatMode mode;
            public float min  ;
            public float max = 1;
            #endregion

            #region Methods
            public override bool Equals(Settings param)
            {
                return base.Equals(param) && mode == ((FloatSettings)param).mode && min == ((FloatSettings)param).min && max == ((FloatSettings)param).max;
            }
            #endregion
        }
        #endregion

        #region FloatMode enum
        public enum FloatMode
        {
            Default,
            Slider
        }
        #endregion

        #region Fields
        [SerializeField] private float val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (float)value; }
        public override Type ValueType => typeof(float);
        #endregion

        #region Methods
        protected override Settings CreateSettings()
        {
            return new FloatSettings();
        }
        #endregion
    }

    [Serializable]
    public class Vector2Parameter : ExposedParameter
    {
        #region Nested type: Vector2Settings
        [Serializable]
        public class Vector2Settings : Settings
        {
            #region Fields
            public Vector2Mode mode;
            public float min  ;
            public float max = 1;
            #endregion

            #region Methods
            public override bool Equals(Settings param)
            {
                return base.Equals(param) && mode == ((Vector2Settings)param).mode && min == ((Vector2Settings)param).min && max == ((Vector2Settings)param).max;
            }
            #endregion
        }
        #endregion

        #region Vector2Mode enum
        public enum Vector2Mode
        {
            Default,
            MinMaxSlider
        }
        #endregion

        #region Fields
        [SerializeField] private Vector2 val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Vector2)value; }
        public override Type ValueType => typeof(Vector2);
        #endregion

        #region Methods
        protected override Settings CreateSettings()
        {
            return new Vector2Settings();
        }
        #endregion
    }

    [Serializable]
    public class Vector3Parameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private Vector3 val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Vector3)value; }
        public override Type ValueType => typeof(Vector3);
        #endregion
    }

    [Serializable]
    public class Vector4Parameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private Vector4 val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Vector4)value; }
        public override Type ValueType => typeof(Vector4);
        #endregion
    }

    [Serializable]
    public class IntParameter : ExposedParameter
    {
        #region Nested type: IntSettings
        [Serializable]
        public class IntSettings : Settings
        {
            #region Fields
            public IntMode mode;
            public int min  ;
            public int max = 10;
            #endregion

            #region Methods
            public override bool Equals(Settings param)
            {
                return base.Equals(param) && mode == ((IntSettings)param).mode && min == ((IntSettings)param).min && max == ((IntSettings)param).max;
            }
            #endregion
        }
        #endregion

        #region IntMode enum
        public enum IntMode
        {
            Default,
            Slider
        }
        #endregion

        #region Fields
        [SerializeField] private int val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (int)value; }
        public override Type ValueType => typeof(int);
        #endregion

        #region Methods
        protected override Settings CreateSettings()
        {
            return new IntSettings();
        }
        #endregion
    }

    [Serializable]
    public class Vector2IntParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private Vector2Int val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Vector2Int)value; }
        public override Type ValueType => typeof(Vector2Int);
        #endregion
    }

    [Serializable]
    public class Vector3IntParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private Vector3Int val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Vector3Int)value; }
        public override Type ValueType => typeof(Vector3Int);
        #endregion
    }

    [Serializable]
    public class DoubleParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private double val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (double)value; }
        public override Type ValueType => typeof(double);
        #endregion
    }

    [Serializable]
    public class LongParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private long val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (long)value; }
        public override Type ValueType => typeof(long);
        #endregion
    }

    [Serializable]
    public class StringParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private string val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (string)value; }
        public override Type ValueType => typeof(string);
        #endregion
    }

    [Serializable]
    public class RectParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private Rect val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Rect)value; }
        public override Type ValueType => typeof(Rect);
        #endregion
    }

    [Serializable]
    public class RectIntParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private RectInt val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (RectInt)value; }
        public override Type ValueType => typeof(RectInt);
        #endregion
    }

    [Serializable]
    public class BoundsParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private Bounds val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Bounds)value; }
        public override Type ValueType => typeof(Bounds);
        #endregion
    }

    [Serializable]
    public class BoundsIntParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private BoundsInt val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (BoundsInt)value; }
        public override Type ValueType => typeof(BoundsInt);
        #endregion
    }

    [Serializable]
    public class AnimationCurveParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private AnimationCurve val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (AnimationCurve)value; }
        public override Type ValueType => typeof(AnimationCurve);
        #endregion
    }

    [Serializable]
    public class GradientParameter : ExposedParameter
    {
        #region Nested type: GradientSettings
        [Serializable]
        public class GradientSettings : Settings
        {
            #region Fields
            public GradientColorMode mode;
            #endregion

            #region Methods
            public override bool Equals(Settings param)
            {
                return base.Equals(param) && mode == ((GradientSettings)param).mode;
            }
            #endregion
        }
        #endregion

        #region GradientColorMode enum
        public enum GradientColorMode
        {
            Default,
            HDR
        }
        #endregion

        #region Fields
        [SerializeField] private Gradient val;
        [SerializeField] [GradientUsage(true)] private Gradient hdrVal;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Gradient)value; }
        public override Type ValueType => typeof(Gradient);
        #endregion

        #region Methods
        protected override Settings CreateSettings()
        {
            return new GradientSettings();
        }
        #endregion
    }

    [Serializable]
    public class GameObjectParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private GameObject val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (GameObject)value; }
        public override Type ValueType => typeof(GameObject);
        #endregion
    }

    [Serializable]
    public class BoolParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private bool val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (bool)value; }
        public override Type ValueType => typeof(bool);
        #endregion
    }

    [Serializable]
    public class Texture2DParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private Texture2D val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Texture2D)value; }
        public override Type ValueType => typeof(Texture2D);
        #endregion
    }

    [Serializable]
    public class RenderTextureParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private RenderTexture val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (RenderTexture)value; }
        public override Type ValueType => typeof(RenderTexture);
        #endregion
    }

    [Serializable]
    public class MeshParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private Mesh val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Mesh)value; }
        public override Type ValueType => typeof(Mesh);
        #endregion
    }

    [Serializable]
    public class MaterialParameter : ExposedParameter
    {
        #region Fields
        [SerializeField] private Material val;
        #endregion

        #region Properties
        public override object Value { get => val; set => val = (Material)value; }
        public override Type ValueType => typeof(Material);
        #endregion
    }
}