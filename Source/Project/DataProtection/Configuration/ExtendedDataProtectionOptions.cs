using System;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.DataProtection.Configuration.KeyProtection;
using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegionOrebroLan.Configuration;

namespace HansKindberg.IdentityServer.DataProtection.Configuration
{
	public abstract class ExtendedDataProtectionOptions : DataProtectionOptions, IApplicationOptions, IServiceConfigurationOptions
	{
		#region Properties

		public virtual DynamicOptions KeyProtection { get; set; }

		#endregion

		#region Methods

		public virtual void Add(IServiceConfiguration serviceConfiguration, IServiceCollection services, params IConfigurationSection[] configurationSections)
		{
			try
			{
				if(services == null)
					throw new ArgumentNullException(nameof(services));

				var dataProtectionBuilder = services.AddDataProtection(dataProtectionOptions =>
				{
					foreach(var configurationSection in configurationSections)
					{
						configurationSection.Bind(dataProtectionOptions);
					}
				});

				foreach(var configurationSection in configurationSections)
				{
					configurationSection.Bind(this);
				}

				services.AddSingleton(this);

				this.AddInternal(dataProtectionBuilder, serviceConfiguration);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not add data-protection with options of type \"{this.GetType()}\".", exception);
			}
		}

		protected internal virtual void AddInternal(IDataProtectionBuilder dataProtectionBuilder, IServiceConfiguration serviceConfiguration)
		{
			if(dataProtectionBuilder == null)
				throw new ArgumentNullException(nameof(dataProtectionBuilder));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			if(this.KeyProtection?.Type == null)
				return;

			KeyProtectionOptions keyProtectionOptions = null;

			try
			{
				keyProtectionOptions = (KeyProtectionOptions)serviceConfiguration.InstanceFactory.Create(this.KeyProtection.Type);
				this.KeyProtection.Options.Bind(keyProtectionOptions);
				keyProtectionOptions.Add(dataProtectionBuilder, serviceConfiguration);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not add key-protection with options of type \"{(keyProtectionOptions != null ? keyProtectionOptions.GetType().ToString() : this.KeyProtection.Type)}\".", exception);
			}
		}

		public virtual void Use(IApplicationBuilder applicationBuilder)
		{
			try
			{
				this.UseInternal(applicationBuilder);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not use data-protection with options of type \"{this.GetType()}\".", exception);
			}
		}

		protected internal virtual void UseInternal(IApplicationBuilder applicationBuilder) { }

		#endregion
	}
}