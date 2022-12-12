using Fusion;
using Project.Echo.Contexts.Interfaces;

namespace Project.Echo.Contexts
{
	public abstract class ContextSimulationBehaviour : SimulationBehaviour, IContextBehaviour
	{
		public SceneContext Context { get; set; }
	}
}