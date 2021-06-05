using System;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests.Helpers.Configuration.Extensions
{
	public static class ConfigurationBuilderExtension
	{
		#region Methods

		public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder configurationBuilder)
		{
			if(configurationBuilder == null)
				throw new ArgumentNullException(nameof(configurationBuilder));

			return configurationBuilder.AddJsonFileInternal("appsettings.json");
		}

		public static IConfigurationBuilder AddJsonFileForEnvironment(this IConfigurationBuilder configurationBuilder, string environmentName)
		{
			if(configurationBuilder == null)
				throw new ArgumentNullException(nameof(configurationBuilder));

			return configurationBuilder.AddJsonFileInternal($"appsettings.{environmentName}.json");
		}

		private static IConfigurationBuilder AddJsonFileInternal(this IConfigurationBuilder configurationBuilder, string path)
		{
			if(configurationBuilder == null)
				throw new ArgumentNullException(nameof(configurationBuilder));

			return configurationBuilder.AddJsonFile(path, true, true);
		}

		#endregion
	}
}