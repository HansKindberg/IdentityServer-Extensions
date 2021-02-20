using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using HansKindberg.RoleService.Controllers;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Controllers
{
	// ReSharper disable All
	[TestClass]
	public class RoleControllerTest
	{
		#region Methods

		protected internal virtual async Task<ControllerContext> CreateControllerContextAsync(ClaimsPrincipal claimsPrincipal, IServiceProvider serviceProvider)
		{
			var actionDescriptor = new ControllerActionDescriptor
			{
				ControllerTypeInfo = typeof(RoleController).GetTypeInfo()
			};

			var actionContext = new ActionContext(await this.CreateHttpContextAsync(claimsPrincipal, serviceProvider), new RouteData(), actionDescriptor);

			return await Task.FromResult(new ControllerContext(actionContext));
		}

		protected internal virtual async Task<HttpContext> CreateHttpContextAsync(ClaimsPrincipal claimsPrincipal, IServiceProvider serviceProvider)
		{
			return await Task.FromResult(new DefaultHttpContext
			{
				RequestServices = serviceProvider,
				User = claimsPrincipal
			});
		}

		protected internal virtual async Task<IEnumerable<string>> GetCurrentWindowsUserRoles()
		{
			var securityIdentifiers = WindowsIdentity.GetCurrent().Groups
				.Cast<SecurityIdentifier>()
				.Where(securityIdentifier => securityIdentifier.AccountDomainSid != null);

			var identityReferences = new IdentityReferenceCollection();

			foreach(var securityIdentifier in securityIdentifiers)
			{
				identityReferences.Add(securityIdentifier);
			}

			var roles = identityReferences.Translate(typeof(NTAccount)).Select(ntAccount => ntAccount.Value);
			roles = roles.Where(role => !role.StartsWith($"{Environment.MachineName}\\", StringComparison.OrdinalIgnoreCase));
			roles = roles.OrderBy(item => item, StringComparer.OrdinalIgnoreCase);

			return await Task.FromResult(roles);
		}

		[TestMethod]
		public async Task List_IfTheHttpContextUserHasANameIdentifierClaimOrASubjectClaimThatIsConfigured_ShouldReturnRolesThatAreConfigured()
		{
			foreach(var nameIdentifierClaimType in new[] {JwtClaimTypes.Subject, ClaimTypes.NameIdentifier})
			{
				var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {new Claim(nameIdentifierClaimType, "40f297bb-47eb-464b-bb07-80b1ec3f862d")}, "Test"));

				using(var webApplicationFactory = new WebApplicationFactory())
				{
					var controllerFactory = webApplicationFactory.Services.GetRequiredService<IControllerFactory>();
					var roleController = (RoleController)controllerFactory.CreateController(await this.CreateControllerContextAsync(claimsPrincipal, webApplicationFactory.Services));
					var roles = (await roleController.List()).ToArray();
					Assert.AreEqual(1, roles.Length);
					Assert.AreEqual("Administrators", roles.First());
				}
			}
		}

		[TestMethod]
		public async Task List_IfTheHttpContextUserHasAnUpnClaimWithTheSameValueAsTheUpnOfTheCurrentWindowsUser_ShouldReturnTheActiveDirectoryRolesOfTheCurrentWindowsUser()
		{
			var currentUserPrincipalName = UserPrincipal.Current.UserPrincipalName;
			var currentWindowsUserRoles = (await this.GetCurrentWindowsUserRoles()).ToArray();

			foreach(var userPrincipalNameClaimType in new[] {"upn", ClaimTypes.Upn})
			{
				var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {new Claim(userPrincipalNameClaimType, currentUserPrincipalName)}, "Test"));

				using(var webApplicationFactory = new WebApplicationFactory())
				{
					var controllerFactory = webApplicationFactory.Services.GetRequiredService<IControllerFactory>();
					var roleController = (RoleController)controllerFactory.CreateController(await this.CreateControllerContextAsync(claimsPrincipal, webApplicationFactory.Services));
					var roles = (await roleController.List()).ToArray();
					Assert.AreEqual(currentWindowsUserRoles.Length, roles.Length);
					for(var i = 0; i < currentWindowsUserRoles.Length; i++)
					{
						Assert.AreEqual(currentWindowsUserRoles[i], roles[i]);
					}
				}
			}
		}

		[TestMethod]
		public async Task List_IfTheHttpContextUserIsNotAuthenticated_ShouldReturnAnEmptyList()
		{
			var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

			Assert.IsFalse(claimsPrincipal.Identity.IsAuthenticated);

			using(var webApplicationFactory = new WebApplicationFactory())
			{
				var controllerFactory = webApplicationFactory.Services.GetRequiredService<IControllerFactory>();
				var roleController = (RoleController)controllerFactory.CreateController(await this.CreateControllerContextAsync(claimsPrincipal, webApplicationFactory.Services));
				var roles = await roleController.List();
				Assert.IsFalse(roles.Any());
			}
		}

		[TestMethod]
		public async Task List_IfTheHttpContextUserIsNull_ShouldReturnAnEmptyList()
		{
			using(var webApplicationFactory = new WebApplicationFactory())
			{
				var controllerFactory = webApplicationFactory.Services.GetRequiredService<IControllerFactory>();
				var roleController = (RoleController)controllerFactory.CreateController(await this.CreateControllerContextAsync(null, webApplicationFactory.Services));
				var roles = await roleController.List();
				Assert.IsFalse(roles.Any());
			}
		}

		[TestMethod]
		public async Task List_IfTheHttpContextUserIsTheCurrentWindowsUser_ShouldReturnTheActiveDirectoryRolesOfTheCurrentWindowsUser()
		{
			var claimsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
			var currentWindowsUserRoles = (await this.GetCurrentWindowsUserRoles()).ToArray();

			using(var webApplicationFactory = new WebApplicationFactory())
			{
				var controllerFactory = webApplicationFactory.Services.GetRequiredService<IControllerFactory>();
				var roleController = (RoleController)controllerFactory.CreateController(await this.CreateControllerContextAsync(claimsPrincipal, webApplicationFactory.Services));
				var roles = (await roleController.List()).ToArray();
				Assert.AreEqual(currentWindowsUserRoles.Length, roles.Length);
				for(var i = 0; i < currentWindowsUserRoles.Length; i++)
				{
					Assert.AreEqual(currentWindowsUserRoles[i], roles[i]);
				}
			}
		}

		#endregion
	}
	// ReSharper restore All
}