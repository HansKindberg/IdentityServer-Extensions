using IdentityServer4;

namespace HansKindberg.IdentityServer
{
	public class ExtendedIdentityServerUser : IdentityServerUser
	{
		#region Constructors

		public ExtendedIdentityServerUser(string subjectId) : base(subjectId) { }

		#endregion

		#region Properties

		public virtual string ProviderUserId { get; set; }

		#endregion
	}
}