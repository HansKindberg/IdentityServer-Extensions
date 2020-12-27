using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HansKindberg.IdentityServer.Identity.Data
{
	public class SqlServerIdentityFactory : IDesignTimeDbContextFactory<SqlServerIdentity>
	{
		#region Methods

		public virtual SqlServerIdentity CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqlServerIdentity>();
			optionsBuilder.UseSqlServer("A value that can not be empty just to be able to create/update migrations.");

			return new SqlServerIdentity(optionsBuilder.Options);
		}

		#endregion
	}
}