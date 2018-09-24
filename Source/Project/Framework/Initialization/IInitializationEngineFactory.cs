using EPiServer.Framework.Initialization;

namespace RegionOrebroLan.EPiServer.Framework.Initialization
{
	public interface IInitializationEngineFactory
	{
		#region Methods

		IInitializationEngine Create(HostType hostType);

		#endregion
	}
}