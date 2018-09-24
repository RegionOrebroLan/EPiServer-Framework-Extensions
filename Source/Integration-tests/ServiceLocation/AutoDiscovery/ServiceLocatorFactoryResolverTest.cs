using System;
using EPiServer.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.EPiServer.ServiceLocation.AutoDiscovery;

namespace RegionOrebroLan.EPiServer.IntegrationTests.ServiceLocation.AutoDiscovery
{
	[TestClass]
	public class ServiceLocatorFactoryResolverTest
	{
		#region Methods

		[TestMethod]
		public void Resolve_IfTheAssembliesContainAnyAttributeDecoratedType_ShouldReturnAnInstance()
		{
			var serviceLocatorFactory = new ServiceLocatorFactoryResolver().Resolve(new[] {typeof(StructureMapServiceLocator).Assembly});

			Assert.IsTrue(serviceLocatorFactory is StructureMapServiceLocatorFactory);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Resolve_IfTheAssembliesDoNotContainAnyAttributeDecoratedType_ShouldThrowAnInvalidOperationException()
		{
			new ServiceLocatorFactoryResolver().Resolve(new[] {this.GetType().Assembly});
		}

		#endregion
	}
}