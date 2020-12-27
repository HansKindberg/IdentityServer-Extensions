using System;
using System.Text;
using IdentityModel;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace Helper.IdentityModel.Client.Extensions
{
	public static class TokenResponseExtension
	{
		#region Methods

		public static void Show(this TokenResponse response)
		{
			if(response == null)
				throw new ArgumentNullException(nameof(response));

			if(!response.IsError)
			{
				ConsoleHelper.WriteGreenLine("Token response:");
				ConsoleHelper.WriteLine(response.Json);

				// ReSharper disable InvertIf
				if(response.AccessToken.Contains("."))
				{
					ConsoleHelper.WriteGreenLine("Access Token (decoded):");

					var parts = response.AccessToken.Split('.');
					var header = parts[0];
					var claims = parts[1];

					ConsoleHelper.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(header))));
					ConsoleHelper.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(claims))));
				}
				// ReSharper restore InvertIf
			}
			else
			{
				if(response.ErrorType == ResponseErrorType.Http)
				{
					ConsoleHelper.WriteGreenLine("HTTP error:");
					ConsoleHelper.WriteLine(response.Error);
					ConsoleHelper.WriteGreenLine("HTTP status code:");
					ConsoleHelper.WriteLine(response.HttpStatusCode);
				}
				else
				{
					ConsoleHelper.WriteGreenLine("Protocol error response:");
					ConsoleHelper.WriteLine(response.Raw);
				}
			}
		}

		#endregion
	}
}