using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Helpers
{
	public class EmptyDatabaseContext : DbContext
	{
		#region Constructors

		public EmptyDatabaseContext(DbContextOptions<EmptyDatabaseContext> options) : base(options) { }

		#endregion
	}
}