using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using EPiServer.Framework;

namespace RegionOrebroLan.EPiServer.Framework
{
	[SuppressMessage("Microsoft.Design", "CA1019:Define accessors for attribute arguments")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class BeforeModuleDependencyAttribute : Attribute
	{
		#region Constructors

		public BeforeModuleDependencyAttribute(Type dependency) : this(new[] {ValidateDependency(dependency)}) { }

		public BeforeModuleDependencyAttribute(IEnumerable<Type> dependencies)
		{
			var dependencyArray = dependencies?.ToArray();

			if(dependencyArray == null)
				throw new ArgumentNullException(nameof(dependencies));

			if(dependencyArray.Any(dependency => dependency == null))
				throw new ArgumentException("Dependencies can not contain null values.", nameof(dependencies));

			if(dependencyArray.Any(dependency => !typeof(IInitializableModule).IsAssignableFrom(dependency)))
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "All dependencies must implement \"{0}\".", typeof(IInitializableModule)), nameof(dependencies));

			this.ModuleDependencyAttribute = new ModuleDependencyAttribute(dependencyArray);
		}

		public BeforeModuleDependencyAttribute(params Type[] dependencies) : this((IEnumerable<Type>) dependencies) { }

		#endregion

		#region Properties

		public ICollection<Type> Dependencies => this.ModuleDependencyAttribute.Dependencies;
		private ModuleDependencyAttribute ModuleDependencyAttribute { get; }

		#endregion

		#region Methods

		private static Type ValidateDependency(Type dependency)
		{
			if(dependency == null)
				throw new ArgumentNullException(nameof(dependency));

			if(!typeof(IInitializableModule).IsAssignableFrom(dependency))
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The dependency must implement \"{0}\".", typeof(IInitializableModule)), nameof(dependency));

			return dependency;
		}

		#endregion
	}
}