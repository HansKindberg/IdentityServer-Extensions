using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using HansKindberg.RoleService.Models.Authorization;
using HansKindberg.RoleService.Models.Authorization.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Authorization
{
	[TestClass]
	public class WindowsRoleLoaderTest
	{
		#region Methods

		[TestMethod]
		public async Task AnonymousIdentity_MachineRolesDisabled_Test()
		{
			await this.AnonymousIdentityTest(false);
		}

		[TestMethod]
		public async Task AnonymousIdentity_MachineRolesEnabled_Test()
		{
			await this.AnonymousIdentityTest(true);
		}

		protected internal virtual async Task AnonymousIdentityTest(bool machineRolesEnabled)
		{
			var windowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetAnonymous());

			var roles = new WindowsRoleLoader(await this.CreateOptionsAsync(machineRolesEnabled)).ListAsync(windowsPrincipal).Result.ToArray();

			Assert.AreEqual(0, roles.Length);
			Assert.AreEqual(windowsPrincipal.FindAll(ClaimTypes.GroupSid).Count(), roles.Length);
		}

		protected internal virtual async Task<IOptions<RoleResolvingOptions>> CreateOptionsAsync(bool machineRolesEnabled)
		{
			return await Task.FromResult(Options.Create(new RoleResolvingOptions {MachineRolesEnabled = machineRolesEnabled}));
		}

		[TestMethod]
		public async Task CurrentIdentity_MachineRolesDisabled_Test()
		{
			await this.CurrentIdentityTest(false);
		}

		[TestMethod]
		public async Task CurrentIdentity_MachineRolesEnabled_Test()
		{
			await this.CurrentIdentityTest(true);
		}

		protected internal virtual async Task CurrentIdentityTest(bool machineRolesEnabled)
		{
			var windowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

			var roles = new WindowsRoleLoader(await this.CreateOptionsAsync(machineRolesEnabled)).ListAsync(windowsPrincipal).Result.ToArray();

			var groupClaims = windowsPrincipal.FindAll(ClaimTypes.GroupSid).ToArray();

			var securityIdentifiers = groupClaims.Select(claim => new SecurityIdentifier(claim.Value)).ToArray();

			if(!machineRolesEnabled)
				securityIdentifiers = securityIdentifiers.Where(securityIdentifier => securityIdentifier.AccountDomainSid != null).ToArray();

			if(!machineRolesEnabled)
				Assert.IsTrue(securityIdentifiers.Length < groupClaims.Length);

			Assert.AreEqual(securityIdentifiers.Length, roles.Length);
		}

		#endregion
	}
}