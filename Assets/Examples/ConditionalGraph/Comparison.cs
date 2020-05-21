using GraphProcessor;
using UnityEngine.Rendering;

namespace NodeGraphProcessor.Examples
{
	[System.Serializable, NodeMenuItem("Conditional/Comparison")]
	public class Comparison : BaseNode
	{
		[Input(name = "In A")]
		public float    inA;
	
		[Input(name = "In B")]
		public float    inB;

		[Output(name = "Out")]
		public bool		compared;

		public CompareFunction		compareFunction = CompareFunction.LessEqual;

		public override string		name => "Comparison";

		protected override void Process()
		{
			switch (compareFunction)
			{
				default:
				case CompareFunction.Disabled:
				case CompareFunction.Never: compared = false; break;
				case CompareFunction.Always: compared = true; break;
				case CompareFunction.Equal: compared = inA == inB; break;
				case CompareFunction.Greater: compared = inA > inB; break;
				case CompareFunction.GreaterEqual: compared = inA >= inB; break;
				case CompareFunction.Less: compared = inA < inB; break;
				case CompareFunction.LessEqual: compared = inA <= inB; break;
				case CompareFunction.NotEqual: compared = inA != inB; break;
			}
		}
	}
}
