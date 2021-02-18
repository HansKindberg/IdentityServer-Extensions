using System;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity;
using IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Identity
{
	[TestClass]
	public class UserStoreTest
	{
		#region Methods

		[TestMethod]
		public async Task AutoSaveChanges_ShouldReturnFalseByDefault()
		{
			using(var context = new Context())
			{
				var userStore = await this.CreateUserStoreAsync(context.ServiceProvider);

				Assert.IsFalse(userStore.AutoSaveChanges);
			}
		}

		protected internal virtual async Task<UserStore> CreateUserStoreAsync(IServiceProvider serviceProvider)
		{
			return await Task.FromResult(serviceProvider.GetRequiredService<UserManager>().Store);
		}

		#endregion
	}
}