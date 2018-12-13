using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		public void InitializationState_AfterInitialize_ShouldReturnInitialized()
		{
			var initializationEngineReplacement = this.CreateInitializationEngineReplacement();

			initializationEngineReplacement.Initialize();

			Assert.AreEqual(InitializationState.Initialized, initializationEngineReplacement.InitializationState);
			Assert.AreEqual(InitializationState.Initialized, initializationEngineReplacement.OriginalInitializationEngine.InitializationState);
		}

		[TestMethod]
		public void Initialize_WhenComplete_ShouldHaveTriggeredInitComplete()
		{
			// First test
			var initializationCompleteCallsCount = 0;
			var initializationEngineReplacement = this.CreateInitializationEngineReplacement();
			initializationEngineReplacement.InitComplete += (sender, e) => initializationCompleteCallsCount++;
			var initializableModule = (InitializableModule) initializationEngineReplacement.Modules.ElementAt(1);

			Assert.AreEqual(0, initializationCompleteCallsCount);
			Assert.AreEqual(0, initializableModule.InitializationCompleteCallsCount);

			initializationEngineReplacement.Initialize();

			Assert.AreEqual(1, initializationCompleteCallsCount);
			Assert.AreEqual(1, initializableModule.InitializationCompleteCallsCount);

			// Second test
			var defaultRegisterTaskMonitor = ConfigurableModule.RegisterTaskMonitor;
			var failed = false;
			initializationEngineReplacement = this.CreateInitializationEngineReplacement();
			ConfigurableModule.RegisterTaskMonitor = false;

			try
			{
				initializationEngineReplacement.Initialize();
				failed = true;
			}
			catch(TargetInvocationException targetInvocationException)
			{
				if(!(targetInvocationException.InnerException is StructureMapBuildPlanException structureMapBuildPlanException))
				{
					failed = true;
				}
				else
				{
					const string expectedExceptionMessageStart = "Unable to create a build plan for concrete type ServiceAccessor<TaskInformationStorage>";
					var message = structureMapBuildPlanException.Message;

					if(!message.StartsWith(expectedExceptionMessageStart, StringComparison.OrdinalIgnoreCase))
						failed = false;
				}
			}

			ConfigurableModule.RegisterTaskMonitor = defaultRegisterTaskMonitor;

			if(failed)
				Assert.Fail("The second initialization should have thrown an exception.");
		}

		[TestMethod]
		public void Initialize_WhenComplete_TheServiceLocatorShouldReturnAnInstanceOfInitializationEngineReplacementWhenAskingForIInitializationEngine()
		{
			this.CreateInitializationEngineReplacement().Initialize();

			Assert.AreEqual(1, ServiceLocator.Current.GetAllInstances<IInitializationEngine>().Count());

			Assert.IsTrue(ServiceLocator.Current.GetInstance<IInitializationEngine>() is InitializationEngineReplacement);
		}

		[TestMethod]
		public void Modules_IfInitialized_ShouldBeOrderedCorrectly()
		{
			var initializationEngineReplacement = this.CreateInitializationEngineReplacement();

			initializationEngineReplacement.Initialize();

			this.ModulesCountAndOrderShouldBeCorrect(initializationEngineReplacement.Modules);
		}

		[TestMethod]
		public void Modules_IfNotInitialized_ShouldBeOrderedCorrectly()
		{
			var initializationEngineReplacement = this.CreateInitializationEngineReplacement();

			this.ModulesCountAndOrderShouldBeCorrect(initializationEngineReplacement.Modules);
		}

		[TestMethod]
		public void Modules_Items_AreTheSameInstanceAsInTheModulesOfTheOriginalInitializationEngine()
		{
			var initializationEngineReplacement = this.CreateInitializationEngineReplacement();

			var initializableModule = (InitializableModule) initializationEngineReplacement.Modules.ElementAt(1);
			var originalInitializableModule = (InitializableModule) initializationEngineReplacement.OriginalInitializationEngine.GetDependencySortedModules().ElementAt(3);

			Assert.IsTrue(ReferenceEquals(initializableModule, originalInitializableModule));
		}

		protected internal virtual void ModulesCountAndOrderShouldBeCorrect(IEnumerable<IInitializableModule> modules)
		{
			if(modules == null)
				throw new ArgumentNullException(nameof(modules));

			modules = modules.ToArray();

			Assert.AreEqual(7, modules.Count());

			Assert.AreEqual("EPiServer.Async.Internal.ShutdownRegistrationModule", modules.ElementAt(0).GetType().FullName);
			Assert.AreEqual(typeof(InitializableModule), modules.ElementAt(1).GetType());
			Assert.AreEqual(typeof(ProviderBasedLocalizationService), modules.ElementAt(2).GetType());
			Assert.AreEqual(typeof(ServiceContainerInitialization), modules.ElementAt(3).GetType());
			Assert.AreEqual(typeof(ConfigurableModule), modules.ElementAt(4).GetType());
			Assert.AreEqual(typeof(FrameworkInitialization), modules.ElementAt(5).GetType());
			Assert.AreEqual(typeof(ValidationService), modules.ElementAt(6).GetType());
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