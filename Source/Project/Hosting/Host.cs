using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RegionOrebroLan.Configuration.EnvironmentVariables;
using Serilog;

namespace HansKindberg.IdentityServer.Hosting
{
	public static class Host
	{
		#region Methods

		public static IHostBuilder CreateHostBuilder<TStartup>(string[] arguments) where TStartup : class
		{
			return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(arguments)
				.UseSerilog((hostBuilderContext, serviceProvider, loggerConfiguration) =>
				{
					loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration);
					loggerConfiguration.ReadFrom.Services(serviceProvider);
				})
				.ConfigureAppConfiguration(configurationBuilder =>
				{
					for(var i = 0; i < configurationBuilder.Sources.Count; i++)
					{
						if(!(configurationBuilder.Sources[i] is Microsoft.Extensions.Configuration.EnvironmentVariables.EnvironmentVariablesConfigurationSource))
							continue;

						configurationBuilder.Sources[i] = new EnvironmentVariablesConfigurationSource();
					}
				})
				.ConfigureWebHostDefaults(webHostBuilder => webHostBuilder.UseStartup<TStartup>());
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		[SuppressMessage("Globalization", "CA1305:Specify IFormatProvider")]
		public static int Run<TStartup>(string applicationName, string[] arguments) where TStartup : class
		{
			Console.Title = applicationName;

			Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
			Log.Information($"Starting host for \"{applicationName}\" at {DateTime.Now:o} ...");

			try
			{
				CreateHostBuilder<TStartup>(arguments).Build().Run();

				return 0;
			}
			catch(Exception exception)
			{
				Log.Fatal(exception, $"Host for \"{applicationName}\" terminated unexpectedly.");

				return 1;
			}
			finally
			{
				Log.Information($"Stopping host for \"{applicationName}\" at {DateTime.Now:o} ...");
				Log.CloseAndFlush();
			}
		}

		#endregion
	}
}