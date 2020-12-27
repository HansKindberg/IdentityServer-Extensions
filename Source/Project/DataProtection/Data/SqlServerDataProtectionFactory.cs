using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HansKindberg.IdentityServer.DataProtection.Data
{
	public class SqlServerDataProtectionFactory : IDesignTimeDbContextFactory<SqlServerDataProtection>
	{
		#region Methods

		public virtual SqlServerDataProtection CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqlServerDataProtection>();
			optionsBuilder.UseSqlServer("A value that can not be empty just to be able to create/update migrations.");

			return new SqlServerDataProtection(optionsBuilder.Options);
		}

		#endregion
	}
}