using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HansKindberg.IdentityServer.Data.Transferring
{
	public interface IDataExportResult
	{
		#region Properties

		IDictionary<Type, IEnumerable<object>> Instances { get; }

		#endregion

		#region Methods

		string ToJson(IContractResolver contractResolver = null, Formatting? formatting = null, NullValueHandling? nullValueHandling = null);

		#endregion
	}
}