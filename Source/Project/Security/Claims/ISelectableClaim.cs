using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Security.Claims
{
	public interface ISelectableClaim
	{
		#region Properties

		IReadOnlyDictionary<string, string> Details { get; }
		string Id { get; }
		bool Selected { get; }
		string Text { get; }
		string Value { get; }

		#endregion
	}
}