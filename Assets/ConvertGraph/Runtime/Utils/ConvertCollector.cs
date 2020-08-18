using GraphProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


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

                //if (framwWorkAssembiles == null || framwWorkAssembiles.Length < 1)
                //{
                //    //var config = AssetHelper.LoadSync<KailashConfig>(AssetDefine.KAILASH_CONFIG_PATH);
                //    var config = AddressableHelper.LoadAssetByAddressableName<KailashConfig>(AssetDefine.KAILASH_CONFIG_PATH);
                //    var assemblies = config.Assemblies;
                //    var list = new List<string>();
                //    foreach (UnityEditorInternal.AssemblyDefinitionAsset item in assemblies)
                //    {
                //        if (item.name == "Kailash.Framework") continue;
                //        list.Add(item.name);
                //    }
                //    framwWorkAssembiles = list.ToArray();
                //}

                //return framwWorkAssembiles;
            }

        }

        static ConvertCollector()
        {
            var assemblies = FrameWorkdAssembliles;
            foreach (var assemblyName in assemblies)
            {
                var assembly = Assembly.Load(assemblyName);

                foreach (var classType in assembly.GetTypes())
                {
                    if (classType.IsClass && classType.IsAbstract && classType.IsSealed) // static classes are declared abstract and sealed at the IL level https://stackoverflow.com/a/1175901
                    {
                        var convertFuncAttribute = classType.GetCustomAttribute<ConvertFuncAttribute>();
                        if (convertFuncAttribute != null)
                        {
                            MethodInfo[] methods = classType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                            bool containsConvertFunc = false;
                            foreach (var method in methods)
                            {
                                if (method.Name == ConverMethodName)
                                {
                                    var menu = convertFuncAttribute.catalog;
                                    var menuStrs = menu.Split('/');
                                    string lastMenu = menuStrs.LastOrDefault();
                                    int length = menuStrs.Length;
                                    if (lastMenu != classType.Name)
                                    {
                                        if (lastMenu == string.Empty)
                                        {
                                            menuStrs[menuStrs.Length - 2] = classType.Name;
                                            length -= 1;
                                        }
                                        menuStrs[menuStrs.Length - 1] = classType.Name;
                                    }

                                    var sb = new System.Text.StringBuilder();
                                    for (int i = 0; i < length; i++)
                                    {
                                        sb.Append(menuStrs[i]);
                                        if (i != length - 1) sb.Append('/');
                                    }

                                    sb.Append("$");
                                    sb.Append(assemblyName);
                                    if (!GraphProcessor.NodeProvider.AppendNodePerMenu(sb.ToString(), typeof(ConvertNode)))
                                    {
                                        throw new Exception(string.Format("Please rename the convert class  {0} ", sb.ToString()));
                                    }
                                    containsConvertFunc = true;
                                }
                            }
                            if (!containsConvertFunc)
                            {
                                throw new Exception(string.Format("Please declare a method named {0} or check out the method name", ConverMethodName));
                            }
                        }

                    }

                }
            }
        }

        public static void NothingToDo(){

        }
    }
}