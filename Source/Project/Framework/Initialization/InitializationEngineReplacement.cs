using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.TypeScanner;
using EPiServer.Framework.TypeScanner.Internal;
using EPiServer.Logging.Compatibility;
using EPiServer.ServiceLocation;

namespace RegionOrebroLan.EPiServer.Framework.Initialization
{
	public class InitializationEngineReplacement : IInitializationEngineReplacement
	{
		#region Fields

		private IAssemblyScanner _assemblyScanner;
		private static IEnumerable<Type> _attributeTypesOfInterest;
		private static readonly IEnumerable<Type> _extraAttributeTypesOfInterest = new[] {typeof(ExplicitModuleDependencyAttribute)};
		private static PropertyInfo _initializationEngineAssemblyScannerProperty;
		private Action<ServiceConfigurationContext> _initializationEngineAssignServiceLocatorAction;
		private Func<ServiceConfigurationContext> _initializationEngineGetServiceConfigurationContextFunction;
		private Func<ITypeScannerLookup> _initializationEngineGetTypeScannerLookupFunction;
		private static FieldInfo _initializationEngineInitializationStateField;
		private static FieldInfo _initializationEngineIsInitializedField;
		private Action _initializationEngineRunConfigurationTransformsAction;
		private static readonly Type _initializationEngineType = typeof(InitializationEngine);
		private ICollection<IInitializableModule> _initializedModules;
		private static readonly ILog _logger = LogManager.GetLogger(typeof(InitializationEngineReplacement));
		private IEnumerable<IInitializableModule> _modules;
		private ServiceConfigurationContext _serviceConfigurationContext;
		private Action _serviceConfigurationContextRaiseConfigurationCompleteAction;
		private ITypeScannerLookup _typeScannerLookup;
		private static readonly IEnumerable<Type> _validInitializableModuleAttributeTypes = new[] {typeof(BeforeModuleDependencyAttribute), typeof(InitializableModuleAttribute), typeof(ModuleDependencyAttribute)};

		#endregion

		#region Constructors

		public InitializationEngineReplacement(DisabledInitializationEngine originalInitializationEngine) : this(_logger, originalInitializationEngine) { }

		protected internal InitializationEngineReplacement(ILog logger, DisabledInitializationEngine originalInitializationEngine)
		{
			this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.OriginalInitializationEngine = originalInitializationEngine ?? throw new ArgumentNullException(nameof(originalInitializationEngine));

			originalInitializationEngine.InitComplete += this.OnInitializationComplete;
		}

		#endregion

		#region Events

		public virtual event EventHandler InitComplete;

		#endregion

		#region Properties

		protected internal virtual IEnumerable<Assembly> Assemblies
		{
			get => this.OriginalInitializationEngine.Assemblies;
			set
			{
				this.OriginalInitializationEngine.Assemblies = value ?? throw new ArgumentNullException(nameof(value));

				this.TypeScannerLookup = null;
			}
		}

		protected internal virtual IAssemblyScanner AssemblyScanner
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._assemblyScanner == null)
				{
					var assemblyScanner = (IAssemblyScanner) this.InitializationEngineAssemblyScannerProperty.GetValue(this.OriginalInitializationEngine);

					this._assemblyScanner = assemblyScanner ?? throw new InvalidOperationException("The assembly-scanner can not be null.");
				}
				// ReSharper restore InvertIf

