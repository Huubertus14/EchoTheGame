using Fusion;
using Project.Echo.Contexts.Interfaces;

namespace Project.Echo.Contexts
{
	public abstract class ContextBehaviour : NetworkBehaviour, IContextBehaviour
	{
		public SceneContext Context { get; set; }
	}
}
