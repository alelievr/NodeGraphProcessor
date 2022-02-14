using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphProcessor
{
    public static class MemberInfoExtension
    {
        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }

        public static string GetPath(this IList<MemberInfo> list)
        {
            string path = "";
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) path += ".";
                path += list[i].Name;
            }
            return path;
        }

        public static bool HasCustomAttribute<T>(this MemberInfo memberInfo)
        {
            return Attribute.IsDefined(memberInfo, typeof(T));
        }

        public static object GetValue(this MemberInfo memberInfo, object forObject)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(forObject);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(forObject);
                default:
                    throw new NotImplementedException();
            }
        }

        public static void SetValue(this MemberInfo memberInfo, object forObject, object value)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)memberInfo).SetValue(forObject, value);
                    break;
                case MemberTypes.Property:
                    if (((PropertyInfo)memberInfo).GetSetMethod(true) == null) break;
                    ((PropertyInfo)memberInfo).SetValue(forObject, value);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool IsPublic(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).IsPublic;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetAccessors().Any(MethodInfo => MethodInfo.IsPublic);
                default:
                    return false;
            }
        }

        public static bool IsField(this MemberInfo memberInfo)
        {
            return memberInfo.MemberType == MemberTypes.Field;
        }

        public static object GetFinalValue(this IList<MemberInfo> list, object startingValue)
        {
            object currentValue = startingValue;
            for (int i = 0; i < list.Count; i++)
            {
                currentValue = list[i].GetValue(currentValue);
            }
            return currentValue;
        }

        public static void SetValue(this IList<MemberInfo> list, object startingValue, object finalValue)
        {
            object currentValue = startingValue;
            for (int i = 0; i < list.Count; i++)
            {
                if (i + 1 == list.Count)
                {
                    list[i].SetValue(currentValue, finalValue);
                    break;
                }

                currentValue = list[i].GetValue(currentValue);
            }
        }

        public static object GetValueAt(this IList<MemberInfo> list, object startingValue, int index)
        {
            object currentValue = startingValue;
            for (int i = 0; i < list.Count; i++)
            {
                currentValue = list[i].GetValue(currentValue);
                if (i == index) break;
            }
            return currentValue;
        }

        public static bool IsValid(this IList<MemberInfo> list)
        {
            return list.Any(x => x == null);
        }
    }
}