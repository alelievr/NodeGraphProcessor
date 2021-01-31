using System;
using System.Collections;
using GraphProcessor;
using UnityEngine;

namespace NodeGraphProcessor.Examples
{
	[Serializable, NodeMenuItem("Functions/Wait Frames")]
	public class WaitFrameNode : WaitableNode
	{
		public override string name => "Wait Frames";

		[SerializeField, Input(name = "Frames")]
		public int frame = 1;

		private static WaitFrameMonoBehaviour waitFrameMonoBehaviour;

		protected override void Process()
		{
			if(waitFrameMonoBehaviour == null)
			{
				var go = new GameObject(name: "WaitFrameGameObject");
				waitFrameMonoBehaviour = go.AddComponent<WaitFrameMonoBehaviour>();
			}

			waitFrameMonoBehaviour.Process(frame, ProcessFinished);
		}
	}

	public class WaitFrameMonoBehaviour : MonoBehaviour
	{
		public void Process(int frame, Action callback)
		{
			StartCoroutine(_Process(frame, callback));
		}

		private IEnumerator _Process(int frame, Action callback)
		{
			for(int i = 0; i < frame; i++)
			{
				yield return new WaitForEndOfFrame();
				i++;
			}

			callback.Invoke();
		}
	}
}