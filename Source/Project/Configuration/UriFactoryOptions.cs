namespace HansKindberg.IdentityServer.Configuration
{
	public class UriFactoryOptions
	{
		#region Properties

		public virtual bool TrailingPathSlash { get; set; } = true;
		public virtual bool UiLocalesInReturnUrl { get; set; } = true;

		#endregion
	}
}