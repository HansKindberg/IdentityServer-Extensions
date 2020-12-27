using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Helper;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
	public class Startup
	{
		#region Methods

		public void Configure(IApplicationBuilder applicationBuilder)
		{
			applicationBuilder.UseDeveloperExceptionPage();

			applicationBuilder.UseCors(policy =>
			{
				policy.WithOrigins(
					"http://localhost:28895",
					"https://localhost:44312",
					"http://localhost:7017");

				policy.AllowAnyHeader();
				policy.AllowAnyMethod();
				policy.WithExposedHeaders("WWW-Authenticate");
			});

			applicationBuilder.UseRouting();
			applicationBuilder.UseAuthentication();
			applicationBuilder.UseAuthorization();
			applicationBuilder.UseMiddleware<ConfirmationValidationMiddleware>();

			applicationBuilder.UseEndpoints(endpointRouteBuilder => endpointRouteBuilder.MapDefaultControllerRoute());
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddCors();
			services.AddDistributedMemoryCache();

			services.AddAuthentication("token")
				.AddIdentityServerAuthentication("token", options =>
				{
					options.ApiName = "resource1";
					options.ApiSecret = "secret";
					options.Authority = IdentityServerHost.IisAuthority;
					options.RequireHttpsMetadata = false;
				})
				.AddCertificate("x509", options =>
				{
					options.RevocationMode = X509RevocationMode.NoCheck;

					options.Events = new CertificateAuthenticationEvents
					{
						OnCertificateValidated = context =>
						{
							context.Principal = Principal.CreateFromCertificate(context.ClientCertificate, includeAllClaims: true);
							context.Success();

							return Task.CompletedTask;
						}
					};
				});
		}

		#endregion
	}
}