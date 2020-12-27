using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public interface IPartialImporter
	{
		#region Methods

		Task ImportAsync(IConfiguration configuration, ImportOptions options, IDataImportResult result);

		#endregion
	}
}