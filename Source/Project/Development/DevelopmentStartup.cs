using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HansKindberg.IdentityServer.DependencyInjection;
using HansKindberg.IdentityServer.Development.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HansKindberg.IdentityServer.Development
{
	public class DevelopmentStartup : IDevelopmentStartup
	{
		#region Fields

		private Lazy<Assembly> _assembly;
		private Lazy<Action<IApplicationBuilder>> _configureAction;
		private Lazy<Action<IServiceCollection>> _configureServicesAction;
		private Lazy<Type> _startupType;

		#endregion

		#region Constructors

		public DevelopmentStartup(IServiceConfigurationBuilder serviceConfigurationBuilder)
		{
			this.ServiceConfigurationBuilder = serviceConfigurationBuilder ?? throw new ArgumentNullException(nameof(serviceConfigurationBuilder));
		}

		#endregion

		#region Properties

		protected internal virtual Assembly Assembly
		{
			get
			{
				this._assembly ??= new Lazy<Assembly>(() =>
				{
					var path = this.Options.Value.AssemblyPath?.Trim(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

					if(string.IsNullOrWhiteSpace(path))
						return null;

					// ReSharper disable AssignNullToNotNullAttribute
					if(!Path.IsPathRooted(path))
						path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
					// ReSharper restore AssignNullToNotNullAttribute

					return File.Exists(path) ? Assembly.LoadFile(path) : null;
				});

				return this._assembly.Value;
			}
		}

		protected internal virtual Action<IApplicationBuilder> ConfigureAction
		{
			get
			{
				this._configureAction ??= new Lazy<Action<IApplicationBuilder>>(() =>
				{
					var configureMethod = this.GetStartupMethod("Configure");

					if(configureMethod == null)
						return null;

					return (Action<IApplicationBuilder>)Delegate.CreateDelegate(typeof(Action<IApplicationBuilder>), configureMethod);
				});

				return this._configureAction.Value;
			}
		}

		protected internal virtual Action<IServiceCollection> ConfigureServicesAction
		{
			get
			{
				this._configureServicesAction ??= new Lazy<Action<IServiceCollection>>(() =>
				{
					var configureServicesMethod = this.GetStartupMethod("ConfigureServices");

					if(configureServicesMethod == null)
						return null;

					return (Action<IServiceCollection>)Delegate.CreateDelegate(typeof(Action<IServiceCollection>), configureServicesMethod);
				});

				return this._configureServicesAction.Value;
			}
		}

		protected internal virtual IOptions<DevelopmentOptions> Options => this.ServiceConfigurationBuilder.Development;
		protected internal virtual IServiceConfigurationBuilder ServiceConfigurationBuilder { get; }

		protected internal virtual Type StartupType
		{
			get
			{
				this._startupType ??= new Lazy<Type>(() => { return this.Assembly?.GetTypes().FirstOrDefault(type => type.Name.Equals("Startup", StringComparison.OrdinalIgnoreCase)); });

				return this._startupType.Value;
			}
		}

		#endregion

		#region Methods

		public virtual void Configure(IApplicationBuilder applicationBuilder)
		{
			this.ConfigureAction?.Invoke(applicationBuilder);
		}

		public virtual void ConfigureServices(IServiceCollection services)
		{
			this.ConfigureServicesAction?.Invoke(services);
		}

		protected internal virtual MethodInfo GetStartupMethod(string name)
		{
			return this.StartupType?.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
		}

		#endregion
	}
}