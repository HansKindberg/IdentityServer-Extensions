using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using HansKindberg.RoleService.Models.Authorization.Configuration;
using Microsoft.Extensions.Options;

namespace HansKindberg.RoleService.Models.Authorization
{
	public class WindowsRoleLoader : IRoleLoader
	{
		#region Constructors

		public WindowsRoleLoader(IOptions<RoleResolvingOptions> options)
		{
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
		}

		#endregion

		#region Properties

		protected internal virtual IOptions<RoleResolvingOptions> Options { get; }

		#endregion

		#region Methods

		protected internal virtual async Task<SecurityIdentifier> CreateSecurityIdentifier(string value)
		{
			if(string.IsNullOrWhiteSpace(value))
				return null;

			return await Task.FromResult(new SecurityIdentifier(value));
		}

		protected internal virtual async Task<IEnumerable<Claim>> GetGroupSidClaimsAsync(WindowsPrincipal windowsPrincipal)
		{
			if(windowsPrincipal == null)
				throw new ArgumentNullException(nameof(windowsPrincipal));

			return await Task.FromResult(windowsPrincipal.FindAll(ClaimTypes.GroupSid));
		}

		protected internal virtual async Task<bool> IncludeSecurityIdentifierAsync(SecurityIdentifier securityIdentifier)
		{
			if(securityIdentifier == null)
				return false;

			if(this.Options.Value.MachineRolesEnabled)
				return true;

			return await this.IsDomainSecurityIdentifierAsync(securityIdentifier);
		}

		protected internal virtual async Task<bool> IsDomainSecurityIdentifierAsync(SecurityIdentifier securityIdentifier)
		{
			if(securityIdentifier == null)
				return false;

			return await Task.FromResult(securityIdentifier.AccountDomainSid != null);
		}

		public virtual async Task<IEnumerable<string>> ListAsync(IPrincipal principal)
		{
			// ReSharper disable All

			if(principal == null)
				throw new ArgumentNullException(nameof(principal));

			// This probably will not happen, but anyhow.
			if(principal is WindowsPrincipal windowsPrincipal)
				return await this.ListAsync(windowsPrincipal);

			if(principal is ClaimsPrincipal claimsPrincipal)
			{
				var userPrincipalNameClaim = claimsPrincipal.FindFirst(ClaimTypes.Upn);

				if(userPrincipalNameClaim == null)
					throw new InvalidOperationException("There is no user-principal-name-claim for the principal.");

				try
				{
					using(var windowsIdentity = new WindowsIdentity(userPrincipalNameClaim.Value))
					{
						return await this.ListAsync(new WindowsPrincipal(windowsIdentity));
					}
				}
				catch(SecurityException securityException)
				{
					throw new InvalidOperationException($"Could not create a windows-identity from user-principal-name \"{userPrincipalNameClaim.Value}\".", securityException);
				}
			}

			throw new ArgumentException("Invalid principal.", nameof(principal));

			// ReSharper restore All
		}

		protected internal virtual async Task<IEnumerable<string>> ListAsync(WindowsPrincipal windowsPrincipal)
		{
			var stringComparer = StringComparer.OrdinalIgnoreCase;
			var list = new HashSet<string>(stringComparer);

			foreach(var claim in await this.GetGroupSidClaimsAsync(windowsPrincipal))
			{
				var securityIdentifier = await this.CreateSecurityIdentifier(claim.Value);

				var includeSecurityIdentifierAsync = await this.IncludeSecurityIdentifierAsync(securityIdentifier);

				if(!includeSecurityIdentifierAsync)
					continue;

				var role = await this.TranslateToRoleAsync(securityIdentifier);

				if(role == null)
					continue;

				list.Add(role);
			}

			return list.OrderBy(item => item, stringComparer);
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		protected internal virtual async Task<string> TranslateToRoleAsync(SecurityIdentifier securityIdentifier)
		{
			if(securityIdentifier == null)
				return null;

			try
			{
				var role = securityIdentifier.Translate(typeof(NTAccount)).Value;

				return await Task.FromResult(role);
			}
			catch
			{
				return null;
			}
		}

		#endregion
	}
}