using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

public class RuntimeGraph : MonoBehaviour
{
	public BaseGraph	graph;
	public ProcessGraphProcessor	processor;

	public GameObject	assignedGameObject;

	private void Start()
	{
		if (graph != null)
			processor = new ProcessGraphProcessor(graph);
	}

	int i = 0;

    void Update()
    {
		if (graph != null)
		{
			graph.SetParameterValue("Input", (float)i++);
			graph.SetParameterValue("GameObject", assignedGameObject);
			processor.Run();
			Debug.Log("Output: " + graph.GetParameterValue("Output"));
		}
    }
}
