using System;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HansKindberg.IdentityServer.Logging.Configuration
{
	public abstract class LoggingProviderOptions : ServiceConfigurationOptions, IApplicationOptions
	{
		#region Methods

		protected internal override void Add(IServiceConfiguration serviceConfiguration, IServiceCollection services)
		{
			try
			{
				this.AddInternal(serviceConfiguration, services);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not add logging-provider with options of type \"{this.GetType()}\".", exception);
			}
		}

		protected internal abstract void AddInternal(IServiceConfiguration serviceConfiguration, IServiceCollection services);

		protected internal override void AddService(IServiceCollection services)
		{
			services.AddSingleton(this);
		}

		public virtual void Use(IApplicationBuilder applicationBuilder)
		{
			try
			{
				this.UseInternal(applicationBuilder);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not use logging-provider with options of type \"{this.GetType()}\".", exception);
			}
		}

		protected internal abstract void UseInternal(IApplicationBuilder applicationBuilder);

		#endregion
	}
}