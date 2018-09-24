using EPiServer.Framework.Initialization;

namespace RegionOrebroLan.EPiServer.Framework.Initialization
{
	public interface IInitializationEngineReplacement : IInitializationEngine
	{
		#region Properties

		DisabledInitializationEngine OriginalInitializationEngine { get; }

		#endregion
	}
}