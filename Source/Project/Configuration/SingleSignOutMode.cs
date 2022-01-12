using System;

namespace HansKindberg.IdentityServer.Configuration
{
	/// <summary>
	/// Mode for single-sign-out / single-logout / SLO.
	/// </summary>
	[Flags]
	public enum SingleSignOutMode
	{
		None = 0,
		ClientInitiated = 1,
		IdpInitiated = 2
	}
}