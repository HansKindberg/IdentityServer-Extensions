using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RegionOrebroLan.Logging.Extensions;

namespace HansKindberg.IdentityServer.Validation
{
	/// <inheritdoc />
	public class X509IssuerSecretValidator : ISecretValidator
	{
		#region Fields

		private const string _secretType = "X509Issuer";

		#endregion

		#region Constructors

		public X509IssuerSecretValidator(ILoggerFactory loggerFactory)
		{
			if(loggerFactory == null)
				throw new ArgumentNullException(nameof(loggerFactory));

			this.Logger = loggerFactory.CreateLogger(this.GetType().FullName);
		}

		#endregion

		#region Properties

		protected internal virtual ILogger Logger { get; }
		protected internal virtual string SecretType => _secretType;

		#endregion

		#region Methods

		[SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
		public virtual Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
		{
			var fail = Task.FromResult(new SecretValidationResult { Success = false });

			if(parsedSecret == null || parsedSecret.Type != IdentityServerConstants.ParsedSecretTypes.X509Certificate)
			{
				this.Logger.LogDebugIfEnabled($"{this.SecretType} secret validator cannot process {parsedSecret?.Type ?? "null"}.");

				return fail;
			}

			if(!(parsedSecret.Credential is X509Certificate2 certificate))
				throw new InvalidOperationException("Credential is not a x509 certificate.");

			var issuer = certificate.Issuer;
			if(issuer == null)
			{
				this.Logger.LogWarningIfEnabled("The certificate-issuer is null.");

				return fail;
			}

			var issuerSecrets = secrets.Where(secret => string.Equals(secret.Type, this.SecretType, StringComparison.Ordinal)).ToArray();
			if(!issuerSecrets.Any())
			{
				this.Logger.LogDebugIfEnabled($"No {this.SecretType} secrets configured for client.");

				return fail;
			}

			// ReSharper disable All
			foreach(var secret in issuerSecrets)
			{
				var secretDescription = string.IsNullOrEmpty(secret.Description) ? "no description" : secret.Description;

				if(issuer.Equals(secret.Value, StringComparison.OrdinalIgnoreCase))
				{
					var values = new Dictionary<string, string>
					{
						{ "x5t#S256", certificate.Thumbprint }
					};

					var confirmation = JsonConvert.SerializeObject(values);

					var result = new SecretValidationResult
					{
						Success = true,
						Confirmation = confirmation
					};

					return Task.FromResult(result);
				}
			}
			// ReSharper restore All

			this.Logger.LogDebugIfEnabled($"No matching {this.SecretType} secret found.");

			return fail;
		}

		#endregion
	}
}