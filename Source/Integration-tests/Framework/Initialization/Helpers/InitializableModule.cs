using System;
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
		#region Properties

		public virtual int InitializationCompleteCallsCount { get; private set; }

		#endregion

		#region Methods

		public virtual void Initialize(InitializationEngine context)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			context.InitComplete += this.OnInitializationComplete;
		}

		protected internal virtual void OnInitializationComplete(object sender, EventArgs e)
		{
			this.InitializationCompleteCallsCount++;
		}

		public virtual void Uninitialize(InitializationEngine context) { }

		#endregion
	}
}