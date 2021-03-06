﻿using System;
using EPiServer.Async;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Moq;

namespace RegionOrebroLan.EPiServer.IntegrationTests.Framework.Initialization.Helpers
{
	[InitializableModule]
	[ModuleDependency(typeof(InitializableModule))]
	public class ConfigurableModule : IConfigurableModule
	{
		#region Properties

		public static bool RegisterTaskMonitor { get; set; } = true;

		#endregion

		#region Methods

		public virtual void ConfigureContainer(ServiceConfigurationContext context)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			context.Services.AddSingleton(Mock.Of<IVirtualRoleReplication>());

			if(RegisterTaskMonitor)
				context.Services.AddSingleton(Mock.Of<TaskMonitor>());
		}

		public virtual void Initialize(InitializationEngine context) { }
		public virtual void Uninitialize(InitializationEngine context) { }

		#endregion
	}
}