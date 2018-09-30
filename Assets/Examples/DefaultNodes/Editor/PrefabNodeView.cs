using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
using GraphProcessor;

[NodeCustomEditor(typeof(PrefabNode))]
public class PrefabNodeView : BaseNodeView
{
	public override void Enable()
	{
		var prefabNode = nodeTarget as PrefabNode;

        var objField = new ObjectField
        {
			objectType = typeof(GameObject),
			allowSceneObjects = false,
            value = prefabNode.output,
        };

		var preview = new Image();

		objField.OnValueChanged(v => {
			preview.image = AssetPreview.GetAssetPreview(objField.value) ?? AssetPreview.GetMiniThumbnail(objField.value);
		});

		controlsContainer.Add(objField);
		controlsContainer.Add(preview);
	}
}