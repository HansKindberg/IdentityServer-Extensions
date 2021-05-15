using System;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.EntityFramework.Extensions
{
	public static class ModelBuilderExtension
	{
		#region Methods

		public static void SqliteCaseInsensitive(this ModelBuilder modelBuilder)
		{
			if(modelBuilder == null)
				throw new ArgumentNullException(nameof(modelBuilder));

			foreach(var entityType in modelBuilder.Model.GetEntityTypes())
			{
				foreach(var property in entityType.GetProperties())
				{
					if(property.ClrType == typeof(string))
						property.SetCollation("NOCASE");
				}
			}
		}

		#endregion
	}
}