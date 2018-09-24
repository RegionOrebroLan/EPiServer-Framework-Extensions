using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.TypeScanner.Internal;
using EPiServer.ServiceLocation.AutoDiscovery;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RegionOrebroLan.EPiServer.Framework.Initialization;

namespace RegionOrebroLan.EPiServer.UnitTests.Framework.Initialization
{
	[TestClass]
	public class InitializationEngineFactoryTest
	{
		#region Methods

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		[SuppressMessage("Usage", "CA1806:Do not ignore method results")]
		public void Constructor_IfTheAssembliesParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			try
			{
				// ReSharper disable ObjectCreationAsStatement
				new InitializationEngineFactory(null, Mock.Of<IAssemblyScanner>(), Mock.Of<IServiceLocatorFactory>());
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
				new InitializationEngineFactory(Mock.Of<IEnumerable<Assembly>>(), null, Mock.Of<IServiceLocatorFactory>());
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
				new InitializationEngineFactory(Mock.Of<IEnumerable<Assembly>>(), Mock.Of<IAssemblyScanner>(), null);
				// ReSharper restore ObjectCreationAsStatement
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(argumentNullException.ParamName.Equals("serviceLocatorFactory", StringComparison.Ordinal))
					throw;
			}
		}

		[TestMethod]
		public void Create_ShouldReturnAnInitializationEngineReplacement()
		{
			var initializationEngine = new InitializationEngineFactory(Mock.Of<IEnumerable<Assembly>>(), Mock.Of<IAssemblyScanner>(), Mock.Of<IServiceLocatorFactory>()).Create(HostType.Undefined);

			Assert.IsTrue(initializationEngine is InitializationEngineReplacement);
		}

		#endregion
	}
}