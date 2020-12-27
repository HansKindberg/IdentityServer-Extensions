using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Data.Transferring
{
	/// <inheritdoc />
	public class DataImportResult : IDataImportResult
	{
		#region Properties

		public virtual IList<string> Errors { get; } = new List<string>();
		public virtual IDictionary<Type, IDataImportResultItem> Items { get; } = new Dictionary<Type, IDataImportResultItem>();
		public virtual int SavedChanges { get; set; }

		#endregion
	}
}