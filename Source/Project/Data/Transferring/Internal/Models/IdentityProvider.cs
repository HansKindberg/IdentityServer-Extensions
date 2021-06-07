using System.Diagnostics.CodeAnalysis;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal.Models
{
	/// <summary>
	/// This class is needed because Duende.IdentityServer.Models.IdentityProvider doesn't have a parameterless constructor.
	/// </summary>
	[SuppressMessage("Naming", "CA1724:Type names should not match namespaces")]
	public class IdentityProvider : Duende.IdentityServer.Models.IdentityProvider
	{
		#region Constructors

		public IdentityProvider() : base(string.Empty) { }

		#endregion
	}
}