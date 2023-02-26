using UnityEditor;
using GraphProcessor;

public class GraphProcessorMenuItems : NodeGraphProcessorMenuItems
{
	[MenuItem("Assets/Create/Node C# Script", false, MenuItemPosition.afterCreateScript)]
	private static void CreateNodeCSharpScript() => CreateDefaultNodeCSharpScript();
	
	[MenuItem("Assets/Create/Node View C# Script", false, MenuItemPosition.afterCreateScript + 1)]
	private static void CreateNodeViewCSharpScript() => CreateDefaultNodeViewCSharpScript();

	// To add your C# script creation with you own templates, use ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, defaultFileName)
}