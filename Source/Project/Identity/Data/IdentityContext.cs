using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Identity.Data
{
	public abstract class IdentityContext : IdentityDbContext<User, Role, string>
	{
		#region Constructors

		protected IdentityContext(DbContextOptions options) : base(options) { }

		#endregion
	}

	public abstract class IdentityContext<T> : IdentityContext where T : IdentityContext
	{
		#region Constructors

		protected IdentityContext(DbContextOptions<T> options) : base(options) { }

		#endregion
	}
}