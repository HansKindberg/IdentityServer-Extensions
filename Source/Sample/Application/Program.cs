using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RegionOrebroLan.Configuration.EnvironmentVariables;

namespace Application
{
	public static class Program
	{
		#region Methods

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(configurationBuilder =>
				{
					for(var i = 0; i < configurationBuilder.Sources.Count; i++)
					{
						if(!(configurationBuilder.Sources[i] is Microsoft.Extensions.Configuration.EnvironmentVariables.EnvironmentVariablesConfigurationSource))
							continue;

						configurationBuilder.Sources[i] = new EnvironmentVariablesConfigurationSource();
					}
				})
				.ConfigureWebHostDefaults(webHostBuilder => webHostBuilder.UseStartup<Startup>());
		}

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		#endregion
	}
}