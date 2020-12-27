namespace HansKindberg.IdentityServer.Configuration
{
	public class RedirectionOptions
	{
		#region Properties

		public virtual byte SecondsBeforeRedirect { get; set; } = 4;

		#endregion
	}
}