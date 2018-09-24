using System;
using System.Linq;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.Framework.TypeScanner.Internal;
using EPiServer.ServiceLocation;
using EPiServer.Validation.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.EPiServer.Framework.Initialization;
using RegionOrebroLan.EPiServer.IntegrationTests.Framework.Initialization.Helpers;
using StructureMap;

namespace RegionOrebroLan.EPiServer.IntegrationTests.Framework.Initialization
{
	[TestClass]
	public class InitializationEngineReplacementTest
	{
		#region Methods

		protected internal virtual InitializationEngineReplacement CreateInitializationEngineReplacement()
		{
			return this.CreateInitializationEngineReplacement(new Container());
		}

		protected internal virtual InitializationEngineReplacement CreateInitializationEngineReplacement(IContainer container)
		{
			if(container == null)
				throw new ArgumentNullException(nameof(container));

			var assemblies = new[] {typeof(IInitializationEngine).Assembly, this.GetType().Assembly};
			var assemblyScanner = new ReflectionAssemblyScanner();
			const HostType hostType = HostType.WebApplication;
			var serviceLocatorFactory = new StructureMapServiceLocatorFactory(container);

			var originalInitializationEngine = new DisabledInitializationEngine(assemblies, assemblyScanner, hostType, serviceLocatorFactory);

			return new InitializationEngineReplacement(originalInitializationEngine);
		}

		[TestMethod]
		public void Initialize_Test()
		{
			var initializationEngineReplacement = this.CreateInitializationEngineReplacement();

			Assert.AreEqual(InitializationState.PreInitialize, initializationEngineReplacement.InitializationState);
			Assert.AreEqual(InitializationState.PreInitialize, initializationEngineReplacement.OriginalInitializationEngine.InitializationState);

			Assert.AreEqual(0, initializationEngineReplacement.OriginalInitializationEngine.Modules.Count());

			Assert.AreEqual(7, initializationEngineReplacement.Modules.Count());
			Assert.AreEqual(7, initializationEngineReplacement.OriginalInitializationEngine.Modules.Count());

			initializationEngineReplacement.Initialize();

			Assert.AreEqual(InitializationState.Initialized, initializationEngineReplacement.InitializationState);
			Assert.AreEqual(InitializationState.Initialized, initializationEngineReplacement.OriginalInitializationEngine.InitializationState);

			Assert.AreEqual(7, initializationEngineReplacement.Modules.Count());
			Assert.AreEqual(7, initializationEngineReplacement.OriginalInitializationEngine.Modules.Count());

			Assert.AreEqual("EPiServer.Async.Internal.ShutdownRegistrationModule", initializationEngineReplacement.Modules.ElementAt(0).GetType().FullName);
			Assert.AreEqual(typeof(InitializableModule), initializationEngineReplacement.Modules.ElementAt(1).GetType());
			Assert.AreEqual(typeof(ProviderBasedLocalizationService), initializationEngineReplacement.Modules.ElementAt(2).GetType());
			Assert.AreEqual(typeof(ServiceContainerInitialization), initializationEngineReplacement.Modules.ElementAt(3).GetType());
			Assert.AreEqual(typeof(ConfigurableModule), initializationEngineReplacement.Modules.ElementAt(4).GetType());
			Assert.AreEqual(typeof(FrameworkInitialization), initializationEngineReplacement.Modules.ElementAt(5).GetType());
			Assert.AreEqual(typeof(ValidationService), initializationEngineReplacement.Modules.ElementAt(6).GetType());
		}

		[TestMethod]
		public void OriginalOrderPrerequisiteTest()
		{
			var initializationEngineReplacement = this.CreateInitializationEngineReplacement();

			Assert.AreEqual(7, initializationEngineReplacement.Modules.Count());

			var originallyOrderedModules = initializationEngineReplacement.OriginalInitializationEngine.GetDependencySortedModules();

			Assert.AreEqual("EPiServer.Async.Internal.ShutdownRegistrationModule", originallyOrderedModules.ElementAt(0).GetType().FullName);
			Assert.AreEqual(typeof(ProviderBasedLocalizationService), originallyOrderedModules.ElementAt(1).GetType());
			Assert.AreEqual(typeof(ServiceContainerInitialization), originallyOrderedModules.ElementAt(2).GetType());
			Assert.AreEqual(typeof(InitializableModule), originallyOrderedModules.ElementAt(3).GetType());
			Assert.AreEqual(typeof(FrameworkInitialization), originallyOrderedModules.ElementAt(4).GetType());
			Assert.AreEqual(typeof(ValidationService), originallyOrderedModules.ElementAt(5).GetType());
			Assert.AreEqual(typeof(ConfigurableModule), originallyOrderedModules.ElementAt(6).GetType());
		}

		#endregion
	}
}