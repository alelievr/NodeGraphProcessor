using UnityEditor;
using GraphProcessor;
using UnityEngine;

public class GraphProcessorMenuItems : NodeGraphProcessorMenuItems
{
    static readonly string converterBaseName = "Converter.cs";
    static string _converterTemplatePath = null;

    public static string ConverterTemplatePath
    {
        get
        {
            if (string.IsNullOrEmpty(_converterTemplatePath))
            {
                var tempaltes = AssetDatabase.FindAssets("ConvertTemplate");
                if (tempaltes.Length == 1)
                {
                    _converterTemplatePath = AssetDatabase.GUIDToAssetPath(tempaltes[0]);
                }
                else
                {
                    Debug.LogWarning("Please contact me: 110 or 120");
                }
            }
            return _converterTemplatePath;
        }
    }

    [MenuItem("Assets/Create/Converter C# Script", false, MenuItemPosition.afterCreateScript)]
    private static void CreateConverterCSharpScritpt() => ProjectWindowUtil.CreateScriptAssetFromTemplateFile(ConverterTemplatePath, converterBaseName);
}