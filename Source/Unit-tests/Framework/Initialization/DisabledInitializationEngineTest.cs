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
	public class DisabledInitializationEngineTest
	{
		#region Methods

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void BuildTypeScanner_ShouldThrowAnInvalidOperationException()
		{
			try
			{
				this.CreateDisabledInitializationEngine().BuildTypeScanner();
			}
			catch(InvalidOperationException invalidOperationException)
			{
				if(invalidOperationException.Message.Equals("This is a disabled initialization-engine.", StringComparison.OrdinalIgnoreCase))
					throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ConfigureModules_ShouldThrowAnInvalidOperationException()
		{
			try
			{
				this.CreateDisabledInitializationEngine().ConfigureModules(It.IsAny<IEnumerable<IInitializableModule>>());
			}
			catch(InvalidOperationException invalidOperationException)
			{
				if(invalidOperationException.Message.Equals("This is a disabled initialization-engine.", StringComparison.OrdinalIgnoreCase))
					throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		[SuppressMessage("Usage", "CA1806:Do not ignore method results")]
		public void Constructor_IfTheAssembliesParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			try
			{
				// ReSharper disable ObjectCreationAsStatement
				new DisabledInitializationEngine(null, Mock.Of<IAssemblyScanner>(), HostType.Undefined, Mock.Of<IServiceLocatorFactory>());
				// ReSharper restore ObjectCreationAsStatement
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(argumentNullException.ParamName.Equals("assemblies", StringComparison.Ordinal))
					throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		[SuppressMessage("Usage", "CA1806:Do not ignore method results")]
		public void Constructor_IfTheAssemblyScannerParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			try
			{
				// ReSharper disable ObjectCreationAsStatement
				new DisabledInitializationEngine(Mock.Of<IEnumerable<Assembly>>(), null, HostType.Undefined, Mock.Of<IServiceLocatorFactory>());
				// ReSharper restore ObjectCreationAsStatement
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(argumentNullException.ParamName.Equals("assemblyScanner", StringComparison.Ordinal))
					throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		[SuppressMessage("Usage", "CA1806:Do not ignore method results")]
		public void Constructor_IfTheServiceLocatorFactoryParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			try
			{
				// ReSharper disable ObjectCreationAsStatement
				new DisabledInitializationEngine(Mock.Of<IEnumerable<Assembly>>(), Mock.Of<IAssemblyScanner>(), HostType.Undefined, null);
				// ReSharper restore ObjectCreationAsStatement
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(argumentNullException.ParamName.Equals("serviceLocatorFactory", StringComparison.Ordinal))
					throw;
			}
		}

		[TestMethod]
		public void Constructor_ShouldSetTheAssembliesToTheAssembliesParameter()
		{
			var assemblies = Mock.Of<IEnumerable<Assembly>>();
			var disabledInitializationEngine = new DisabledInitializationEngine(assemblies, Mock.Of<IAssemblyScanner>(), HostType.Undefined, Mock.Of<IServiceLocatorFactory>());

			Assert.IsTrue(ReferenceEquals(assemblies, disabledInitializationEngine.Assemblies));
		}

		[TestMethod]
		public void Constructor_ShouldSetTheAssemblyScannerToTheAssemblyScannerParameter()
		{
			var assemblyScanner = Mock.Of<IAssemblyScanner>();
			var disabledInitializationEngine = new DisabledInitializationEngine(Mock.Of<IEnumerable<Assembly>>(), assemblyScanner, HostType.Undefined, Mock.Of<IServiceLocatorFactory>());

			var actualAssemblyScanner = (IAssemblyScanner) typeof(InitializationEngine).GetProperty("AssemblyScanner", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(disabledInitializationEngine);

			Assert.IsTrue(ReferenceEquals(assemblyScanner, actualAssemblyScanner));
		}

		protected internal virtual DisabledInitializationEngine CreateDisabledInitializationEngine()
		{
			return this.CreateDisabledInitializationEngine(HostType.Undefined);
		}

		protected internal virtual DisabledInitializationEngine CreateDisabledInitializationEngine(HostType hostType)
		{
			return this.CreateDisabledInitializationEngine(Mock.Of<IEnumerable<Assembly>>(), Mock.Of<IAssemblyScanner>(), hostType, Mock.Of<IServiceLocatorFactory>());
		}

		protected internal virtual DisabledInitializationEngine CreateDisabledInitializationEngine(IEnumerable<Assembly> assemblies, IAssemblyScanner assemblyScanner, HostType hostType, IServiceLocatorFactory serviceLocatorFactory)
		{
			return new DisabledInitializationEngine(assemblies, assemblyScanner, hostType, serviceLocatorFactory);
		}

		[TestMethod]
		public void InitializationState_Get_ShouldReturnPreInitializeByDefault()
		{
			Assert.AreEqual(InitializationState.PreInitialize, this.CreateDisabledInitializationEngine().InitializationState);
		}

		[TestMethod]
		public void Locate_Get_ShouldNotBeNullByDefault()
		{
			Assert.IsNotNull(this.CreateDisabledInitializationEngine().Locate);
		}

		[TestMethod]
		public void Modules_Get_ShouldReturnAnEmptyCollectionByDefault()
		{
			var disabledInitializationEngine = this.CreateDisabledInitializationEngine();

			Assert.IsNotNull(disabledInitializationEngine.Modules);
			Assert.IsFalse(disabledInitializationEngine.Modules.Any());
		}

		[TestMethod]
		public void Modules_Set_IfTheValueParameterIsEmpty_ShouldNotThrowAnException()
		{
			this.CreateDisabledInitializationEngine().Modules = Enumerable.Empty<IInitializableModule>();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Modules_Set_IfTheValueParameterIsNotEmpty_ShouldThrowAnInvalidOperationException()
		{
			try
			{
				this.CreateDisabledInitializationEngine().Modules = new IInitializableModule[1];
			}
			catch(InvalidOperationException invalidOperationException)
			{
				if(invalidOperationException.Message.Equals("This is a disabled initialization-engine.", StringComparison.OrdinalIgnoreCase))
					throw;
			}
		}

		[TestMethod]
		public void Modules_Set_IfTheValueParameterIsNull_ShouldNotThrowAnException()
		{
			this.CreateDisabledInitializationEngine().Modules = null;
		}

		#endregion
	}
}