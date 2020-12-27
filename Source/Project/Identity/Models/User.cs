namespace HansKindberg.IdentityServer.Identity.Models
{
	public class User
	{
		#region Properties

		public virtual string Email { get; set; }
		public virtual string Password { get; set; }
		public virtual string UserName { get; set; }

		#endregion
	}
}