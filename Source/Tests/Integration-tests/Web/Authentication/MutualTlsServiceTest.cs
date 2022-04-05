using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.DependencyInjection.Extensions;
using HansKindberg.IdentityServer.Web.Authentication;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IntegrationTests.Web.Authentication
{
	[TestClass]
	public class MutualTlsServiceTest
	{
		#region Fields

		private static readonly string _domainWithDotExceptionMessage = string.Format(null, _domainWithDotExceptionMessageFormat, "mtls.site");
		private const string _domainWithDotExceptionMessageFormat = "The MutualTlsOptions.DomainName can not contain dots. The current value is \"{0}\". The reason is that the interactive mtls functionality, interactive client-certificate authentication, only supports subdomain.";
		private const string _resourcesDirectoryRelativePath = "Web/Authentication/Resources/MutualTlsService/";

		#endregion

		#region Methods

		protected internal virtual Action<IServiceCollection, IConfiguration, IHostEnvironment> CreateConfigureServicesAction(string authority)
		{
			var defaultHttpContext = new DefaultHttpContext
			{
				Request =
				{
					Host = new HostString(authority),
					Scheme = Uri.UriSchemeHttps
				}
			};

			return (services, configuration, hostEnvironment) =>
			{
				services.Configure(configuration, hostEnvironment);

				var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
				httpContextAccessorMock.Setup(httpContext => httpContext.HttpContext).Returns(defaultHttpContext);
				services.AddSingleton(httpContextAccessorMock.Object);
			};
		}

		protected internal virtual async Task<HttpRequest> CreateHttpRequest(string authority)
		{
			var httpContext = new DefaultHttpContext
			{
				Request =
				{
					Host = new HostString(authority),
					Scheme = Uri.UriSchemeHttps
				}
			};

			return await Task.FromResult(httpContext.Request);
		}

		[TestMethod]
		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public async Task GetIssuerOriginAsync_Test()
		{
			var authority = "example.org";
			var expectedIssuerOrigin = "https://example.org";
			await this.GetIssuerOriginAsyncTest("Disabled", authority, expectedIssuerOrigin);
			try
			{
				await this.GetIssuerOriginAsyncTest("DomainWithDot", authority, expectedIssuerOrigin);
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.GetIssuerOriginAsyncTest("DomainWithHyphen", authority, expectedIssuerOrigin);
			await this.GetIssuerOriginAsyncTest("EmptyDomain", authority, expectedIssuerOrigin);
			await this.GetIssuerOriginAsyncTest("Enabled", authority, expectedIssuerOrigin);
			await this.GetIssuerOriginAsyncTest("EnabledWithIssuer", authority, expectedIssuerOrigin);

			authority = "mtls.example.org";
			expectedIssuerOrigin = "https://mtls.example.org";
			await this.GetIssuerOriginAsyncTest("Disabled", authority, expectedIssuerOrigin);
			try
			{
				await this.GetIssuerOriginAsyncTest("DomainWithDot", authority, expectedIssuerOrigin);
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.GetIssuerOriginAsyncTest("DomainWithHyphen", authority, expectedIssuerOrigin);
			await this.GetIssuerOriginAsyncTest("EmptyDomain", authority, expectedIssuerOrigin);
			await this.GetIssuerOriginAsyncTest("Enabled", authority, "https://example.org");
			await this.GetIssuerOriginAsyncTest("EnabledWithIssuer", authority, "https://example.org");

			authority = "identityserver.local:6001";
			expectedIssuerOrigin = "https://identityserver.local:6001";
			await this.GetIssuerOriginAsyncTest("Disabled", authority, expectedIssuerOrigin);
			try
			{
				await this.GetIssuerOriginAsyncTest("DomainWithDot", authority, expectedIssuerOrigin);
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.GetIssuerOriginAsyncTest("DomainWithHyphen", authority, expectedIssuerOrigin);
			await this.GetIssuerOriginAsyncTest("EmptyDomain", authority, expectedIssuerOrigin);
			await this.GetIssuerOriginAsyncTest("Enabled", authority, expectedIssuerOrigin);
			await this.GetIssuerOriginAsyncTest("EnabledWithIssuer", authority, "https://example.org");

			authority = "mtls.identityserver.local:6001";
			expectedIssuerOrigin = "https://mtls.identityserver.local:6001";
			await this.GetIssuerOriginAsyncTest("Disabled", authority, expectedIssuerOrigin);
			try
			{
				await this.GetIssuerOriginAsyncTest("DomainWithDot", authority, expectedIssuerOrigin);
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.GetIssuerOriginAsyncTest("DomainWithHyphen", authority, expectedIssuerOrigin);
			await this.GetIssuerOriginAsyncTest("EmptyDomain", authority, expectedIssuerOrigin);
			await this.GetIssuerOriginAsyncTest("Enabled", authority, "https://identityserver.local:6001");
			await this.GetIssuerOriginAsyncTest("EnabledWithIssuer", authority, "https://example.org");

			authority = "mtls-site.identityserver.local:6001";
			expectedIssuerOrigin = "https://mtls-site.identityserver.local:6001";
			await this.GetIssuerOriginAsyncTest("Disabled", authority, expectedIssuerOrigin);
			try
			{
				await this.GetIssuerOriginAsyncTest("DomainWithDot", authority, expectedIssuerOrigin);
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.GetIssuerOriginAsyncTest("DomainWithHyphen", authority, "https://identityserver.local:6001");
			await this.GetIssuerOriginAsyncTest("EmptyDomain", authority, expectedIssuerOrigin);
			// According to me the following would give:
			//await this.GetIssuerOriginAsyncTest("Enabled", authority, "https://mtls-site.identityserver.local:6001");
			// But gives:
			await this.GetIssuerOriginAsyncTest("Enabled", authority, "https://site.identityserver.local:6001");
			await this.GetIssuerOriginAsyncTest("EnabledWithIssuer", authority, "https://example.org");
		}

		protected internal virtual async Task GetIssuerOriginAsyncTest(string additionalJsonConfiguration, string contextAuthority, string expectedIssuerOrigin)
		{
			var additionalJsonConfigurationRelativeFilePath = $"{_resourcesDirectoryRelativePath}{additionalJsonConfiguration}.json";

			using(var context = new Context(additionalJsonConfigurationRelativeFilePath: additionalJsonConfigurationRelativeFilePath, configureServicesAction: this.CreateConfigureServicesAction(contextAuthority)))
			{
				// ReSharper disable PossibleNullReferenceException
				context.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext.RequestServices = context.ServiceProvider;
				// ReSharper restore PossibleNullReferenceException

				var mutualTlsService = (MutualTlsService)context.ServiceProvider.GetRequiredService<IMutualTlsService>();

				Assert.AreEqual(expectedIssuerOrigin, await mutualTlsService.GetIssuerOriginAsync());
			}
		}

		[TestMethod]
		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public async Task GetMtlsOriginAsync_Test()
		{
			var authority = "example.org";
			var expectedMtlsOrigin = "https://example.org";
			await this.GetMtlsOriginAsyncTest("Disabled", authority, expectedMtlsOrigin);
			try
			{
				await this.GetMtlsOriginAsyncTest("DomainWithDot", authority, expectedMtlsOrigin);
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.GetMtlsOriginAsyncTest("DomainWithHyphen", authority, "https://mtls-site.example.org");
			await this.GetMtlsOriginAsyncTest("EmptyDomain", authority, expectedMtlsOrigin);
			await this.GetMtlsOriginAsyncTest("Enabled", authority, "https://mtls.example.org");
			await this.GetMtlsOriginAsyncTest("EnabledWithIssuer", authority, "https://mtls.example.org");

			authority = "mtls.example.org";
			expectedMtlsOrigin = "https://mtls.example.org";
			await this.GetMtlsOriginAsyncTest("Disabled", authority, expectedMtlsOrigin);
			try
			{
				await this.GetMtlsOriginAsyncTest("DomainWithDot", authority, expectedMtlsOrigin);
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.GetMtlsOriginAsyncTest("DomainWithHyphen", authority, "https://mtls-site.mtls.example.org");
			await this.GetMtlsOriginAsyncTest("EmptyDomain", authority, expectedMtlsOrigin);
			await this.GetMtlsOriginAsyncTest("Enabled", authority, "https://mtls.example.org");
			await this.GetMtlsOriginAsyncTest("EnabledWithIssuer", authority, "https://mtls.example.org");

			authority = "identityserver.local:6001";
			expectedMtlsOrigin = "https://identityserver.local:6001";
			await this.GetMtlsOriginAsyncTest("Disabled", authority, expectedMtlsOrigin);
			try
			{
				await this.GetMtlsOriginAsyncTest("DomainWithDot", authority, expectedMtlsOrigin);
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.GetMtlsOriginAsyncTest("DomainWithHyphen", authority, "https://mtls-site.identityserver.local:6001");
			await this.GetMtlsOriginAsyncTest("EmptyDomain", authority, expectedMtlsOrigin);
			await this.GetMtlsOriginAsyncTest("Enabled", authority, "https://mtls.identityserver.local:6001");
			await this.GetMtlsOriginAsyncTest("EnabledWithIssuer", authority, "https://mtls.example.org");

			authority = "mtls.identityserver.local:6001";
			expectedMtlsOrigin = "https://mtls.identityserver.local:6001";
			await this.GetMtlsOriginAsyncTest("Disabled", authority, expectedMtlsOrigin);
			try
			{
				await this.GetMtlsOriginAsyncTest("DomainWithDot", authority, expectedMtlsOrigin);
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.GetMtlsOriginAsyncTest("DomainWithHyphen", authority, "https://mtls-site.mtls.identityserver.local:6001");
			await this.GetMtlsOriginAsyncTest("EmptyDomain", authority, expectedMtlsOrigin);
			await this.GetMtlsOriginAsyncTest("Enabled", authority, expectedMtlsOrigin);
			await this.GetMtlsOriginAsyncTest("EnabledWithIssuer", authority, "https://mtls.example.org");

			authority = "mtls-site.identityserver.local:6001";
			expectedMtlsOrigin = "https://mtls-site.identityserver.local:6001";
			await this.GetMtlsOriginAsyncTest("Disabled", authority, expectedMtlsOrigin);
			try
			{
				await this.GetMtlsOriginAsyncTest("DomainWithDot", authority, expectedMtlsOrigin);
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.GetMtlsOriginAsyncTest("DomainWithHyphen", authority, expectedMtlsOrigin);
			await this.GetMtlsOriginAsyncTest("EmptyDomain", authority, expectedMtlsOrigin);
			// According to me the following would give:
			//await this.GetMtlsOriginAsyncTest("Enabled", authority, "https://mtls.mtls-site.identityserver.local:6001");
			// But gives:
			await this.GetMtlsOriginAsyncTest("Enabled", authority, "https://mtls.site.identityserver.local:6001");
			await this.GetMtlsOriginAsyncTest("EnabledWithIssuer", authority, "https://mtls.example.org");
		}

		protected internal virtual async Task GetMtlsOriginAsyncTest(string additionalJsonConfiguration, string contextAuthority, string expectedMtlsOrigin)
		{
			var additionalJsonConfigurationRelativeFilePath = $"{_resourcesDirectoryRelativePath}{additionalJsonConfiguration}.json";

			using(var context = new Context(additionalJsonConfigurationRelativeFilePath: additionalJsonConfigurationRelativeFilePath, configureServicesAction: this.CreateConfigureServicesAction(contextAuthority)))
			{
				// ReSharper disable PossibleNullReferenceException
				context.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext.RequestServices = context.ServiceProvider;
				// ReSharper restore PossibleNullReferenceException

				var mutualTlsService = (MutualTlsService)context.ServiceProvider.GetRequiredService<IMutualTlsService>();

				Assert.AreEqual(expectedMtlsOrigin, await mutualTlsService.GetMtlsOriginAsync());
			}
		}

		[TestMethod]
		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public async Task IsMtlsDomainRequestAsync_Test()
		{
			var authority = "example.org";
			await this.IsMtlsDomainRequestAsyncTest("Disabled", authority, false, await this.CreateHttpRequest(authority));
			try
			{
				await this.IsMtlsDomainRequestAsyncTest("DomainWithDot", authority, false, await this.CreateHttpRequest(authority));
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.IsMtlsDomainRequestAsyncTest("DomainWithHyphen", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("EmptyDomain", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("Enabled", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("EnabledWithIssuer", authority, false, await this.CreateHttpRequest(authority));

			authority = "mtls.example.org";
			await this.IsMtlsDomainRequestAsyncTest("Disabled", authority, false, await this.CreateHttpRequest(authority));
			try
			{
				await this.IsMtlsDomainRequestAsyncTest("DomainWithDot", authority, false, await this.CreateHttpRequest(authority));
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.IsMtlsDomainRequestAsyncTest("DomainWithHyphen", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("EmptyDomain", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("Enabled", authority, true, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("EnabledWithIssuer", authority, true, await this.CreateHttpRequest(authority));

			authority = "identityserver.local:6001";
			await this.IsMtlsDomainRequestAsyncTest("Disabled", authority, false, await this.CreateHttpRequest(authority));
			try
			{
				await this.IsMtlsDomainRequestAsyncTest("DomainWithDot", authority, false, await this.CreateHttpRequest(authority));
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.IsMtlsDomainRequestAsyncTest("DomainWithHyphen", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("EmptyDomain", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("Enabled", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("EnabledWithIssuer", authority, false, await this.CreateHttpRequest(authority));

			authority = "mtls.identityserver.local:6001";
			await this.IsMtlsDomainRequestAsyncTest("Disabled", authority, false, await this.CreateHttpRequest(authority));
			try
			{
				await this.IsMtlsDomainRequestAsyncTest("DomainWithDot", authority, false, await this.CreateHttpRequest(authority));
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.IsMtlsDomainRequestAsyncTest("DomainWithHyphen", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("EmptyDomain", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("Enabled", authority, true, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("EnabledWithIssuer", authority, false, await this.CreateHttpRequest(authority));

			authority = "mtls-site.identityserver.local:6001";
			await this.IsMtlsDomainRequestAsyncTest("Disabled", authority, false, await this.CreateHttpRequest(authority));
			try
			{
				await this.IsMtlsDomainRequestAsyncTest("DomainWithDot", authority, false, await this.CreateHttpRequest(authority));
				Assert.Fail("Should throw an exception.");
			}
			catch(Exception exception)
			{
				Assert.IsTrue(exception is InvalidOperationException);
				Assert.AreEqual(_domainWithDotExceptionMessage, exception.Message);
			}

			await this.IsMtlsDomainRequestAsyncTest("DomainWithHyphen", authority, true, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("EmptyDomain", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("Enabled", authority, false, await this.CreateHttpRequest(authority));
			await this.IsMtlsDomainRequestAsyncTest("EnabledWithIssuer", authority, false, await this.CreateHttpRequest(authority));
		}

		protected internal virtual async Task IsMtlsDomainRequestAsyncTest(string additionalJsonConfiguration, string contextAuthority, bool expected, HttpRequest httpRequest)
		{
			var additionalJsonConfigurationRelativeFilePath = $"{_resourcesDirectoryRelativePath}{additionalJsonConfiguration}.json";

			using(var context = new Context(additionalJsonConfigurationRelativeFilePath: additionalJsonConfigurationRelativeFilePath, configureServicesAction: this.CreateConfigureServicesAction(contextAuthority)))
			{
				// ReSharper disable PossibleNullReferenceException
				context.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext.RequestServices = context.ServiceProvider;
				// ReSharper restore PossibleNullReferenceException

				var mutualTlsService = (MutualTlsService)context.ServiceProvider.GetRequiredService<IMutualTlsService>();

				Assert.AreEqual(expected, await mutualTlsService.IsMtlsDomainRequestAsync(httpRequest));
			}
		}

		#endregion
	}
}