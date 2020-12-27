using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Data.Transferring
{
	/// <inheritdoc />
	public class DataImportResultItem : IDataImportResultItem
	{
		#region Properties

		public virtual int Adds { get; set; }
		public virtual int After { get; set; }
		public virtual int Before { get; set; }
		public virtual int Deletes { get; set; }
		public virtual IDictionary<Type, IDataImportResultItem> Relations { get; } = new Dictionary<Type, IDataImportResultItem>();
		public virtual int Updates { get; set; }

		#endregion
	}
}