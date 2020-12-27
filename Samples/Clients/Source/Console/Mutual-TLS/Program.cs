using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Helper;
using Helper.IdentityModel.Client.Extensions;
using IdentityModel;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace Application
{
	public class Program : ConsoleHelper
	{
		#region Methods

		[SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings")]
		private static async Task CallServiceAsync(string token)
		{
			using(var httpClientHandler = new HttpClientHandler())
			{
				var certificate = await GetCertificateAsync();
				httpClientHandler.ClientCertificates.Add(certificate);

				using(var httpClient = new HttpClient(httpClientHandler) {BaseAddress = new Uri(ApiHost.ResourceApiAuthority)})
				{
					httpClient.SetBearerToken(token);
					var response = await httpClient.GetStringAsync("identity");

					WriteGreenLine("Service claims:");
					WriteLine(JArray.Parse(response));
				}
			}
		}

		private static async Task<X509Certificate2> GetCertificateAsync()
		{
			return await Task.FromResult(CertificateLoader.GetClientCertificateWithEmailAndUpn());
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public static async Task Main()
		{
			Console.Title = "Mutual TLS client";

			try
			{
				CertificateLoader.ValidateTrustedRootCertificate();

				var response = await RequestTokenAsync();
				response.Show();

				WriteYellowLine("Press any key to continue...");
				ReadLine();

				await CallServiceAsync(response.AccessToken);
			}
			catch(Exception exception)
			{
				WriteRedLine(exception.ToString());
			}

			WriteYellowLine("Press any key to close...");
			ReadLine();
		}

		private static async Task<TokenResponse> RequestTokenAsync()
		{
			using(var httpClientHandler = new HttpClientHandler())
			{
				var certificate = await GetCertificateAsync();
				httpClientHandler.ClientCertificates.Add(certificate);

				using(var httpClient = new HttpClient(httpClientHandler))
				{
					var disco = await httpClient.GetDiscoveryDocumentAsync(IdentityServerHost.IisAuthority);

					if(disco.IsError)
						throw new InvalidOperationException(disco.Error);

					using(var clientCredentialsTokenRequest = new ClientCredentialsTokenRequest())
					{
						clientCredentialsTokenRequest.Address = disco
							.TryGetValue(OidcConstants.Discovery.MtlsEndpointAliases)
							.Value<string>(OidcConstants.Discovery.TokenEndpoint);

						clientCredentialsTokenRequest.ClientId = "MTLS-Test";
						clientCredentialsTokenRequest.Scope = "resource1.scope1";

						var response = await httpClient.RequestClientCredentialsTokenAsync(clientCredentialsTokenRequest);

						if(response.IsError)
							throw new InvalidOperationException(response.Error);

						return response;
					}
				}
			}
		}

		#endregion
	}
}