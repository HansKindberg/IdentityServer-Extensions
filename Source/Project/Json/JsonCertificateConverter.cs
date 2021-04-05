using System;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace HansKindberg.IdentityServer.Json
{
	/// <summary>
	/// A json-converter for X509Certificate2. This will never include the private-key so it should only be used for public-key certificates.
	/// </summary>
	public class JsonCertificateConverter : JsonConverter<X509Certificate2>
	{
		#region Methods

		public override X509Certificate2 ReadJson(JsonReader reader, Type objectType, X509Certificate2 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			if(reader.Value == null)
				return null;

			var bytes = Convert.FromBase64String((string)reader.Value);

			return new X509Certificate2(bytes);
		}

		public override void WriteJson(JsonWriter writer, X509Certificate2 value, JsonSerializer serializer)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			var bytes = value?.GetRawCertData();

			writer.WriteValue(bytes);
		}

		#endregion
	}
}