using System;
using Application;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests
{
	public class WebApplicationFactory : WebApplicationFactory<Startup>
	{
		#region Constructors

		public WebApplicationFactory()
		{
			this.ClientOptions.BaseAddress = new Uri("https://localhost:44300");
		}

		#endregion
	}
}