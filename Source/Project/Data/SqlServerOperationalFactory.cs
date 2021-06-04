using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HansKindberg.IdentityServer.Data
{
	public class SqlServerOperationalFactory : IDesignTimeDbContextFactory<SqlServerOperational>
	{
		#region Methods

		public virtual SqlServerOperational CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqlServerOperational>();
			optionsBuilder.UseSqlServer("A value that can not be empty just to be able to create/update migrations.");

			return new SqlServerOperational(optionsBuilder.Options, new OperationalStoreOptions());
		}

		#endregion
	}
}