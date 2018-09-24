using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using RegionOrebroLan.EPiServer.Framework;

namespace RegionOrebroLan.EPiServer.IntegrationTests.Framework.Initialization.Helpers
{
	[InitializableModule]
	[BeforeModuleDependency(typeof(ServiceContainerInitialization))]
	public class InitializableModule : IInitializableModule
	{
		#region Methods

		public virtual void Initialize(InitializationEngine context) { }
		public virtual void Uninitialize(InitializationEngine context) { }

		#endregion
	}
}