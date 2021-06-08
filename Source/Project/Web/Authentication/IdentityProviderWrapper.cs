using System;
using System.Diagnostics.CodeAnalysis;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Models;
using RegionOrebroLan.Abstractions;
using RegionOrebroLan.Web.Authentication;
using RegionOrebroLan.Web.Authentication.Configuration;

namespace HansKindberg.IdentityServer.Web.Authentication
{
	public class IdentityProviderWrapper : Wrapper<IdentityProvider>, IAuthenticationScheme
	{
		#region Fields

		private const AuthenticationSchemeKind _defaultKind = AuthenticationSchemeKind.Remote;
		private static readonly SchemeRegistrationOptions _defaultOptions = new SchemeRegistrationOptions();

		#endregion

		#region Constructors

		public IdentityProviderWrapper(DynamicProviderOptions dynamicProviderOptions, IdentityProvider identityProvider) : base(identityProvider, nameof(identityProvider))
		{
			this.DynamicProviderOptions = dynamicProviderOptions ?? throw new ArgumentNullException(nameof(dynamicProviderOptions));
		}

		#endregion

		#region Properties

		protected internal virtual AuthenticationSchemeKind DefaultKind => _defaultKind;
		protected internal virtual SchemeRegistrationOptions DefaultOptions => _defaultOptions;
		public virtual string DisplayName => this.WrappedInstance.DisplayName;
		protected internal virtual DynamicProviderOptions DynamicProviderOptions { get; }
		public virtual bool Enabled => this.WrappedInstance.Enabled;

		public virtual Type HandlerType
		{
			get
			{
				if(this.WrappedInstance.Type == null)
					return null;

				var dynamicProviderType = this.DynamicProviderOptions.FindProviderType(this.WrappedInstance.Type);

				return dynamicProviderType?.HandlerType;
			}
		}

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
		public virtual string Icon
		{
			get
			{
				if(!this.WrappedInstance.Properties.TryGetValue(nameof(this.Icon), out var icon))
					icon = this.Name;

				return (icon ?? this.DefaultOptions.Icon)?.ToLowerInvariant();
			}
		}

		public virtual int Index
		{
			get
			{
				if(this.WrappedInstance.Properties.TryGetValue(nameof(this.Index), out var value) && int.TryParse(value, out var index))
					return index;

				return this.DefaultOptions.Index;
			}
		}

		public virtual bool Interactive
		{
			get
			{
				if(this.WrappedInstance.Properties.TryGetValue(nameof(this.Interactive), out var value) && bool.TryParse(value, out var interactive))
					return interactive;

				return this.DefaultOptions.Interactive;
			}
		}

		public virtual AuthenticationSchemeKind Kind
		{
			get
			{
				if(this.WrappedInstance.Properties.TryGetValue(nameof(this.Kind), out var value) && Enum.TryParse(value, true, out AuthenticationSchemeKind kind))
					return kind;

				return this.DefaultKind;
			}
		}

		public virtual string Name => this.WrappedInstance.Scheme;

		public virtual bool SignOutSupport
		{
			get
			{
				if(this.WrappedInstance.Properties.TryGetValue(nameof(this.SignOutSupport), out var value) && bool.TryParse(value, out var signOutSupport))
					return signOutSupport;

				return this.DefaultOptions.SignOutSupport;
			}
		}

		#endregion
	}
}