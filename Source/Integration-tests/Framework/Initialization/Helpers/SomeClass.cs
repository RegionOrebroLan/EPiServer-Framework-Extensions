using EPiServer.Framework.Localization;
using RegionOrebroLan.EPiServer.Framework;

namespace RegionOrebroLan.EPiServer.IntegrationTests.Framework.Initialization.Helpers
{
	[ExplicitModuleDependency(typeof(ProviderBasedLocalizationService), typeof(InitializableModule))]
	public class SomeClass { }
}