				return this._assemblyScanner;
			}
		}

		protected internal virtual IEnumerable<Type> AttributeTypesOfInterest
		{
			get
			{
				// ReSharper disable InvertIf
				if(_attributeTypesOfInterest == null)
				{
					var attributeTypesOfInterest = new HashSet<Type>(this.ValidInitializableModuleAttributeTypes.Concat(this.ExtraAttributeTypesOfInterest));

					_attributeTypesOfInterest = attributeTypesOfInterest.ToArray();
				}
				// ReSharper restore InvertIf

				return _attributeTypesOfInterest;
			}
		}

		protected internal virtual IEnumerable<Type> ExtraAttributeTypesOfInterest => _extraAttributeTypesOfInterest;
		public virtual HostType HostType => this.OriginalInitializationEngine.HostType;

		protected internal virtual PropertyInfo InitializationEngineAssemblyScannerProperty
		{
			get
			{
				if(_initializationEngineAssemblyScannerProperty == null)
					_initializationEngineAssemblyScannerProperty = this.InitializationEngineType.GetProperty("AssemblyScanner", BindingFlags.Instance | BindingFlags.NonPublic);

				return _initializationEngineAssemblyScannerProperty;
			}
		}

		protected internal virtual Action<ServiceConfigurationContext> InitializationEngineAssignServiceLocatorAction
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._initializationEngineAssignServiceLocatorAction == null)
				{
					var method = this.InitializationEngineType.GetMethod("AssignServiceLocator", BindingFlags.Instance | BindingFlags.NonPublic);

					this._initializationEngineAssignServiceLocatorAction = this.CreateDelegate<Action<ServiceConfigurationContext>>(this.OriginalInitializationEngine, method);
				}
				// ReSharper restore InvertIf

				return this._initializationEngineAssignServiceLocatorAction;
			}
		}

		protected internal virtual Func<ServiceConfigurationContext> InitializationEngineGetServiceConfigurationContextFunction
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._initializationEngineGetServiceConfigurationContextFunction == null)
				{
					var method = this.InitializationEngineType.GetMethod("GetServiceConfigurationContext", BindingFlags.Instance | BindingFlags.NonPublic);

					this._initializationEngineGetServiceConfigurationContextFunction = this.CreateDelegate<Func<ServiceConfigurationContext>>(this.OriginalInitializationEngine, method);
				}
				// ReSharper restore InvertIf

				return this._initializationEngineGetServiceConfigurationContextFunction;
			}
		}

		protected internal virtual Func<ITypeScannerLookup> InitializationEngineGetTypeScannerLookupFunction
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._initializationEngineGetTypeScannerLookupFunction == null)
				{
					var method = this.InitializationEngineType.GetMethod("GetTypeScannerLookup", BindingFlags.Instance | BindingFlags.NonPublic);

					this._initializationEngineGetTypeScannerLookupFunction = this.CreateDelegate<Func<ITypeScannerLookup>>(this.OriginalInitializationEngine, method);
				}
				// ReSharper restore InvertIf

				return this._initializationEngineGetTypeScannerLookupFunction;
			}
		}

		protected internal virtual FieldInfo InitializationEngineInitializationStateField
		{
			get
			{
				if(_initializationEngineInitializationStateField == null)
					_initializationEngineInitializationStateField = this.InitializationEngineType.GetField("_initializationState", BindingFlags.Instance | BindingFlags.NonPublic);

				return _initializationEngineInitializationStateField;
			}
		}

		protected internal virtual FieldInfo InitializationEngineIsInitializedField
		{
			get
			{
				if(_initializationEngineIsInitializedField == null)
					_initializationEngineIsInitializedField = this.InitializationEngineType.GetField("_isInitialized", BindingFlags.Instance | BindingFlags.NonPublic);

				return _initializationEngineIsInitializedField;
			}
		}

		protected internal virtual Action InitializationEngineRunConfigurationTransformsAction
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._initializationEngineRunConfigurationTransformsAction == null)
				{
					var method = this.InitializationEngineType.GetMethod("RunConfigurationTransforms", BindingFlags.NonPublic | BindingFlags.Static);

					this._initializationEngineRunConfigurationTransformsAction = this.CreateDelegate<Action>(method);
				}
				// ReSharper restore InvertIf

				return this._initializationEngineRunConfigurationTransformsAction;
			}
		}

		protected internal virtual Type InitializationEngineType => _initializationEngineType;

		public virtual InitializationState InitializationState
		{
			get => this.OriginalInitializationEngine.InitializationState;
			protected internal set => this.InitializationEngineInitializationStateField.SetValue(this.OriginalInitializationEngine, value);
		}

		protected internal virtual ICollection<IInitializableModule> InitializedModules
		{
			get
			{
				// ReSharper disable ConvertIfStatementToNullCoalescingExpression
				if(this._initializedModules == null)
					this._initializedModules = (HashSet<IInitializableModule>) this.InitializationEngineIsInitializedField.GetValue(this.OriginalInitializationEngine);
				// ReSharper restore ConvertIfStatementToNullCoalescingExpression

				return this._initializedModules;
			}
		}

		protected internal virtual ILog Logger { get; }

		//public virtual IEnumerable<IInitializableModule> Modules
		//{
		//	get => this._modules ?? (this._modules = this.OriginalInitializationEngine.Modules = this.GetInitializableModules());
		//	set => this._modules = this.OriginalInitializationEngine.Modules = value;
		//}

		public virtual IEnumerable<IInitializableModule> Modules
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._modules == null)
				{
					var modules = this.GetInitializableModules().ToArray();

					this._modules = modules;
					this.OriginalInitializationEngine.SetModules(modules);
					//=> this._modules ?? (this._modules = this.OriginalInitializationEngine.Modules = this.GetInitializableModules());
				}
				// ReSharper restore InvertIf

				return this._modules;
			}
			set
			{
				this._modules = value;
				this.OriginalInitializationEngine.SetModules(value);
			}
		}

		public virtual DisabledInitializationEngine OriginalInitializationEngine { get; }

		public virtual ServiceConfigurationContext ServiceConfigurationContext
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._serviceConfigurationContext == null)
				{
					var serviceConfigurationContext = this.InitializationEngineGetServiceConfigurationContextFunction.Invoke();

					this._serviceConfigurationContext = serviceConfigurationContext ?? throw new InvalidOperationException("The service-configuration-context can not be null.");
				}
				// ReSharper restore InvertIf

				return this._serviceConfigurationContext;
			}
		}

		protected internal virtual Action ServiceConfigurationContextRaiseConfigurationCompleteAction
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._serviceConfigurationContextRaiseConfigurationCompleteAction == null)
				{
					var method = typeof(ServiceConfigurationContext).GetMethod("RaiseConfigurationComplete", BindingFlags.Instance | BindingFlags.NonPublic);

					this._serviceConfigurationContextRaiseConfigurationCompleteAction = this.CreateDelegate<Action>(this.ServiceConfigurationContext, method);
				}
				// ReSharper restore InvertIf

				return this._serviceConfigurationContextRaiseConfigurationCompleteAction;
			}
		}

		protected internal virtual ITypeScannerLookup TypeScannerLookup
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._typeScannerLookup == null)
				{
					var typeScannerLookup = this.InitializationEngineGetTypeScannerLookupFunction.Invoke();

					this._typeScannerLookup = typeScannerLookup ?? throw new InvalidOperationException("The type-scanner-lookup can not be null.");
				}
				// ReSharper restore InvertIf

				return this._typeScannerLookup;
			}
			set => this._typeScannerLookup = value;
		}

		protected internal virtual IEnumerable<Type> ValidInitializableModuleAttributeTypes => _validInitializableModuleAttributeTypes;

		#endregion

		#region Methods

		protected internal virtual void AdjustDependenciesForInitializableModuleTypeMap(IDictionary<Type, IList<Type>> initializableModuleTypeMap)
		{
			if(initializableModuleTypeMap == null)
				throw new ArgumentNullException(nameof(initializableModuleTypeMap));

			var types = initializableModuleTypeMap.Select(mapping => mapping.Key).Distinct().ToArray();

			foreach(var mapping in initializableModuleTypeMap)
			{
				for(var i = mapping.Value.Count - 1; i >= 0; i--)
				{
					if(types.Contains(mapping.Value[i]))
						continue;

					mapping.Value.RemoveAt(i);
				}
			}
		}

		protected internal virtual void AssignServiceLocator(ServiceConfigurationContext serviceConfigurationContext)
		{
			this.InitializationEngineAssignServiceLocatorAction.Invoke(serviceConfigurationContext);
		}

		protected internal virtual int CompareInitializableModuleTypeMappings(KeyValuePair<Type, IList<Type>> firstInitializableModuleTypeMapping, KeyValuePair<Type, IList<Type>> secondInitializableModuleTypeMapping)
		{
			if(this.TryCompareInitializableModuleTypeMappingsByReference(firstInitializableModuleTypeMapping, secondInitializableModuleTypeMapping, out var compare))
				return compare;

			// ReSharper disable ConvertIfStatementToReturnStatement

			if(this.TryCompareInitializableModuleTypeMappingsByNumberOfDependencies(firstInitializableModuleTypeMapping, secondInitializableModuleTypeMapping, out compare))
				return compare;

			// ReSharper restore ConvertIfStatementToReturnStatement

			return string.Compare(firstInitializableModuleTypeMapping.Key.FullName, secondInitializableModuleTypeMapping.Key.FullName, StringComparison.OrdinalIgnoreCase);
		}

		public virtual void Configure()
		{
			if(this.InitializationState != InitializationState.PreInitialize)
				throw new InvalidOperationException("Configure must be called on uninitialized engine.");

			this.ExecuteTransition(false);
		}

		protected internal virtual void ConfigureModules(IEnumerable<IInitializableModule> modules)
		{
			if(modules == null)
				throw new ArgumentNullException(nameof(modules));

			foreach(var module in modules.OfType<IConfigurableModule>())
			{
				module.ConfigureContainer(this.ServiceConfigurationContext);
			}

			this.RaiseConfigurationComplete();

			this.AssignServiceLocator(this.ServiceConfigurationContext);
		}

		protected internal virtual T CreateDelegate<T>(MethodInfo method)
		{
			return this.CreateDelegate<T>(null, method);
		}

		protected internal virtual T CreateDelegate<T>(object instance, MethodInfo method)
		{
			return (T) (object) Delegate.CreateDelegate(typeof(T), instance, method);
		}

		protected internal virtual void ExecuteTransition(bool continueTransitions)
		{
			this.ValidateHostType();

			// ReSharper disable LoopVariableIsNeverChangedInsideLoop
			do
			{
				switch(this.InitializationState)
				{
					case InitializationState.PreInitialize:
						this.ConfigureModules(this.Modules);
						this.InitializationState = InitializationState.Initializing;
						break;
					case InitializationState.Initializing:
						this.InitializeModules(this.Modules);
						if(this.InitializationState == InitializationState.InitializeDelayed)
							return;
						goto case InitializationState.InitializeComplete;
					case InitializationState.InitializeDelayed:
					case InitializationState.InitializeFailed:
					case InitializationState.Uninitialized:
						this.InitializationState = InitializationState.Initializing;
						break;
					case InitializationState.InitializeComplete:
						this.OnInitializationComplete(this, EventArgs.Empty);
						this.InitializationState = InitializationState.Initialized;
						break;
					case InitializationState.Initialized:
					case InitializationState.UninitializeFailed:
					case InitializationState.Uninitializing:
					case InitializationState.WaitingBeginRequest:
						return;
					default:
						return;
				}
			} while(continueTransitions);
			// ReSharper restore LoopVariableIsNeverChangedInsideLoop
		}

		protected internal virtual IDictionary<Type, IEnumerable<Type>> GetExplicitDependencyMap(IDictionary<Type, IEnumerable<Attribute>> typeToAttributeMap)
		{
			if(typeToAttributeMap == null)
				throw new ArgumentNullException(nameof(typeToAttributeMap));

			var explicitDependencyMap = new Dictionary<Type, HashSet<Type>>();

			foreach(var mapping in typeToAttributeMap)
			{
				var explicitModuleDependencyAttributes = mapping.Value.OfType<ExplicitModuleDependencyAttribute>().ToArray();

				if(!explicitModuleDependencyAttributes.Any())
					continue;

				foreach(var explicitModuleDependencyAttribute in explicitModuleDependencyAttributes)
				{
					foreach(var type in explicitModuleDependencyAttribute.Types)
					{
						if(!this.IsInitializableModuleType(type))
							throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The type \"{0}\" is included in an explicit-module-dependency-attribute on type \"{1}\" but it does not implement \"{2}\". It must implement either \"{2}\" or \"{3}\".", type, mapping.Key, typeof(IInitializableModule), typeof(IConfigurableModule)));
					}

					foreach(var dependency in explicitModuleDependencyAttribute.Dependencies)
					{
						if(!this.IsInitializableModuleType(dependency))
							throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The dependency \"{0}\" is included in an explicit-module-dependency-attribute on type \"{1}\" but it does not implement \"{2}\". It must implement either \"{2}\" or \"{3}\".", dependency, mapping.Key, typeof(IInitializableModule), typeof(IConfigurableModule)));
					}

					if(!explicitModuleDependencyAttribute.Dependencies.Any())
						continue;

					foreach(var type in explicitModuleDependencyAttribute.Types)
					{
						if(!explicitDependencyMap.TryGetValue(type, out var list))
						{
							list = new HashSet<Type>();
							explicitDependencyMap.Add(type, list);
						}

						foreach(var dependency in explicitModuleDependencyAttribute.Dependencies)
						{
							list.Add(dependency);
						}
					}
				}
			}

			return explicitDependencyMap.ToDictionary(item => item.Key, item => (IEnumerable<Type>) item.Value);
		}

		protected internal virtual IEnumerable<IInitializableModule> GetInitializableModules()
		{
			var initializableModuleTypeMap = this.GetInitializableModuleTypeMap();

			var initializableModuleTypeMapAsList = initializableModuleTypeMap.ToList();

			initializableModuleTypeMapAsList.Sort(this.CompareInitializableModuleTypeMappings);

			initializableModuleTypeMap = initializableModuleTypeMapAsList.ToDictionary(item => item.Key, item => item.Value);

			var sortedInitializableModuleTypes = new List<Type>();

			this.PopulateSortedInitializableModuleTypes(initializableModuleTypeMap, sortedInitializableModuleTypes);

			var initializableModules = sortedInitializableModuleTypes.Select(type => (IInitializableModule) Activator.CreateInstance(type)).ToArray();

			return initializableModules;
		}

		protected internal virtual IDictionary<Type, IList<Type>> GetInitializableModuleTypeMap()
		{
			var initializableModuleTypeMap = new Dictionary<Type, IList<Type>>();

			var typeToAttributeMap = this.GetTypeToAttributeMap();
			var initializableModuleTypeToAttributeMap = this.GetInitializableModuleTypeToAttributeMap(typeToAttributeMap);

			var beforeDependencyMap = this.GetInitializableModuleTypeToBeforeDependencyMap(initializableModuleTypeToAttributeMap);
			var dependencyMap = this.GetInitializableModuleTypeToDependencyMap(initializableModuleTypeToAttributeMap);
			var explicitDependencyMap = this.GetExplicitDependencyMap(typeToAttributeMap);

			foreach(var mapping in dependencyMap)
			{
				var dependencies = beforeDependencyMap.Where(item => item.Value.Contains(mapping.Key)).Select(item => item.Key).ToArray();

				foreach(var dependency in dependencies)
				{
					mapping.Value.Add(dependency);
				}

				dependencies = explicitDependencyMap.Where(item => item.Key == mapping.Key).SelectMany(item => item.Value).ToArray();

				foreach(var dependency in dependencies)
				{
					mapping.Value.Add(dependency);
				}

				initializableModuleTypeMap.Add(mapping.Key, mapping.Value.ToList());
			}

			return initializableModuleTypeMap;
		}

		protected internal virtual IDictionary<Type, IEnumerable<Attribute>> GetInitializableModuleTypeToAttributeMap(IDictionary<Type, IEnumerable<Attribute>> typeToAttributeMap)
		{
			if(typeToAttributeMap == null)
				throw new ArgumentNullException(nameof(typeToAttributeMap));

			var initializableModuleTypeToAttributeMap = new Dictionary<Type, IEnumerable<Attribute>>();
			var validInitializableModuleAttributeTypesValue = string.Join(", ", this.ValidInitializableModuleAttributeTypes);

			foreach(var mapping in typeToAttributeMap)
			{
				if(!mapping.Value.Any(this.IsValidInitializableModuleAttribute))
					continue;

				if(!this.IsInitializableModuleType(mapping.Key))
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The type \"{0}\" is decorated with an initializable-module attribute ({1}) but it does not implement \"{2}\". It must implement either \"{2}\" or \"{3}\".", mapping.Key, validInitializableModuleAttributeTypesValue, typeof(IInitializableModule), typeof(IConfigurableModule)));

				initializableModuleTypeToAttributeMap.Add(mapping.Key, mapping.Value);
			}

			return initializableModuleTypeToAttributeMap;
		}

		protected internal virtual IDictionary<Type, IEnumerable<Type>> GetInitializableModuleTypeToBeforeDependencyMap(IDictionary<Type, IEnumerable<Attribute>> initializableModuleTypeToAttributeMap)
		{
			if(initializableModuleTypeToAttributeMap == null)
				throw new ArgumentNullException(nameof(initializableModuleTypeToAttributeMap));

			var beforeDependencyMap = new Dictionary<Type, IEnumerable<Type>>();

			foreach(var mapping in initializableModuleTypeToAttributeMap)
			{
				var beforeDependencies = new HashSet<Type>(mapping.Value.OfType<BeforeModuleDependencyAttribute>().SelectMany(item => item.Dependencies));

				if(beforeDependencies.Any())
					beforeDependencyMap.Add(mapping.Key, beforeDependencies);
			}

			return beforeDependencyMap;
		}

		protected internal virtual IDictionary<Type, HashSet<Type>> GetInitializableModuleTypeToDependencyMap(IDictionary<Type, IEnumerable<Attribute>> initializableModuleTypeToAttributeMap)
		{
			if(initializableModuleTypeToAttributeMap == null)
				throw new ArgumentNullException(nameof(initializableModuleTypeToAttributeMap));

			var dependencyMap = new Dictionary<Type, HashSet<Type>>();

			foreach(var mapping in initializableModuleTypeToAttributeMap)
			{
				var dependencies = new HashSet<Type>(mapping.Value.OfType<ModuleDependencyAttribute>().SelectMany(item => item.Dependencies));

				dependencyMap.Add(mapping.Key, dependencies);
			}

			return dependencyMap;
		}

		protected internal virtual string GetNameForLogging(Action<InitializationEngine> action)
		{
			if(action == null)
				throw new ArgumentNullException(nameof(action));

			// ReSharper disable PossibleNullReferenceException
			return action.Method.Name + " on class " + action.Method.DeclaringType.AssemblyQualifiedName;
			// ReSharper restore PossibleNullReferenceException
		}

		protected internal virtual IDictionary<Type, IEnumerable<Attribute>> GetTypeToAttributeMap()
		{
			var typeToAttributeMap = new Dictionary<Type, IEnumerable<Attribute>>();

			foreach(var type in this.Assemblies.SelectMany(assembly => assembly.GetTypes()))
			{
				var attributes = type.GetCustomAttributes(false).Cast<Attribute>().Where(this.IsAttributeOfInterest).ToArray();

				if(!attributes.Any())
					continue;

				typeToAttributeMap.Add(type, attributes);
			}

			return typeToAttributeMap;
		}

		public virtual void Initialize()
		{
			this.ExecuteTransition(true);
		}

		protected internal virtual void InitializeModules(IEnumerable<IInitializableModule> modules)
		{
			if(modules == null)
				throw new ArgumentNullException(nameof(modules));

			this.RunConfigurationTransforms();

			foreach(var module in modules)
			{
				if(this.IsInitialized(module))
					continue;

				var nameForLogging = this.GetNameForLogging(module.Initialize);
				try
				{
					module.Initialize(this.OriginalInitializationEngine);
				}
				catch(TerminateInitializationException exception)
				{
					if(this.Logger.IsWarnEnabled)
						this.Logger.Warn(string.Format(CultureInfo.InvariantCulture, "Initialize action \"{0}\" terminated processing.", nameForLogging), exception);

					this.InitializationState = InitializationState.InitializeDelayed;

					break;
				}
				catch(Exception exception)
				{
					if(this.Logger.IsErrorEnabled)
						this.Logger.Error(string.Format(CultureInfo.InvariantCulture, "Initialize action failed for \"{0}\".", nameForLogging), exception);

					this.InitializationState = InitializationState.InitializeFailed;

					throw new InitializationException(string.Format(CultureInfo.InvariantCulture, "Initialize action failed for {0}.", nameForLogging), exception, new[] {module.GetType()});
				}

				if(this.Logger.IsDebugEnabled)
					this.Logger.DebugFormat("Initialize action successful for \"{0}\".", nameForLogging);

				this.InitializedModules.Add(module);
			}
		}

		protected internal virtual bool IsAttributeOfInterest(Attribute attribute)
		{
			if(attribute == null)
				return false;

			// ReSharper disable ConvertIfStatementToReturnStatement
			if(this.AttributeTypesOfInterest.Contains(attribute.GetType()))
				return true;
			// ReSharper restore ConvertIfStatementToReturnStatement

			return false;
		}

		protected internal virtual bool IsInitializableModuleType(Type type)
		{
			return type != null && typeof(IInitializableModule).IsAssignableFrom(type);
		}

		[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
		protected internal virtual bool IsInitialized(IInitializableModule module)
		{
			return this.OriginalInitializationEngine.IsInitialized(module);
		}

		protected internal virtual bool IsValidInitializableModuleAttribute(Attribute attribute)
		{
			if(attribute == null)
				return false;

			// ReSharper disable ConvertIfStatementToReturnStatement
			if(this.ValidInitializableModuleAttributeTypes.Contains(attribute.GetType()))
				return true;
			// ReSharper restore ConvertIfStatementToReturnStatement

			return false;
		}

		protected internal virtual void OnInitializationComplete(object sender, EventArgs e)
		{
			this.InitComplete?.Invoke(sender, e);
		}

		protected internal virtual void PopulateSortedInitializableModuleTypes(IDictionary<Type, IList<Type>> initializableModuleTypeMap, IList<Type> sortedInitializableModuleTypes)
		{
			if(initializableModuleTypeMap == null)
				throw new ArgumentNullException(nameof(initializableModuleTypeMap));

			if(sortedInitializableModuleTypes == null)
				throw new ArgumentNullException(nameof(sortedInitializableModuleTypes));

			while(initializableModuleTypeMap.Any())
			{
				var initializableModuleTypeWithoutDependenciesMap = initializableModuleTypeMap.Where(mapping => !mapping.Value.Any()).ToArray();

				if(initializableModuleTypeWithoutDependenciesMap.Any())
				{
					foreach(var mapping in initializableModuleTypeWithoutDependenciesMap)
					{
						initializableModuleTypeMap.Remove(mapping);
						sortedInitializableModuleTypes.Add(mapping.Key);
					}
				}
				else
				{
					this.AdjustDependenciesForInitializableModuleTypeMap(initializableModuleTypeMap);
				}
			}
		}

		[SuppressMessage("Design", "CA1030:Use events where appropriate")]
		protected internal virtual void RaiseConfigurationComplete()
		{
			this.ServiceConfigurationContextRaiseConfigurationCompleteAction.Invoke();
		}

		protected internal virtual void RunConfigurationTransforms()
		{
			this.InitializationEngineRunConfigurationTransformsAction.Invoke();
		}

		protected internal virtual bool TryCompareInitializableModuleTypeMappingsByNumberOfDependencies(KeyValuePair<Type, IList<Type>> firstInitializableModuleTypeMapping, KeyValuePair<Type, IList<Type>> secondInitializableModuleTypeMapping, out int compare)
		{
			compare = firstInitializableModuleTypeMapping.Value.Count().CompareTo(secondInitializableModuleTypeMapping.Value.Count());

			return compare != 0;
		}

		protected internal virtual bool TryCompareInitializableModuleTypeMappingsByReference(KeyValuePair<Type, IList<Type>> firstInitializableModuleTypeMapping, KeyValuePair<Type, IList<Type>> secondInitializableModuleTypeMapping, out int compare)
		{
			compare = 0;

			if(ReferenceEquals(firstInitializableModuleTypeMapping.Key, secondInitializableModuleTypeMapping.Key))
			{
				compare = 0;
				return true;
			}

			if(firstInitializableModuleTypeMapping.Key == null)
			{
				compare = -1;
				return true;
			}

			// ReSharper disable InvertIf
			if(secondInitializableModuleTypeMapping.Key == null)
			{
				compare = 1;
				return true;
			}
			// ReSharper restore InvertIf

			return false;
		}

		public virtual void Uninitialize()
		{
			this.InitializationState = InitializationState.Uninitializing;

			foreach(var module in this.Modules.Reverse())
			{
				if(!this.IsInitialized(module))
					continue;

				var nameForLogging = this.GetNameForLogging(module.Uninitialize);

				try
				{
					module.Uninitialize(this.OriginalInitializationEngine);
				}
				catch(Exception exception)
				{
					if(this.Logger.IsErrorEnabled)
						this.Logger.Error(string.Format(CultureInfo.InvariantCulture, "Uninitialize action failed for \"{0}\".", nameForLogging), exception);

					this.InitializationState = InitializationState.UninitializeFailed;

					throw;
				}

				if(this.Logger.IsDebugEnabled)
					this.Logger.DebugFormat("Uninitialize action successful for \"{0}\".", nameForLogging);

				this.InitializedModules.Remove(module);
			}

			this.InitializationState = InitializationState.Uninitialized;
		}

		protected internal virtual void ValidateHostType()
		{
			if(this.HostType != HostType.LegacyMirroringAppDomain && this.HostType != HostType.TestFramework && this.HostType != HostType.WebApplication)
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Initializing with unsupported host type \"{0}\".", this.HostType));
		}

		#endregion
	}
}