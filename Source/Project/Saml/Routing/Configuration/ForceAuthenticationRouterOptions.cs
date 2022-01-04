using HansKindberg.IdentityServer.FeatureManagement;

namespace HansKindberg.IdentityServer.Saml.Routing.Configuration
{
	public class ForceAuthenticationRouterOptions
	{
		#region Properties

		public virtual string Path { get; set; } = $"/{Feature.ClaimsSelection}";

		#endregion
	}
}