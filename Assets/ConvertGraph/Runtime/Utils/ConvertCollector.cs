using GraphProcessor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Cr7Sund.ConvertGraph
{
    public static class ConvertCollector
    {
        public const string ConverMethodName = "Convert";

        private static string[] framwWorkAssembiles;
        private static string[] FrameWorkdAssembliles
        {
            get
            {
                var list = new List<string>();
                list.Add(typeof(ConvertNode).Assembly.FullName);
                return list.ToArray();

                // if (framwWorkAssembiles == null || framwWorkAssembiles.Length < 1)
                // {
                //     //var config = AssetHelper.LoadSync<KailashConfig>(AssetDefine.KAILASH_CONFIG_PATH);
                //     var config = AddressableHelper.LoadAssetByAddressableName<KailashConfig>(AssetDefine.KAILASH_CONFIG_PATH);
                //     var assemblies = config.Assemblies;
                //     var list = new List<string>();
                //     foreach (UnityEditorInternal.AssemblyDefinitionAsset item in assemblies)
                //     {
                //         if (item.name == "Kailash.Framework") continue;
                //         list.Add(item.name);
                //     }
                //     framwWorkAssembiles = list.ToArray();
                // }

                // return framwWorkAssembiles;
            }

        }

        static bool hasDeclareConverterClass;
        static ConvertCollector()
        {
            var assemblies = FrameWorkdAssembliles;
            int count = 0;
            foreach (var assemblyName in assemblies)
            {
                var assembly = Assembly.Load(assemblyName);

                foreach (var classType in assembly.GetTypes())
                {
                    if (classType.IsClass && classType.IsAbstract && classType.IsSealed) // static classes are declared abstract and sealed at the IL level https://stackoverflow.com/a/1175901
                    {
                        var convertClassAttribute = classType.GetCustomAttribute<ConvertClassAttribute>();
                        if (convertClassAttribute != null)
                        {
                            MethodInfo[] methods = classType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                            foreach (var method in methods)
                            {
                                var convertFuncAttribute = method.GetCustomAttribute<ConvertFuncAttribute>();
                                if (convertFuncAttribute != null)
                                {
                                    var menu = convertClassAttribute.catalog;
                                    var menuStrs = menu.Split('/');

                                    var sb = new System.Text.StringBuilder();
                                    for (int i = 0; i < menuStrs.Length; i++)
                                    {
                                        if (i == menuStrs.Length - 1 && menuStrs[i] == string.Empty)
                                        {
                                            continue;
                                        }
                                        sb.Append(menuStrs[i]);
                                        sb.Append('/');
                                    }

                                    sb.Append($"{classType.FullName}/{method.Name}");

                                    sb.Append("$");
                                    sb.Append(convertFuncAttribute.hint);
                                    sb.Append("$");
                                    sb.Append(assemblyName);
                                    if (!GraphProcessor.NodeProvider.AppendNodePerMenu(sb.ToString(), typeof(ConvertNode)))
                                    {
                                        throw new Exception(string.Format("Please rename the convert class  {0} ", sb.ToString()));
                                    }

                                    GraphProcessor.NodeProvider.AppendNodePerMenu("SourceNode/", typeof(SourceNode));

                                    ParameterInfo[] parameterInfos = method.GetParameters();
                                    var inputParams = new List<ParameterInfo>();
                                    var outputParams = new List<ParameterInfo>();
                                    for (int i = 0; i < parameterInfos.Length; i++)
                                    {
                                        if (parameterInfos[i].IsOut)
                                        {
                                            outputParams.Add(parameterInfos[i]);
                                        }
                                        else
                                            inputParams.Add(parameterInfos[i]);

                                    }
                                    GraphProcessor.NodeProvider.AppendPortInfoPerNode(inputParams, typeof(ConvertNode), true, sb.ToString());
                                    GraphProcessor.NodeProvider.AppendPortInfoPerNode(outputParams, typeof(ConvertNode), false, sb.ToString());

                                    ++count;
                                }
                            }
                        }
                    }

                }
            }

            if (count < 1)
            {
                hasDeclareConverterClass = false;
            }
            else
            {
                hasDeclareConverterClass = true;
            }
        }
        public static bool CheckEverythingIsValid()
        {
            if (!hasDeclareConverterClass)
            {
                Debug.LogError($"You must declar a converter scipt first \n . Please create one via Create/Converter C# Script menu");
                return false;
            }

            return true;
        }
    }
}