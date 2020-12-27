using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Data.Transferring
{
	public interface IDataImportResult
	{
		#region Properties

		IList<string> Errors { get; }
		IDictionary<Type, IDataImportResultItem> Items { get; }
		int SavedChanges { get; }

		#endregion
	}
}