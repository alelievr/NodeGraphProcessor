using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GraphProcessor
{
	public class BaseGraph : ScriptableObject, ISerializationCallbackReceiver
	{
		//JSon datas contaning all elements of the graph
		[SerializeField]
		public string				serializedNodes;

		[System.NonSerialized]
		public List< BaseNode >		nodes = new List< BaseNode >();

        void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;
            UndoRedoPerformed();
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        void UndoRedoPerformed()
        {
			Debug.Log("TODO !");
        }

		public void OnBeforeSerialize()
		{
			serializedNodes = JsonUtility.ToJson(nodes);
			throw new System.NotImplementedException();
		}

		public void OnAfterDeserialize()
		{
			throw new System.NotImplementedException();
		}
	}
}