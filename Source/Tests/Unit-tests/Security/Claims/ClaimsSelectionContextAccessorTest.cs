using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.Security.Claims;
using HansKindberg.IdentityServer.Security.Claims.Configuration;
using HansKindberg.IdentityServer.Web.Authentication;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RegionOrebroLan.Security.Claims;
using RegionOrebroLan.Web.Authentication;
using TestHelpers.Mocks.Logging;
using TestHelpers.Security.Claims;
using UnitTests.Mocks.Security.Claims;
using UnitTests.Mocks.Web.Authentication;
using HttpContextFactory = UnitTests.Helpers.HttpContextFactory;

namespace UnitTests.Security.Claims
{
	[TestClass]
	public class ClaimsSelectionContextAccessorTest
	{
		#region Methods

		[TestMethod]
		public async Task ClaimsSelectionContext_IfTheHttpContextIsNull_ShouldReturnNull()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var httpContextAccessor = await this.CreateHttpContextAccessorAsync(null);

				var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync(httpContextAccessor: httpContextAccessor, loggerFactory: loggerFactory);

				Assert.IsNull(httpContextAccessor.HttpContext);
				Assert.IsNull(claimsSelectionContextAccessor.ClaimsSelectionContext);
			}
		}

		[TestMethod]
		public async Task ClaimsSelectionContext_Test01()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync(loggerFactory: loggerFactory);
				Assert.IsNull(claimsSelectionContextAccessor.ClaimsSelectionContext);
			}
		}

		[TestMethod]
		public async Task ClaimsSelectionContext_Test02()
		{
			var httpContext = await HttpContextFactory.CreateAsync();
			var httpContextAccessor = await this.CreateHttpContextAccessorAsync(httpContext);

			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync(httpContextAccessor: httpContextAccessor, loggerFactory: loggerFactory);
				Assert.IsNull(claimsSelectionContextAccessor.ClaimsSelectionContext);
			}
		}

		[TestMethod]
		public async Task ClaimsSelectionContext_Test03()
		{
			const string authenticationSchemeName = "Test";

			var authenticationScheme = new AuthenticationSchemeMock
			{
				Name = authenticationSchemeName
			};
			var authenticationSchemeRetriever = await this.CreateAuthenticationSchemeRetrieverAsync(authenticationScheme, authenticationSchemeName);

			var claims = new ClaimBuilderCollection
			{
				new ClaimBuilder(new Claim(JwtClaimTypes.IdentityProvider, authenticationSchemeName))
			};
			var httpContext = await HttpContextFactory.CreateAsync();
			httpContext.User = await ClaimsPrincipalFactory.CreateAsync(claims);
			var httpContextAccessor = await this.CreateHttpContextAccessorAsync(httpContext);

			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync(authenticationSchemeRetriever: authenticationSchemeRetriever, httpContextAccessor: httpContextAccessor, loggerFactory: loggerFactory);
				Assert.IsNull(claimsSelectionContextAccessor.ClaimsSelectionContext);
			}
		}

		[TestMethod]
		public async Task ClaimsSelectionContext_Test04()
		{
			const string authenticationSchemeName = "Test";

			var authenticationScheme = new AuthenticationSchemeMock
			{
				Enabled = true,
				Name = authenticationSchemeName
			};
			var authenticationSchemeRetriever = await this.CreateAuthenticationSchemeRetrieverAsync(authenticationScheme, authenticationSchemeName);

			var claims = new ClaimBuilderCollection
			{
				new ClaimBuilder(new Claim(JwtClaimTypes.IdentityProvider, authenticationSchemeName))
			};
			var httpContext = await HttpContextFactory.CreateAsync();
			httpContext.User = await ClaimsPrincipalFactory.CreateAsync(claims);
			var httpContextAccessor = await this.CreateHttpContextAccessorAsync(httpContext);

			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync(authenticationSchemeRetriever: authenticationSchemeRetriever, httpContextAccessor: httpContextAccessor, loggerFactory: loggerFactory);
				Assert.IsNull(claimsSelectionContextAccessor.ClaimsSelectionContext);
			}
		}

		[TestMethod]
		public async Task ClaimsSelectionContext_Test05()
		{
			const string authenticationSchemeName = "Test";

			var authenticationScheme = new AuthenticationSchemeMock
			{
				Enabled = true,
				Name = authenticationSchemeName
			};
			var authenticationSchemeRetriever = await this.CreateAuthenticationSchemeRetrieverAsync(authenticationScheme, authenticationSchemeName);

			var claimsSelectors = new List<IClaimsSelector>();
			var claimsSelectorLoader = await this.CreateClaimsSelectorLoaderAsync(authenticationSchemeName, claimsSelectors);

			var claims = new ClaimBuilderCollection
			{
				new ClaimBuilder(new Claim(JwtClaimTypes.IdentityProvider, authenticationSchemeName))
			};
			var httpContext = await HttpContextFactory.CreateAsync();
			httpContext.User = await ClaimsPrincipalFactory.CreateAsync(claims);
			var httpContextAccessor = await this.CreateHttpContextAccessorAsync(httpContext);

			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync(authenticationSchemeRetriever: authenticationSchemeRetriever, claimsSelectorLoader: claimsSelectorLoader, httpContextAccessor: httpContextAccessor, loggerFactory: loggerFactory);
				Assert.IsNull(claimsSelectionContextAccessor.ClaimsSelectionContext);
				Assert.AreEqual(1, loggerFactory.Logs.Count());
				Assert.AreEqual($"The claims-selection-context is null because there are no claims-selectors for authentication-scheme {authenticationSchemeName.ToStringRepresentation()}.", loggerFactory.Logs.ElementAt(0).Message);
			}
		}

		[TestMethod]
		public async Task ClaimsSelectionContext_Test06()
		{
			const string authenticationSchemeName = "Test";

			var authenticationScheme = new AuthenticationSchemeMock
			{
				Enabled = true,
				Name = authenticationSchemeName
			};
			var authenticationSchemeRetriever = await this.CreateAuthenticationSchemeRetrieverAsync(authenticationScheme, authenticationSchemeName);

			var claimsSelectors = new List<IClaimsSelector>
			{
				new ClaimsSelectorMock()
			};
			var claimsSelectorLoader = await this.CreateClaimsSelectorLoaderAsync(authenticationSchemeName, claimsSelectors);

			var claims = new ClaimBuilderCollection
			{
				new ClaimBuilder(new Claim(JwtClaimTypes.IdentityProvider, authenticationSchemeName))
			};
			var httpContext = await HttpContextFactory.CreateAsync();
			httpContext.User = await ClaimsPrincipalFactory.CreateAsync(claims);
			var httpContextAccessor = await this.CreateHttpContextAccessorAsync(httpContext);

			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync(authenticationSchemeRetriever: authenticationSchemeRetriever, claimsSelectorLoader: claimsSelectorLoader, httpContextAccessor: httpContextAccessor, loggerFactory: loggerFactory);
				var claimsSelectionContext = claimsSelectionContextAccessor.ClaimsSelectionContext;
				Assert.IsNotNull(claimsSelectionContext);
				Assert.AreEqual(authenticationSchemeName, claimsSelectionContext.AuthenticationScheme);
				Assert.AreEqual(1, claimsSelectionContext.Selectors.Count());
				Assert.AreEqual("/ClaimsSelection?ReturnUrl=%2FAccount", claimsSelectionContext.Url.OriginalString);
				Assert.AreEqual(0, loggerFactory.Logs.Count());
			}
		}

		protected internal virtual async Task<IAuthenticationSchemeRetriever> CreateAuthenticationSchemeRetrieverAsync()
		{
			return (await this.CreateAuthenticationSchemeRetrieverMockAsync()).Object;
		}

		protected internal virtual async Task<IAuthenticationSchemeRetriever> CreateAuthenticationSchemeRetrieverAsync(IAuthenticationScheme authenticationScheme, string authenticationSchemeName)
		{
			var authenticationSchemeRetrieverMock = await this.CreateAuthenticationSchemeRetrieverMockAsync();

			authenticationSchemeRetrieverMock.Setup(authenticationSchemeRetriever => authenticationSchemeRetriever.GetAsync(authenticationSchemeName)).Returns(Task.FromResult(authenticationScheme));

			return await Task.FromResult(authenticationSchemeRetrieverMock.Object);
		}

		protected internal virtual async Task<Mock<IAuthenticationSchemeRetriever>> CreateAuthenticationSchemeRetrieverMockAsync()
		{
			var authenticationSchemeRetrieverMock = new Mock<IAuthenticationSchemeRetriever>();

			authenticationSchemeRetrieverMock.Setup(authenticationSchemeRetriever => authenticationSchemeRetriever.GetAsync(It.IsAny<string>())).Returns(Task.FromResult((IAuthenticationScheme)null));

			return await Task.FromResult(authenticationSchemeRetrieverMock);
		}

		protected internal virtual async Task<ClaimsSelectionContextAccessor> CreateClaimsSelectionContextAccessorAsync(IAuthenticationSchemeRetriever authenticationSchemeRetriever = null, IOptionsMonitor<ClaimsSelectionOptions> claimsSelectionOptionsMonitor = null, IClaimsSelectorLoader claimsSelectorLoader = null, IFeatureManager featureManager = null, IHttpContextAccessor httpContextAccessor = null, ILoggerFactory loggerFactory = null)
		{
			var claimsSelectionContextAccessor = new ClaimsSelectionContextAccessor(authenticationSchemeRetriever ?? await this.CreateAuthenticationSchemeRetrieverAsync(), claimsSelectionOptionsMonitor ?? await this.CreateClaimsSelectionOptionsMonitorAsync(new ClaimsSelectionOptions()), claimsSelectorLoader ?? await this.CreateClaimsSelectorLoaderAsync(), featureManager ?? await this.CreateFeatureManagerAsync(true), httpContextAccessor ?? await this.CreateHttpContextAccessorAsync(null), loggerFactory ?? Mock.Of<ILoggerFactory>());

			return await Task.FromResult(claimsSelectionContextAccessor);
		}

		protected internal virtual async Task<IOptionsMonitor<ClaimsSelectionOptions>> CreateClaimsSelectionOptionsMonitorAsync(ClaimsSelectionOptions claimsSelectionOptions)
		{
			var claimsSelectionOptionsMonitorMock = new Mock<IOptionsMonitor<ClaimsSelectionOptions>>();

			claimsSelectionOptionsMonitorMock.Setup(claimsSelectionOptionsMonitor => claimsSelectionOptionsMonitor.CurrentValue).Returns(claimsSelectionOptions);

			return await Task.FromResult(claimsSelectionOptionsMonitorMock.Object);
		}

		protected internal virtual async Task<IClaimsSelectorLoader> CreateClaimsSelectorLoaderAsync()
		{
			return (await this.CreateClaimsSelectorLoaderMockAsync()).Object;
		}

		protected internal virtual async Task<IClaimsSelectorLoader> CreateClaimsSelectorLoaderAsync(string authenticationScheme, IList<IClaimsSelector> claimsSelectors)
		{
			var claimsSelectorLoaderMock = await this.CreateClaimsSelectorLoaderMockAsync();

			claimsSelectorLoaderMock.Setup(claimsSelectorLoader => claimsSelectorLoader.GetClaimsSelectorsAsync(authenticationScheme)).Returns(Task.FromResult((IEnumerable<IClaimsSelector>)claimsSelectors));

			return claimsSelectorLoaderMock.Object;
		}

		protected internal virtual async Task<Mock<IClaimsSelectorLoader>> CreateClaimsSelectorLoaderMockAsync()
		{
			var claimsSelectorLoaderMock = new Mock<IClaimsSelectorLoader>();

			claimsSelectorLoaderMock.Setup(claimsSelectorLoader => claimsSelectorLoader.GetClaimsSelectorsAsync(It.IsAny<string>())).Returns(Task.FromResult(Enumerable.Empty<IClaimsSelector>()));

			return await Task.FromResult(claimsSelectorLoaderMock);
		}

		protected internal virtual async Task<IFeatureManager> CreateFeatureManagerAsync(bool enabled)
		{
			var featureManagerMock = new Mock<IFeatureManager>();

			featureManagerMock.Setup(featureManager => featureManager.IsEnabledAsync(Feature.ClaimsSelection.ToString())).Returns(Task.FromResult(enabled));

			return await Task.FromResult(featureManagerMock.Object);
		}

		protected internal virtual async Task<IHttpContextAccessor> CreateHttpContextAccessorAsync(HttpContext httpContext)
		{
			var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

			httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext).Returns(httpContext);

			return await Task.FromResult(httpContextAccessorMock.Object);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task GetAsync_WithAuthenticationSchemeParameterAndReturnUrlParameter_IfTheAuthenticationSchemeParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync();

			try
			{
				await claimsSelectionContextAccessor.GetAsync(null, string.Empty);
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(argumentNullException.ParamName == "authenticationScheme")
					throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task GetAsync_WithAuthenticationSchemeParameterAndReturnUrlParameter_IfTheReturnUrlParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync();

			try
			{
				await claimsSelectionContextAccessor.GetAsync("Test", null);
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(argumentNullException.ParamName == "returnUrl")
					throw;
			}
		}

		[TestMethod]
		public async Task GetAsync_WithAuthenticationSchemeParameterAndReturnUrlParameter_Test01()
		{
			const string authenticationSchemeName = "Test";

			var authenticationScheme = new AuthenticationSchemeMock
			{
				Enabled = true,
				Name = authenticationSchemeName
			};
			var authenticationSchemeRetriever = await this.CreateAuthenticationSchemeRetrieverAsync(authenticationScheme, authenticationSchemeName);

			var claimsSelectors = new List<IClaimsSelector>
			{
				new ClaimsSelectorMock()
			};
			var claimsSelectorLoader = await this.CreateClaimsSelectorLoaderAsync(authenticationSchemeName, claimsSelectors);

			var httpContext = await HttpContextFactory.CreateAsync();
			var httpContextAccessor = await this.CreateHttpContextAccessorAsync(httpContext);

			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync(authenticationSchemeRetriever: authenticationSchemeRetriever, claimsSelectorLoader: claimsSelectorLoader, httpContextAccessor: httpContextAccessor, loggerFactory: loggerFactory);
				var claimsSelectionContext = await claimsSelectionContextAccessor.GetAsync(authenticationSchemeName, "/Test");
				Assert.IsNotNull(claimsSelectionContext);
				Assert.AreEqual(authenticationSchemeName, claimsSelectionContext.AuthenticationScheme);
				Assert.AreEqual(1, claimsSelectionContext.Selectors.Count());
				Assert.AreEqual("/ClaimsSelection?ReturnUrl=%2FTest", claimsSelectionContext.Url.OriginalString);
				Assert.AreEqual(0, loggerFactory.Logs.Count());
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task GetAsync_WithReturnUrlParameter_IfTheReturnUrlParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync();

			try
			{
				await claimsSelectionContextAccessor.GetAsync((string)null);
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(argumentNullException.ParamName == "returnUrl")
					throw;
			}
		}

		[TestMethod]
		public async Task GetAsync_WithReturnUrlParameter_Test01()
		{
			const string authenticationSchemeName = "Test";

			var authenticationScheme = new AuthenticationSchemeMock
			{
				Enabled = true,
				Name = authenticationSchemeName
			};
			var authenticationSchemeRetriever = await this.CreateAuthenticationSchemeRetrieverAsync(authenticationScheme, authenticationSchemeName);

			var claimsSelectors = new List<IClaimsSelector>
			{
				new ClaimsSelectorMock()
			};
			var claimsSelectorLoader = await this.CreateClaimsSelectorLoaderAsync(authenticationSchemeName, claimsSelectors);

			var claims = new ClaimBuilderCollection
			{
				new ClaimBuilder(new Claim(JwtClaimTypes.IdentityProvider, authenticationSchemeName))
			};
			var httpContext = await HttpContextFactory.CreateAsync();
			httpContext.User = await ClaimsPrincipalFactory.CreateAsync(claims);
			var httpContextAccessor = await this.CreateHttpContextAccessorAsync(httpContext);

			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimsSelectionContextAccessor = await this.CreateClaimsSelectionContextAccessorAsync(authenticationSchemeRetriever: authenticationSchemeRetriever, claimsSelectorLoader: claimsSelectorLoader, httpContextAccessor: httpContextAccessor, loggerFactory: loggerFactory);
				var claimsSelectionContext = await claimsSelectionContextAccessor.GetAsync("/Test");
				Assert.IsNotNull(claimsSelectionContext);
				Assert.AreEqual(authenticationSchemeName, claimsSelectionContext.AuthenticationScheme);
				Assert.AreEqual(1, claimsSelectionContext.Selectors.Count());
				Assert.AreEqual("/ClaimsSelection?ReturnUrl=%2FTest", claimsSelectionContext.Url.OriginalString);
				Assert.AreEqual(0, loggerFactory.Logs.Count());
			}
		}

		#endregion
	}
}