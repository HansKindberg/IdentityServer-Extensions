using System;
using System.IO;
using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;

namespace HansKindberg.IdentityServer.DataProtection.Configuration
{
	public class FileSystemOptions : ExtendedDataProtectionOptions
	{
		#region Properties

		public virtual string Path { get; set; }

		#endregion

		#region Methods

		protected internal override void AddInternal(IDataProtectionBuilder dataProtectionBuilder, IServiceConfiguration serviceConfiguration)
		{
			if(dataProtectionBuilder == null)
				throw new ArgumentNullException(nameof(dataProtectionBuilder));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			if(string.IsNullOrWhiteSpace(this.Path))
				throw new InvalidOperationException("The path can not be null, empty or whitespaces only.");

			var path = this.Path;

			if(!System.IO.Path.IsPathRooted(path))
				path = System.IO.Path.Combine(serviceConfiguration.HostEnvironment.ContentRootPath, path);

			dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(path));

			base.AddInternal(dataProtectionBuilder, serviceConfiguration);
		}

		#endregion
	}
}