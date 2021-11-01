using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HansKindberg.IdentityServer.Data;
using HansKindberg.IdentityServer.DependencyInjection.Extensions;
using HansKindberg.IdentityServer.FeatureManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace IntegrationTests.Helpers
{
	[SuppressMessage("Naming", "CA1724:Type names should not match namespaces")]
	public class Context : IDisposable
	{
		#region Fields

		private readonly IEnumerable<KeyValuePair<string, string>> _additionalConfiguration;
		private readonly string _additionalJsonConfigurationRelativeFilePath;
		private IApplicationBuilder _applicationBuilder;
		private readonly Action<IServiceCollection, IConfiguration, IHostEnvironment> _configureServicesAction;
		private readonly string _contentRootPath;
		private readonly DatabaseProvider? _databaseProvider;
		private bool _disposed;
		private readonly string _environmentName;
		private readonly IDictionary<Feature, bool> _features;
		private readonly PhysicalFileProvider _fileProvider;
		private readonly ILoggerFactory _loggerFactory;
		private IServiceProvider _serviceProvider;

		#endregion

		#region Constructors

		public Context(IEnumerable<KeyValuePair<string, string>> additionalConfiguration = null, string additionalJsonConfigurationRelativeFilePath = null, Action<IServiceCollection, IConfiguration, IHostEnvironment> configureServicesAction = null, string contentRootPath = null, DatabaseProvider? databaseProvider = null, string environmentName = Global.DefaultEnvironment, IDictionary<Feature, bool> features = null)
		{
			this._additionalConfiguration = additionalConfiguration ?? Enumerable.Empty<KeyValuePair<string, string>>();
			this._additionalJsonConfigurationRelativeFilePath = additionalJsonConfigurationRelativeFilePath;
			this._configureServicesAction = configureServicesAction ?? ((services, configuration, hostEnvironment) =>
			{
				services.Configure(configuration, hostEnvironment);
			});
			this._contentRootPath = contentRootPath ?? Global.ProjectDirectoryPath;
			this._databaseProvider = databaseProvider;
			this._environmentName = environmentName;
			this._features = features ?? new Dictionary<Feature, bool>();
			this._fileProvider = new PhysicalFileProvider(this._contentRootPath);
			this._loggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
		}

		#endregion

		#region Properties

		public virtual IApplicationBuilder ApplicationBuilder
		{
			get
			{
				this.ThrowExceptionIfDisposed();

				return this._applicationBuilder ??= new ApplicationBuilderFactory(this.ServiceProvider).CreateBuilder(new FeatureCollection());
			}
		}

		public virtual IFileProvider FileProvider
		{
			get
			{
				this.ThrowExceptionIfDisposed();

				return this._fileProvider;
			}
		}

		public virtual IServiceProvider ServiceProvider
		{
			get
			{
				this.ThrowExceptionIfDisposed();

				// ReSharper disable InvertIf
				if(this._serviceProvider == null)
				{
					var configuration = this.CreateConfigurationBuilder().Build();
					var hostEnvironment = this.CreateHostEnvironment();
					var services = new ServiceCollection();

					services.AddSingleton<IConfiguration>(configuration);
					services.AddSingleton(hostEnvironment);
					services.AddSingleton(this._loggerFactory);

					this._configureServicesAction(services, configuration, hostEnvironment);

					this._serviceProvider = services.BuildServiceProvider();
				}
				// ReSharper restore InvertIf

				return this._serviceProvider;
			}
		}

		#endregion

		#region Methods

		private IConfigurationBuilder CreateConfigurationBuilder()
		{
			return new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", false, true)
				.AddJsonFile($"appsettings.{this._environmentName}.json", true, true)
				.AddJsonFile(this._additionalJsonConfigurationRelativeFilePath ?? "e3398c8f-496f-4c71-b2a0-f7ffed03644a.json", true, false)
				.AddInMemoryCollection(this.CreateDatabaseConfiguration())
				.AddInMemoryCollection(FeatureConfigurationHelper.CreateConfiguration(this._features))
				.AddInMemoryCollection(this._additionalConfiguration)
				.SetFileProvider(this._fileProvider);
		}

		private IList<KeyValuePair<string, string>> CreateDatabaseConfiguration()
		{
			if(this._databaseProvider == null)
				return new List<KeyValuePair<string, string>>();

			return this._databaseProvider == DatabaseProvider.SqlServer ? DatabaseConfigurationHelper.CreateExplicitSqlServerTestConfiguration() : DatabaseConfigurationHelper.CreateExplicitSqliteTestConfiguration();
		}

		private IHostEnvironment CreateHostEnvironment()
		{
			return new HostingEnvironment
			{
				ApplicationName = this.GetType().Assembly.GetName().Name,
				ContentRootFileProvider = this._fileProvider,
				ContentRootPath = this._contentRootPath,
				EnvironmentName = this._environmentName
			};
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(!disposing)
				return;

			this._disposed = true;
			this._fileProvider.Dispose();
			this._loggerFactory.Dispose();
		}

		private void ThrowExceptionIfDisposed()
		{
			if(this._disposed)
				throw new ObjectDisposedException("The context is disposed.");
		}

		#endregion
	}
}