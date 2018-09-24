using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.TypeScanner.Internal;
using EPiServer.ServiceLocation.AutoDiscovery;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RegionOrebroLan.EPiServer.Framework.Initialization;

namespace RegionOrebroLan.EPiServer.UnitTests.Framework.Initialization
{
	[TestClass]
	public class InitializationEngineReplacementTest
	{
		#region Methods

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		[SuppressMessage("Usage", "CA1806:Do not ignore method results")]
		public void Constructor_IfTheOriginalInitializationEngineParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			try
			{
				// ReSharper disable ObjectCreationAsStatement
				new InitializationEngineReplacement(null);
				// ReSharper restore ObjectCreationAsStatement
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(argumentNullException.ParamName.Equals("originalInitializationEngine", StringComparison.Ordinal))
					throw;
			}
		}

		protected internal virtual InitializationEngineReplacement CreateInitializationEngineReplacement()
		{
			return this.CreateInitializationEngineReplacement(HostType.Undefined);
		}

		protected internal virtual InitializationEngineReplacement CreateInitializationEngineReplacement(HostType hostType)
		{
			return this.CreateInitializationEngineReplacement(Mock.Of<IEnumerable<Assembly>>(), Mock.Of<IAssemblyScanner>(), hostType, Mock.Of<IServiceLocatorFactory>());
		}

		protected internal virtual InitializationEngineReplacement CreateInitializationEngineReplacement(DisabledInitializationEngine originalInitializationEngine)
		{
			return new InitializationEngineReplacement(originalInitializationEngine);
		}

		protected internal virtual InitializationEngineReplacement CreateInitializationEngineReplacement(IEnumerable<Assembly> assemblies, IAssemblyScanner assemblyScanner, HostType hostType, IServiceLocatorFactory serviceLocatorFactory)
		{
			return this.CreateInitializationEngineReplacement(new DisabledInitializationEngine(assemblies, assemblyScanner, hostType, serviceLocatorFactory));
		}

		[TestMethod]
		public void InitializationState_Set_ShouldSetTheInitializationStateOfTheOriginalInitializationEngine()
		{
			var initializationEngineReplacement = this.CreateInitializationEngineReplacement();

			Assert.AreEqual(InitializationState.PreInitialize, initializationEngineReplacement.InitializationState);
			Assert.AreEqual(InitializationState.PreInitialize, initializationEngineReplacement.OriginalInitializationEngine.InitializationState);

			initializationEngineReplacement.InitializationState = InitializationState.Uninitialized;

			Assert.AreEqual(InitializationState.Uninitialized, initializationEngineReplacement.InitializationState);
			Assert.AreEqual(InitializationState.Uninitialized, initializationEngineReplacement.OriginalInitializationEngine.InitializationState);
		}

		[TestMethod]
		public void Modules_Get_IfTheAssembliesAreEmpty_ShouldBeEmptyByDefault()
		{
			var initializationEngineReplacement = this.CreateInitializationEngineReplacement();

			initializationEngineReplacement.OriginalInitializationEngine.Assemblies = Enumerable.Empty<Assembly>();

			Assert.IsFalse(initializationEngineReplacement.Assemblies.Any());

			Assert.IsNotNull(initializationEngineReplacement.Modules);
			Assert.IsFalse(initializationEngineReplacement.Modules.Any());
		}

		#endregion
	}
}