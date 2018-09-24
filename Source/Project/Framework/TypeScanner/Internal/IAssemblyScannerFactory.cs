using EPiServer.Framework.TypeScanner.Internal;

namespace RegionOrebroLan.EPiServer.Framework.TypeScanner.Internal
{
	public interface IAssemblyScannerFactory
	{
		#region Methods

		IAssemblyScanner Create();

		#endregion
	}
}