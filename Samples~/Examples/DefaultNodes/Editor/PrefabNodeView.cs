using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
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

		objField.RegisterValueChangedCallback(v => {
			prefabNode.output = objField.value as GameObject;
			UpdatePreviewImage(preview, objField.value);
		});

		UpdatePreviewImage(preview, prefabNode.output);

		controlsContainer.Add(objField);
		controlsContainer.Add(preview);
	}

	void		UpdatePreviewImage(Image image, Object obj)
	{
		image.image = AssetPreview.GetAssetPreview(obj) ?? AssetPreview.GetMiniThumbnail(obj);
	}
}