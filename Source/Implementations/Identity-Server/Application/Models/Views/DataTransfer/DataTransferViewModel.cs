using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Application.Models.Views.DataTransfer
{
	public class DataTransferViewModel
	{
		#region Properties

		public virtual IDictionary<string, int> ExistingData { get; } = new SortedDictionary<string, int>();

		#endregion
	}
}