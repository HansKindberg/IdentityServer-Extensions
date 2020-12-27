using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
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
		#region Fields

		private static readonly IDiscoveryCache _cache = new DiscoveryCache(IdentityServerHost.IisAuthority);

		#endregion

		#region Methods

		[SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings")]
		private static async Task CallServiceAsync(string token)
		{
			var baseAddress = ApiHost.ResourceApiAuthority;

			using(var httpClient = new HttpClient {BaseAddress = new Uri(baseAddress)})
			{
				httpClient.SetBearerToken(token);
				var response = await httpClient.GetStringAsync("identity");

				WriteGreenLine($"{Environment.NewLine}{Environment.NewLine}Service claims:");
				WriteLine(JArray.Parse(response));
			}
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public static async Task Main()
		{
			Console.Title = "Device";

			try
			{
				var authorizeResponse = await RequestAuthorizationAsync();

				var tokenResponse = await RequestTokenAsync(authorizeResponse);
				tokenResponse.Show();

				ReadLine();
				await CallServiceAsync(tokenResponse.AccessToken);
			}
			catch(Exception exception)
			{
				WriteRedLine(exception.ToString());
			}

			WriteYellowLine("Press any key to close...");
			ReadLine();
		}

		private static async Task<DeviceAuthorizationResponse> RequestAuthorizationAsync()
		{
			var disco = await _cache.GetAsync();

			if(disco.IsError)
				throw new InvalidOperationException(disco.Error);

			using(var httpClient = new HttpClient())
			{
				using(var deviceAuthorizationRequest = new DeviceAuthorizationRequest {Address = disco.DeviceAuthorizationEndpoint, ClientId = "device"})
				{
					var response = await httpClient.RequestDeviceAuthorizationAsync(deviceAuthorizationRequest);

					if(response.IsError)
						throw new InvalidOperationException(response.Error);

					WriteLine($"user code   : {response.UserCode}");
					WriteLine($"device code : {response.DeviceCode}");
					WriteLine($"URL         : {response.VerificationUri}");
					WriteLine($"Complete URL: {response.VerificationUriComplete}");

					WriteLine($"{Environment.NewLine}Press enter to launch browser ({response.VerificationUri})");
					ReadLine();

					Process.Start(new ProcessStartInfo(response.VerificationUriComplete) {UseShellExecute = true});

					return response;
				}
			}
		}

		[SuppressMessage("Style", "IDE0078:Use pattern matching")]
		private static async Task<TokenResponse> RequestTokenAsync(DeviceAuthorizationResponse authorizeResponse)
		{
			var disco = await _cache.GetAsync();

			if(disco.IsError)
				throw new InvalidOperationException(disco.Error);

			using(var httpClient = new HttpClient())
			{
				while(true)
				{
					using(var deviceTokenRequest = new DeviceTokenRequest())
					{
						deviceTokenRequest.Address = disco.TokenEndpoint;
						deviceTokenRequest.ClientId = "device";
						deviceTokenRequest.DeviceCode = authorizeResponse.DeviceCode;

						var response = await httpClient.RequestDeviceTokenAsync(deviceTokenRequest);

						if(response.IsError)
						{
							if(response.Error == OidcConstants.TokenErrors.AuthorizationPending || response.Error == OidcConstants.TokenErrors.SlowDown)
							{
								WriteLine($"{response.Error}...waiting.");
								Thread.Sleep(authorizeResponse.Interval * 1000);
							}
							else
							{
								throw new InvalidOperationException(response.Error);
							}
						}
						else
						{
							return response;
						}
					}
				}
			}
		}

		#endregion
	}
}