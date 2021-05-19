using System;
using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HansKindberg.IdentityServer.Configuration
{
	public abstract class ServiceConfigurationOptions : IServiceConfigurationOptions
	{
		#region Methods

		public virtual void Add(IServiceConfigurationBuilder serviceConfigurationBuilder, IServiceCollection services, params IConfigurationSection[] configurationSections)
		{
			try
			{
				if(services == null)
					throw new ArgumentNullException(nameof(services));

				foreach(var configurationSection in configurationSections)
				{
					configurationSection.Bind(this);
				}

				this.AddService(services);

				this.Add(serviceConfigurationBuilder, services);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not add options of type \"{this.GetType()}\".", exception);
			}
		}

		protected internal abstract void Add(IServiceConfigurationBuilder serviceConfigurationBuilder, IServiceCollection services);
		protected internal abstract void AddService(IServiceCollection services);

		#endregion
	}
}