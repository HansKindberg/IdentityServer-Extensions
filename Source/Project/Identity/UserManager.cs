using System;
using System.Collections.Generic;
using HansKindberg.IdentityServer.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HansKindberg.IdentityServer.Identity
{
	public class UserManager : UserManager<User>
	{
		#region Constructors

		public UserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) { }

		#endregion

		#region Properties

		public virtual IdentityContext DatabaseContext => this.Store.Context;
		public new virtual UserStore Store => (UserStore)base.Store;

		#endregion
	}
}