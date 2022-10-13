using System;
using HansKindberg.IdentityServer.Web.Authentication.Cookies.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace HansKindberg.IdentityServer.Configuration
{
	public class IntermediateCookieAuthenticationOptions : CookieAuthenticationOptions
	{
		#region Fields

		private string _name;

		#endregion

		#region Constructors

		public IntermediateCookieAuthenticationOptions()
		{
			this.SetDefaults();
		}

		public IntermediateCookieAuthenticationOptions(TimeSpan expiration, string name, SameSiteMode sameSite) : this()
		{
			this._name = name;
			this.Cookie.Name = name;
			this.Cookie.SameSite = sameSite;
			this.ExpireTimeSpan = expiration;
		}

		#endregion

		#region Properties

		public virtual bool Enabled { get; set; } = true;

		public virtual string Name
		{
			get => this._name;
			set => this._name = value;
		}

		#endregion

		#region Methods

		public virtual void Configure(CookieAuthenticationOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			options.AccessDeniedPath = this.AccessDeniedPath;
			options.ClaimsIssuer = this.ClaimsIssuer;
			options.Cookie = this.Cookie;
			options.CookieManager = this.CookieManager;
			options.DataProtectionProvider = this.DataProtectionProvider;
			options.Events = this.Events;
			options.EventsType = this.EventsType;
			options.ExpireTimeSpan = this.ExpireTimeSpan;
			options.ForwardAuthenticate = this.ForwardAuthenticate;
			options.ForwardChallenge = this.ForwardChallenge;
			options.ForwardDefault = this.ForwardDefault;
			options.ForwardDefaultSelector = this.ForwardDefaultSelector;
			options.ForwardForbid = this.ForwardForbid;
			options.ForwardSignIn = this.ForwardSignIn;
			options.ForwardSignOut = this.ForwardSignOut;
			options.LoginPath = this.LoginPath;
			options.LogoutPath = this.LogoutPath;
			options.ReturnUrlParameter = this.ReturnUrlParameter;
			options.SessionStore = this.SessionStore;
			options.SlidingExpiration = this.SlidingExpiration;
			options.TicketDataFormat = this.TicketDataFormat;
		}

		#endregion
	}
}