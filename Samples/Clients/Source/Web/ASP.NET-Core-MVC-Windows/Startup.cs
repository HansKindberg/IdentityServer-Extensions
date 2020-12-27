using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Application.Models.Configuration;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Application
{
	public class Startup
	{
		#region Fields

		public const string OpenIdConnectScheme = "OpenIdConnect";

		#endregion

		#region Constructors

		public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
		{
			this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.HostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
		}

		#endregion

		#region Properties

		protected internal virtual IConfiguration Configuration { get; }
		protected internal virtual IWebHostEnvironment HostEnvironment { get; }

		#endregion

		#region Methods

		public virtual void Configure(IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			applicationBuilder
				.UseDeveloperExceptionPage()
				.UseStaticFiles()
				.UseRouting()
				.UseAuthentication()
				.UseAuthorization()
				.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
		}

		public virtual void ConfigureServices(IServiceCollection services)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

			services.Configure<RoleServiceOptions>(this.Configuration.GetSection(ConfigurationKeys.RoleServicePath));

			services.AddAuthentication(options =>
				{
					options.DefaultChallengeScheme = OpenIdConnectScheme;
					options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				})
				.AddCookie(options =>
				{
					options.Cookie.Name = "ASP.NET-Core-MVC-Windows";
				})
				.AddOpenIdConnect(OpenIdConnectScheme, options =>
				{
					this.Configuration.GetSection(OpenIdConnectScheme).Bind(options);

					options.ClaimActions.MapAllExcept("aud", "c_hash", "exp", "iat", "iss", "nbf", "nonce");

					const string codeVerifierKey = "code_verifier";

					options.Events.OnAuthorizationCodeReceived = context =>
					{
						if(context.TokenEndpointRequest?.GrantType == OpenIdConnectGrantTypes.AuthorizationCode && context.Properties.Items.TryGetValue(codeVerifierKey, out var codeVerifier))
							context.TokenEndpointRequest.Parameters[codeVerifierKey] = codeVerifier;

						return Task.CompletedTask;
					};

					options.Events.OnRedirectToIdentityProvider = context =>
					{
						// ReSharper disable InvertIf
						if(context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
						{
							var codeVerifier = CryptoRandom.CreateUniqueId(32);
							context.Properties.Items[codeVerifierKey] = codeVerifier;

							string codeChallenge;
							using(var sha256 = SHA256.Create())
							{
								var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
								codeChallenge = Base64Url.Encode(challengeBytes);
							}

							context.ProtocolMessage.Parameters["code_challenge"] = codeChallenge;
							context.ProtocolMessage.Parameters["code_challenge_method"] = "S256";
						}
						// ReSharper restore InvertIf

						return Task.CompletedTask;
					};
				});

			services.AddControllersWithViews();

			services.AddHttpClient();
		}

		#endregion
	}
}