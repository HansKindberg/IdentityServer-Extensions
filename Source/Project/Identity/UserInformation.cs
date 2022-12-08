namespace HansKindberg.IdentityServer.Identity
{
	public class UserInformation
	{
		#region Properties

		public virtual string UniqueIdentifier { get; set; }
		public virtual User User { get; set; }

		#endregion
	}
}