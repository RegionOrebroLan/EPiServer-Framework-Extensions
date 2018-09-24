using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EPiServer.ServiceLocation.AutoDiscovery;

namespace RegionOrebroLan.EPiServer.ServiceLocation.AutoDiscovery
{
	public class ServiceLocatorFactoryResolver : IServiceLocatorFactoryResolver
	{
		#region Methods

		public virtual IServiceLocatorFactory Resolve(IEnumerable<Assembly> assemblies)
		{
			assemblies = assemblies?.ToArray();

			if(assemblies == null)
				throw new ArgumentNullException(nameof(assemblies));

			if(!assemblies.Any())
				throw new ArgumentException("The assembly-collection is empty.", nameof(assemblies));

			var serviceLocatorFactoryType = assemblies.SelectMany(assembly => assembly.GetCustomAttributes(typeof(ServiceLocatorFactoryAttribute)).Cast<ServiceLocatorFactoryAttribute>()).FirstOrDefault()?.ServiceLocatorFactoryType;

			if(serviceLocatorFactoryType == null)
				throw new InvalidOperationException("There is no dependency injection framework installed. Resolve this issue by installing NuGet-package \"EPiServer.ServiceLocation.StructureMap\".");

			return (IServiceLocatorFactory) Activator.CreateInstance(serviceLocatorFactoryType);
		}

		#endregion
	}
}