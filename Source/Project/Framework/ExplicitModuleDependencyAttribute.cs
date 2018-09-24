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
	public sealed class ExplicitModuleDependencyAttribute : Attribute
	{
		#region Constructors

		public ExplicitModuleDependencyAttribute(Type type, Type dependency) : this(type, new[] {ValidateType(dependency, nameof(dependency))}) { }
		public ExplicitModuleDependencyAttribute(Type type, IEnumerable<Type> dependencies) : this(new[] {ValidateType(type)}, dependencies) { }
		public ExplicitModuleDependencyAttribute(Type type, params Type[] dependencies) : this(type, (IEnumerable<Type>) dependencies) { }
		public ExplicitModuleDependencyAttribute(IEnumerable<Type> types, Type dependency) : this(types, new[] {ValidateType(dependency, nameof(dependency))}) { }

		public ExplicitModuleDependencyAttribute(IEnumerable<Type> types, IEnumerable<Type> dependencies)
		{
			var typeArray = types?.ToArray();

			if(typeArray == null)
				throw new ArgumentNullException(nameof(types));

			if(typeArray.Any(type => type == null))
				throw new ArgumentException("Types can not contain null values.", nameof(types));

			if(typeArray.Any(type => !typeof(IInitializableModule).IsAssignableFrom(type)))
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "All types must implement \"{0}\".", typeof(IInitializableModule)), nameof(types));

			var dependencyArray = dependencies?.ToArray();

			if(dependencyArray == null)
				throw new ArgumentNullException(nameof(dependencies));

			if(dependencyArray.Any(type => type == null))
				throw new ArgumentException("Dependencies can not contain null values.", nameof(dependencies));

			if(dependencyArray.Any(type => !typeof(IInitializableModule).IsAssignableFrom(type)))
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "All dependencies must implement \"{0}\".", typeof(IInitializableModule)), nameof(dependencies));

			this.Types = typeArray.ToList();
			this.Dependencies = dependencyArray.ToList();
		}

		#endregion

		#region Properties

		public ICollection<Type> Dependencies { get; }
		public ICollection<Type> Types { get; }

		#endregion

		#region Methods

		private static Type ValidateType(Type type, string parameterName = null)
		{
			parameterName = parameterName ?? nameof(type);

			if(type == null)
				throw new ArgumentNullException(parameterName);

			if(!typeof(IInitializableModule).IsAssignableFrom(type))
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The {0} must implement \"{1}\".", parameterName, typeof(IInitializableModule)), parameterName);

			return type;
		}

		#endregion
	}
}