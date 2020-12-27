using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HansKindberg.IdentityServer.Data.Transferring
{
	/// <summary>
	/// Export data from the database.
	/// </summary>
	public interface IDataExporter
	{
		#region Properties

		IEnumerable<Type> ExportableTypes { get; }

		#endregion

		#region Methods

		Task<IDataExportResult> ExportAsync(IEnumerable<Type> types);

		#endregion
	}
}