using System.Reflection;

namespace GraphProcessor
{
	public abstract class PortAdapter
	{
		public virtual int priority { get { return 0; } }

		public abstract MethodInfo GetAdapterMethod();
	}
}