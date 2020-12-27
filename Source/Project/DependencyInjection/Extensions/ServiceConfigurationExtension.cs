using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using RegionOrebroLan.Abstractions.Extensions;
using RegionOrebroLan.Configuration;
using RegionOrebroLan.Security.Cryptography.Configuration;
using RegionOrebroLan.Security.Cryptography.Extensions;

namespace HansKindberg.IdentityServer.DependencyInjection.Extensions
{
	public static class ServiceConfigurationExtension
	{
		#region Methods

		public static X509Certificate2 GetCertificate(this IServiceConfiguration serviceConfiguration, DynamicOptions certificateOptions)
		{
			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			if(certificateOptions == null)
				throw new ArgumentNullException(nameof(certificateOptions));

			try
			{
				var resolverOptions = (ResolverOptions)serviceConfiguration.InstanceFactory.Create(certificateOptions.Type);
				certificateOptions.Options?.Bind(resolverOptions);

				var certificate = serviceConfiguration.CertificateResolver.Resolve(resolverOptions);

				return certificate.Unwrap<X509Certificate2>();
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not resolve certificate from options with path \"{certificateOptions.Options?.Path}\" and type \"{certificateOptions.Type}\".", exception);
			}
		}

		#endregion
	}
}