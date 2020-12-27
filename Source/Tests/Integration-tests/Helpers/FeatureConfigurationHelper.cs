using System.Collections.Generic;
using HansKindberg.IdentityServer.FeatureManagement;

namespace IntegrationTests.Helpers
{
	public static class FeatureConfigurationHelper
	{
		#region Methods

		public static IList<KeyValuePair<string, string>> CreateConfiguration(IDictionary<Feature, bool> features = null)
		{
			var configuration = new List<KeyValuePair<string, string>>();

			foreach(var (feature, enabled) in features ?? new Dictionary<Feature, bool>())
			{
				configuration.Add(new KeyValuePair<string, string>($"FeatureManagement:{feature}", enabled.ToString()));
			}

			return configuration;
		}

		public static IList<KeyValuePair<string, string>> CreateConfiguration(Feature feature, bool enabled)
		{
			return CreateConfiguration(new Dictionary<Feature, bool> {{feature, enabled}});
		}

		#endregion
	}
}