using System;
using System.Diagnostics.CodeAnalysis;
using HansKindberg.IdentityServer.Web.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTests.Web.Localization
{
	[TestClass]
	public class OpenIdConnectRequestCultureProviderTest
	{
		#region Fields

		private const string _urlWithDuplicateCultures = "https://localhost:44300/Account/SignIn?returnUrl=%2Fconnect%2Fauthorize%2Fcallback%3Fclient_id%3DWindows-UPN-Web-Application%26redirect_uri%3Dhttps%253A%252F%252Flocalhost%253A44380%252Fsignin-oidc%26response_type%3Dcode%2520id_token%26scope%3Dcertificate%2520email%2520openid%2520profile%2520windows%26response_mode%3Dform_post%26nonce%3D637158143939107725.MTQ3MTM0ZjQtNmZmZS00ZWEwLWFjOTMtY2UzOWM4NWVhMWYxNTFiNGE0YjEtOGU4Zi00MjEwLTlhZTAtYWUwNTUyNWFkMDcz%26code_challenge%3DZujtXEVWYagBBL7t4TZ6K_OZimvmmFOeiOrO0Swrl68%26code_challenge_method%3DS256%26ui_locales%3Dde-DE%2520sv-SE%2520fi-FI%2520sv-SE%2520sv-LL%2520sv-SE%2520sv%2520sv-SE%26state%3DCfDJ8KLxABfzZ2hElgLPWTm3y2YGwJjgnFgUCRwC_5-EosKsxvKpOosikg_gQnvVj9nwOVh1L1Gh5-dBvM_0lLRosm7NFnhS0V-Kc51-uq9lmI11n-QzHb7qRelVovotJcpZOZOJybxGwJo2oPRPqSUTetu1qn0IDyu5Mpk2ZvZnbhzL7CqTncxPdPFFHf7EpYHtbTc2LkJN19Aj7r7sM9DyM9biYtl4WmFEPJa8jP5659OGVMt6bfdsgV7GXnGmOEht-hcmPrXIiwjpCElhwORBOib79Ca1CKJK7E2M5RzqgaMVh5rr7D4L1kjm86JRp24cdw3MfXBDJ_G2MjGt9khdxOwvqLtxFmFWKRR5VJeT_zXD0Wy-J1SCjNae7PcRFGJuyg%26x-client-SKU%3DID_NETSTANDARD2_0%26x-client-ver%3D5.5.0.0";
		private const string _urlWithManyCultures = "https://localhost:44300/Account/SignIn?returnUrl=%2Fconnect%2Fauthorize%2Fcallback%3Fclient_id%3DWindows-UPN-Web-Application%26redirect_uri%3Dhttps%253A%252F%252Flocalhost%253A44380%252Fsignin-oidc%26response_type%3Dcode%2520id_token%26scope%3Dcertificate%2520email%2520openid%2520profile%2520windows%26response_mode%3Dform_post%26nonce%3D637158112968832202.ZjA1MTlhZDAtZDIyNC00MTBmLWEzZWQtNzZmMDJhNzUyN2M5NTc5YjQ3YTEtNDBiNS00NDM4LWE4ZjctYTllOTAxOTg5ZDcx%26code_challenge%3D0C-JZwToLXbtTZieZxuqnjyN5_D03PCPdnuwV-Z-Yls%26code_challenge_method%3DS256%26ui_locales%3Dde-DE%2520fi-FI%2520sv-LL%2520sv-SE%2520sv%26state%3DCfDJ8KLxABfzZ2hElgLPWTm3y2Z-OVOPRcvzzno81aFo7n5MhWyOYNx59JzuTRyEBbJwZ5luWDcISwVmZR7q002pihgxE8JHK-hJUdaLGMsIKHPZuHl10J-COV1XvZigt8fetT53QYmj3oNTeMA90Kpq3V-OgnsEJfiWs3qImwSdHKvy_1mDmmsc9nyapyx588hQuLEgHFLpQ3c532P9brz-l8A6dcHDccqrDVoS4xUWZGLLh3Xq8FjbAOcy4qU9-4iEopUobdLYJUMA4gw7T62blNZp5pAlvdOTnI4AOqfOLc4AOQFc23_cKNz5Zq3xrFbBI-jIEsKVOvAoMZrlY272N3zIp5tmabg6Jy_FtJLaKQigSYTf7DK3lbCjd5esMwYzSQ%26x-client-SKU%3DID_NETSTANDARD2_0%26x-client-ver%3D5.5.0.0";
		private const string _urlWithNoCulture = "https://localhost:44300/Account/SignIn?returnUrl=%2Fconnect%2Fauthorize%2Fcallback%3Fclient_id%3DWindows-UPN-Web-Application%26redirect_uri%3Dhttps%253A%252F%252Flocalhost%253A44380%252Fsignin-oidc%26response_type%3Dcode%2520id_token%26scope%3Dcertificate%2520email%2520openid%2520profile%2520windows%26response_mode%3Dform_post%26nonce%3D637158113834906537.ZTkxYjdkYTUtN2QyMi00NTU4LThiNGMtZGUzNWMzNzc3NGE3YjM2MThkNzItYjYxZi00ZDA1LTlhNDQtYzRiYTNjNTVmZmJm%26code_challenge%3DviugLXNNVAL7mTbLBCefF4Hq_DCyNWIOXSdgYVp9tLc%26code_challenge_method%3DS256%26state%3DCfDJ8KLxABfzZ2hElgLPWTm3y2aJwQCgJw_3bVVzfvjF7wzI2tPQq38e5u_H9-EHW4K-X1PcbxVmb0Cz2O9EfsJDo8B5_luL9GMlmOtocG4CeMl5aazszBhtQj993QFsrrtVSILdTdKBmJ0bbZzJChmDnCbUrulL-z_WTUYwwUQNl-FwTRDy9x58hS83w6VhhOGWip6pHkrgvvXmW0WoQYN7F0GJtHfyvijv-9efUyR4qfxUcq9x61hk0p_91ytcIooL0LnHOJYiK6NeHhMvJ_5hylfFyyNex9vUtuRL_aD63Z0oMD3mDkQfPwHW5gd5iC-IgeKsOgGORNK978fSu40_RoVKdtX5OUjLmYlvMm2q5lZxN4WoVvXS2jiSQ7IUGynX0Q%26x-client-SKU%3DID_NETSTANDARD2_0%26x-client-ver%3D5.5.0.0";
		private const string _urlWithOneCulture = "https://localhost:44300/Account/SignIn?returnUrl=%2Fconnect%2Fauthorize%2Fcallback%3Fclient_id%3DWindows-UPN-Web-Application%26redirect_uri%3Dhttps%253A%252F%252Flocalhost%253A44380%252Fsignin-oidc%26response_type%3Dcode%2520id_token%26scope%3Dcertificate%2520email%2520openid%2520profile%2520windows%26response_mode%3Dform_post%26nonce%3D637158111008569368.MzA2MGZmM2YtZjkxZC00MWY2LTgwMzUtYTVmNDkyMmVjNGI3ZGRmOGMxZTYtOGU1MS00MGVhLTlmMTEtMTUxNzgwNGU4NThh%26code_challenge%3DPFykIZ8U-R2e71X6RZuqO5eTyla66pxc2_Kp4BmoqMg%26code_challenge_method%3DS256%26ui_locales%3Dsv-SE%26state%3DCfDJ8KLxABfzZ2hElgLPWTm3y2bO0MhjVQiXNpxRxXdYHuj08NALLV-luGxm3TlYjBEZ_r4rkhpZMJUUW_TBFD_LjJ4BEVFevGJjXMxmH9ncwKjSwUws5SY6MWaaeXEdOocDYAM3oZheeUqY5LAJKrvlzPyfisAKMx6YbRVjjqdLUjJRSyKiycBg9rHCqri8DFvIWdYI6A_hbA4w58Kcm74sQW2xs0u1EqeNG69Vn4fC31Qqt8teqI_J--2JsfYvJhvhvaBOtJZjycAdlixun_dCL3FuuQmZmkIwIqrE6fAOAL_HSKDckdWH9pMyI2GnT2oVgu5lhYhFllW2IZqfo6qA2GD4UD_zaad6ayP6g8moIZEwpnzj1fjjQPzELNAv3hJxsQ%26x-client-SKU%3DID_NETSTANDARD2_0%26x-client-ver%3D5.5.0.0";

		#endregion

		#region Methods

		[SuppressMessage("Design", "CA1054:Uri parameters should not be strings")]
		protected internal virtual HttpContext CreateHttpContext(string url)
		{
			var uri = new Uri(url, UriKind.Absolute);
			var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
			var query = new QueryCollection(queryDictionary);

			var httpRequestMock = new Mock<HttpRequest>();
			httpRequestMock.Setup(httpRequest => httpRequest.Query).Returns(query);

			var httpContextMock = new Mock<HttpContext>();
			httpContextMock.Setup(httpContext => httpContext.Request).Returns(httpRequestMock.Object);

			return httpContextMock.Object;
		}

		protected internal virtual OpenIdConnectRequestCultureProvider CreateOpenIdConnectRequestCultureProvider()
		{
			return new OpenIdConnectRequestCultureProvider();
		}

		[TestMethod]
		public void Test()
		{
			var openIdConnectRequestCultureProvider = this.CreateOpenIdConnectRequestCultureProvider();

			var result = openIdConnectRequestCultureProvider.DetermineProviderCultureResult(this.CreateHttpContext(_urlWithDuplicateCultures)).Result;

			Assert.AreEqual(5, result.Cultures.Count);
			Assert.AreEqual("de-DE", result.Cultures[0].Value);
			Assert.AreEqual("sv-SE", result.Cultures[1].Value);
			Assert.AreEqual("fi-FI", result.Cultures[2].Value);
			Assert.AreEqual("sv-LL", result.Cultures[3].Value);
			Assert.AreEqual("sv", result.Cultures[4].Value);

			Assert.AreEqual(5, result.UICultures.Count);
			Assert.AreEqual("de-DE", result.UICultures[0].Value);
			Assert.AreEqual("sv-SE", result.UICultures[1].Value);
			Assert.AreEqual("fi-FI", result.UICultures[2].Value);
			Assert.AreEqual("sv-LL", result.UICultures[3].Value);
			Assert.AreEqual("sv", result.UICultures[4].Value);

			result = openIdConnectRequestCultureProvider.DetermineProviderCultureResult(this.CreateHttpContext(_urlWithManyCultures)).Result;

			Assert.AreEqual(5, result.Cultures.Count);
			Assert.AreEqual("de-DE", result.Cultures[0].Value);
			Assert.AreEqual("fi-FI", result.Cultures[1].Value);
			Assert.AreEqual("sv-LL", result.Cultures[2].Value);
			Assert.AreEqual("sv-SE", result.Cultures[3].Value);
			Assert.AreEqual("sv", result.Cultures[4].Value);

			Assert.AreEqual(5, result.UICultures.Count);
			Assert.AreEqual("de-DE", result.UICultures[0].Value);
			Assert.AreEqual("fi-FI", result.UICultures[1].Value);
			Assert.AreEqual("sv-LL", result.UICultures[2].Value);
			Assert.AreEqual("sv-SE", result.UICultures[3].Value);
			Assert.AreEqual("sv", result.UICultures[4].Value);

			result = openIdConnectRequestCultureProvider.DetermineProviderCultureResult(this.CreateHttpContext(_urlWithNoCulture)).Result;

			Assert.IsNull(result);

			result = openIdConnectRequestCultureProvider.DetermineProviderCultureResult(this.CreateHttpContext(_urlWithOneCulture)).Result;

			Assert.AreEqual(1, result.Cultures.Count);
			Assert.AreEqual("sv-SE", result.Cultures[0].Value);

			Assert.AreEqual(1, result.UICultures.Count);
			Assert.AreEqual("sv-SE", result.UICultures[0].Value);
		}

		#endregion
	}
}