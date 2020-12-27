using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Data.Transferring
{
	public interface IDataImportResultItem
	{
		#region Properties

		int Adds { get; set; }
		int After { get; set; }
		int Before { get; set; }
		int Deletes { get; set; }
		IDictionary<Type, IDataImportResultItem> Relations { get; }
		int Updates { get; set; }

		#endregion
	}
}