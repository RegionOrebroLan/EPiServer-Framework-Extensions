using System.Collections.Generic;
using System.Reflection;
using EPiServer.ServiceLocation.AutoDiscovery;

namespace RegionOrebroLan.EPiServer.ServiceLocation.AutoDiscovery
{
	public interface IServiceLocatorFactoryResolver
	{
		#region Methods

		IServiceLocatorFactory Resolve(IEnumerable<Assembly> assemblies);

		#endregion
	}
}