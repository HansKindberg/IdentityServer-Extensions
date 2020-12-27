using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Application
{
	public class ConfirmationValidationMiddleware
	{
		#region Constructors

		public ConfirmationValidationMiddleware(RequestDelegate next)
		{
			this.Next = next;
		}

		#endregion

		#region Properties

		[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
		protected internal virtual RequestDelegate Next { get; }

		#endregion

		#region Methods

		public virtual async Task Invoke(HttpContext context)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			if(context.User.Identity != null && context.User.Identity.IsAuthenticated)
			{
				var cnfJson = context.User.FindFirst("cnf")?.Value;

				if(!string.IsNullOrWhiteSpace(cnfJson))
				{
					var certificateResult = await context.AuthenticateAsync("x509");
					if(!certificateResult.Succeeded)
					{
						await context.ChallengeAsync("x509");

						return;
					}

					var certificate = context.Connection.ClientCertificate;
					if(certificate == null)
					{
						await context.ChallengeAsync("x509");

						return;
					}

					var thumbprint = certificate.Thumbprint;

					var cnf = JObject.Parse(cnfJson);
					var sha256 = cnf.Value<string>("x5t#S256");

					if(string.IsNullOrWhiteSpace(sha256) || thumbprint == null || !thumbprint.Equals(sha256, StringComparison.OrdinalIgnoreCase))
					{
						await context.ChallengeAsync("token");

						return;
					}
				}
			}

			await this.Next(context);
		}

		#endregion
	}
}