using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HansKindberg.IdentityServer.Data.Transferring
{
	/// <summary>
	/// Import data to the database. Add, update and optionally delete data in the database.
	/// </summary>
	public interface IDataImporter
	{
		#region Methods

		/// <summary>
		/// Import entities from configuration.
		/// </summary>
		/// <param name="configuration">The configuration to import from.</param>
		/// <param name="options">Options for the import.</param>
		/// <returns></returns>
		Task<IDataImportResult> ImportAsync(IConfiguration configuration, ImportOptions options);

		#endregion
	}
}