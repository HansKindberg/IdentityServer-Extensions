using System;
using HansKindberg.IdentityServer.Application;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Host = HansKindberg.IdentityServer.Hosting.Host;

namespace IntegrationTests
{
	public class WebApplicationFactory : WebApplicationFactory<Startup>
	{
		#region Constructors

		public WebApplicationFactory()
		{
			this.ClientOptions.BaseAddress = new Uri("https://localhost:6001");
		}

		#endregion

		#region Methods

		protected override IHostBuilder CreateHostBuilder()
		{
			return Host.CreateHostBuilder<Startup>(null).UseEnvironment("Integration-tests");
		}

		#endregion
	}
}