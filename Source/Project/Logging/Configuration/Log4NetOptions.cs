using System;
using System.IO;
using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HansKindberg.IdentityServer.Logging.Configuration
{
	public class Log4NetOptions : LoggingProviderOptions
	{
		#region Properties

		public virtual string ConfigurationFileName { get; set; } = "Log4Net.config";
		public virtual bool Watch { get; set; } = true;

		#endregion

		#region Methods

		protected internal override void AddInternal(IServiceConfiguration serviceConfiguration, IServiceCollection services) { }

		protected internal override void UseInternal(IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			if(string.IsNullOrWhiteSpace(this.ConfigurationFileName))
				throw new InvalidOperationException("The configuration-file-name can not be null, empty or whitespaces only.");

			var configurationFilePath = this.ConfigurationFileName;

			if(!Path.IsPathRooted(configurationFilePath))
				configurationFilePath = Path.Combine(applicationBuilder.ApplicationServices.GetRequiredService<IHostEnvironment>().ContentRootPath, configurationFilePath);

			applicationBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>().AddLog4Net(configurationFilePath, this.Watch);
		}

		#endregion
	}
}