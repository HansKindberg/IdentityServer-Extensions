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
		public static int Run<TStartup>(string applicationName, string[] arguments) where TStartup : class
		{
			Console.Title = applicationName;

			try
			{
				//Log.Information($"Starting host for \"{applicationName}\" at {DateTime.Now:o} ...");
				Console.WriteLine($"Starting host for \"{applicationName}\" at {DateTime.Now:o} ...");

				var hostBuilder = CreateHostBuilder<TStartup>(arguments);

				hostBuilder.ConfigureAppConfiguration(configurationBuilder =>
				{
					Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configurationBuilder.Build()).CreateLogger();
				});

				hostBuilder
					.UseSerilog()
					.Build()
					.Run();

				return 0;
			}
			catch(Exception exception)
			{
				Console.WriteLine(exception);
				Log.Fatal(exception, "Host terminated unexpectedly.");

				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		#endregion
	}
}