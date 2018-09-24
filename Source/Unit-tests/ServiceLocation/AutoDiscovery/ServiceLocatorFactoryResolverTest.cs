using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.EPiServer.ServiceLocation.AutoDiscovery;

namespace RegionOrebroLan.EPiServer.UnitTests.ServiceLocation.AutoDiscovery
{
	[TestClass]
	public class ServiceLocatorFactoryResolverTest
	{
		#region Methods

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Resolve_IfTheAssembliesParameterIsEmpty_ShouldThrowAnArgumentException()
		{
			try
			{
				new ServiceLocatorFactoryResolver().Resolve(Enumerable.Empty<Assembly>());
			}
			catch(ArgumentException argumentException)
			{
				if(argumentException.ParamName.Equals("assemblies", StringComparison.Ordinal))
					throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Resolve_IfTheAssembliesParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			try
			{
				new ServiceLocatorFactoryResolver().Resolve(null);
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(argumentNullException.ParamName.Equals("assemblies", StringComparison.Ordinal))
					throw;
			}
		}

		#endregion
	}
}