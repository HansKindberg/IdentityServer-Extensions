namespace HansKindberg.IdentityServer.Identity.Models
{
	public class UserLogin
	{
		#region Properties

		public virtual string Id { get; set; }
		public virtual string Provider { get; set; }
		public virtual string UserIdentifier { get; set; }

		#endregion
	}
}