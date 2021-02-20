using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace HansKindberg.RoleService
{
	public static class Program
	{
		#region Methods

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public static int Main(string[] args)
		{
			const string applicationName = "Hans Kindberg - Role service";
			Console.Title = applicationName;

			try
			{
				Console.WriteLine($"Starting host for \"{applicationName}\" at {DateTime.Now:o} ...");

				var hostBuilder = CreateHostBuilder(args);

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