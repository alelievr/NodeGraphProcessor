using UnityEngine;
using GraphProcessor;

[ExecuteAlways]
public class GraphBehaviour : MonoBehaviour
{
    public BaseGraph graph;

    void Start()
    {
        if (graph == null)
            graph = ScriptableObject.CreateInstance<BaseGraph>();
    }
}
