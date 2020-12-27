using System;
using Microsoft.Extensions.Configuration;

namespace HansKindberg.IdentityServer.Data.Transferring.Extensions
{
	public static class DataImporterExtension
	{
		#region Methods

		public static IDataImportResult Import(this IDataImporter dataImporter, IConfiguration configuration)
		{
			if(dataImporter == null)
				throw new ArgumentNullException(nameof(dataImporter));

			return dataImporter.ImportAsync(configuration, new ImportOptions()).Result;
		}

		#endregion
	}
}