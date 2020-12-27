using System;
using Microsoft.FeatureManagement;

namespace HansKindberg.IdentityServer.FeatureManagement.Extensions
{
	public static class FeatureManagerExtension
	{
		#region Methods

		public static bool IsEnabled(this IFeatureManager featureManager, Feature feature)
		{
			if(featureManager == null)
				throw new ArgumentNullException(nameof(featureManager));

			return featureManager.IsEnabledAsync(feature.ToString()).Result;
		}

		#endregion
	}
}