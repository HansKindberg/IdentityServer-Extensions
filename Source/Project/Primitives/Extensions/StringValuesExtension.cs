using System.Linq;
using Microsoft.Extensions.Primitives;

namespace HansKindberg.IdentityServer.Primitives.Extensions
{
	public static class StringValuesExtension
	{
		#region Methods

		public static bool IsTrue(this StringValues stringValues)
		{
			return bool.TryParse(stringValues.FirstOrDefault(), out var value) && value;
		}

		#endregion
	}
}