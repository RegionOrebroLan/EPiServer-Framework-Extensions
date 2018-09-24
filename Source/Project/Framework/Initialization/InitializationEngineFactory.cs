using System;
using System.Collections.Generic;
using System.Reflection;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.TypeScanner.Internal;
using EPiServer.ServiceLocation.AutoDiscovery;

namespace RegionOrebroLan.EPiServer.Framework.Initialization
{
	public class InitializationEngineFactory : IInitializationEngineFactory
	{
		#region Constructors

		public InitializationEngineFactory(IEnumerable<Assembly> assemblies, IAssemblyScanner assemblyScanner, IServiceLocatorFactory serviceLocatorFactory)
		{
			this.Assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
			this.AssemblyScanner = assemblyScanner ?? throw new ArgumentNullException(nameof(assemblyScanner));
			this.ServiceLocatorFactory = serviceLocatorFactory ?? throw new ArgumentNullException(nameof(serviceLocatorFactory));
		}

		#endregion

		#region Properties

		protected internal virtual IEnumerable<Assembly> Assemblies { get; }
		protected internal virtual IAssemblyScanner AssemblyScanner { get; }
		protected internal virtual IServiceLocatorFactory ServiceLocatorFactory { get; }

		#endregion

		#region Methods

		public virtual IInitializationEngine Create(HostType hostType)
		{
			return new InitializationEngineReplacement(this.CreateOriginalInitializationEngine(hostType));
		}

		protected internal virtual DisabledInitializationEngine CreateOriginalInitializationEngine(HostType hostType)
		{
			return new DisabledInitializationEngine(this.Assemblies, this.AssemblyScanner, hostType, this.ServiceLocatorFactory);
		}

		#endregion
	}
}