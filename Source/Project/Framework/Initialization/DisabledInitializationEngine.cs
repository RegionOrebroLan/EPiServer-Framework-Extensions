using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.TypeScanner;
using EPiServer.Framework.TypeScanner.Internal;
using EPiServer.ServiceLocation.AutoDiscovery;

namespace RegionOrebroLan.EPiServer.Framework.Initialization
{
	public class DisabledInitializationEngine : InitializationEngine
	{
		#region Fields

		private const string _exceptionMessage = "This is a disabled initialization-engine.";
		private IEnumerable<IInitializableModule> _initializableModules;

		#endregion

		#region Constructors

		public DisabledInitializationEngine(IEnumerable<Assembly> assemblies, IAssemblyScanner assemblyScanner, HostType hostType, IServiceLocatorFactory serviceLocatorFactory) : base(serviceLocatorFactory, null, hostType, assemblies, assemblyScanner)
		{
			if(assemblies == null)
				throw new ArgumentNullException(nameof(assemblies));

			if(assemblyScanner == null)
				throw new ArgumentNullException(nameof(assemblyScanner));

			if(serviceLocatorFactory == null)
				throw new ArgumentNullException(nameof(serviceLocatorFactory));
		}

		#endregion

		#region Properties

		protected internal virtual string ExceptionMessage => _exceptionMessage;

		protected internal virtual IEnumerable<IInitializableModule> InitializableModules
		{
			get => this._initializableModules ?? (this._initializableModules = Enumerable.Empty<IInitializableModule>());
			set => this._initializableModules = value;
		}

		public override IEnumerable<IInitializableModule> Modules
		{
			get => this.InitializableModules;
			set
			{
				if(value != null && value.Any())
					throw new InvalidOperationException(this.ExceptionMessage);
			}
		}

		#endregion

		#region Methods

		public override ITypeScannerLookup BuildTypeScanner()
		{
			throw new InvalidOperationException(this.ExceptionMessage);
		}

		public override void ConfigureModules(IEnumerable<IInitializableModule> modules)
		{
			throw new InvalidOperationException(this.ExceptionMessage);
		}

		public virtual void SetInitializationState(InitializationState initializationState)
		{
			this.InitializationState = initializationState;
		}

		public virtual void SetModules(IEnumerable<IInitializableModule> modules)
		{
			this.InitializableModules = modules;
		}

		#endregion
	}
}