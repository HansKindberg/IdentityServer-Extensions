using System;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UserEntity = HansKindberg.IdentityServer.Identity.User;
using UserModel = HansKindberg.IdentityServer.Identity.Models.User;

namespace UnitTests.Identity
{
	[TestClass]
	public class IdentityFacadeTest
	{
		#region Methods

		protected internal virtual async Task<IdentityFacade> CreateIdentityFacadeAsync()
		{
			var httpContextAccessor = Mock.Of<IHttpContextAccessor>();
			var userClaimsPrincipalFactory = Mock.Of<IUserClaimsPrincipalFactory<UserEntity>>();
			var userStore = Mock.Of<IUserStore<UserEntity>>();

			var userManager = new Mock<UserManager>(userStore, null, null, null, null, null, null, null, null).Object;
			var signInManager = new Mock<SignInManager<UserEntity>>(userManager, httpContextAccessor, userClaimsPrincipalFactory, null, null, null, null).Object;

			return await Task.FromResult(new IdentityFacade(Mock.Of<ILoggerFactory>(), signInManager, userManager));
		}

		[TestMethod]
		public async Task UserLoginModelToUserLoginEntityAsync_IfUserLoginParameterIsNull_ShouldReturnNull()
		{
			var identityFacade = await this.CreateIdentityFacadeAsync();

			var userEntity = await identityFacade.UserLoginModelToUserLoginEntityAsync(null);

			Assert.IsNull(userEntity);
		}

		[TestMethod]
		public async Task UserModelToUserEntityAsync_IfUserParameterHasANullId_ShouldReturnAUserEntityWithTheIdSet()
		{
			var userModel = new UserModel();
			Assert.IsNull(userModel.Id);

			var identityFacade = await this.CreateIdentityFacadeAsync();
			var userEntity = await identityFacade.UserModelToUserEntityAsync(userModel);
			Assert.IsNotNull(userEntity.Id);
			Assert.IsFalse(new Guid(userEntity.Id) == Guid.Empty);
		}

		[TestMethod]
		public async Task UserModelToUserEntityAsync_IfUserParameterIsNull_ShouldReturnNull()
		{
			var identityFacade = await this.CreateIdentityFacadeAsync();

			var userEntity = await identityFacade.UserModelToUserEntityAsync(null);

			Assert.IsNull(userEntity);
		}

		#endregion
	}
}