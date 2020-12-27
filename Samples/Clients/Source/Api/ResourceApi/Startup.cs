using Helper;
using IdentityModel.AspNetCore.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
	public class Startup
	{
		#region Methods

		public virtual void Configure(IApplicationBuilder applicationBuilder)
		{
			applicationBuilder.UseCors(policy =>
			{
				policy.WithOrigins("https://localhost:44300");
				policy.AllowAnyHeader();
				policy.AllowAnyMethod();
				policy.WithExposedHeaders("WWW-Authenticate");
			});
			applicationBuilder.UseRouting();
			applicationBuilder.UseAuthentication();
			applicationBuilder.UseAuthorization();
			applicationBuilder.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers().RequireAuthorization();
			});
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddCors();
			services.AddDistributedMemoryCache();
			services.AddAuthentication("token")

				// JWT tokens
				.AddJwtBearer("token", options =>
				{
					options.Authority = IdentityServerHost.IisAuthority;
					options.Audience = "resource1";

					options.TokenValidationParameters.ValidTypes = new[] {"at+jwt"};

					// if token does not contain a dot, it is a reference token
					options.ForwardDefaultSelector = Selector.ForwardReferenceToken("introspection");
				})

				// reference tokens
				.AddOAuth2Introspection("introspection", options =>
				{
					options.Authority = IdentityServerHost.IisAuthority;
					options.ClientId = "resource1";
					options.ClientSecret = "secret";
				});

			services.AddScopeTransformation();
		}

		#endregion
	}
}