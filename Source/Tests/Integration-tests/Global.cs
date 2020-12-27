using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.Debug;

namespace IntegrationTests
{
	// ReSharper disable All
	[TestClass]
	[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
	public static class Global
	{
		#region Fields

		public const string DefaultEnvironment = "Integration-test";
		public static readonly string ProjectDirectoryPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

		#endregion

		//[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
		//public static IServiceCollection CreateServices(IConfiguration configuration, IHostEnvironment hostEnvironment, bool includeLoggerFactory = true)
		//{
		//	var services = new ServiceCollection();

		//	services.AddSingleton(configuration);
		//	services.AddSingleton(hostEnvironment);

		//	if(includeLoggerFactory)
		//		services.AddSingleton<ILoggerFactory>(new LoggerFactory(new[] {new DebugLoggerProvider()}));

		//	return services;
		//}
	}
	// ReSharper restore All
}