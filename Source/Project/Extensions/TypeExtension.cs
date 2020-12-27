using System;
using System.Linq;

namespace HansKindberg.IdentityServer.Extensions
{
	public static class TypeExtension
	{
		#region Methods

		/// <summary>
		/// From https://stackoverflow.com/questions/16466380/get-user-friendly-name-for-generic-type-in-c-sharp/#answer-16466437
		/// </summary>
		public static string FriendlyName(this Type type)
		{
			if(type == null)
				throw new ArgumentNullException(nameof(type));

			if(type == typeof(bool))
				return "bool";

			if(type == typeof(byte))
				return "byte";

			if(type == typeof(decimal))
				return "decimal";

			if(type == typeof(double))
				return "double";

			if(type == typeof(float))
				return "float";

			if(type == typeof(int))
				return "int";

			if(type == typeof(long))
				return "long";

			if(type == typeof(short))
				return "short";

			if(type == typeof(string))
				return "string";

			if(type.IsGenericType)
				return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(genericArgument => genericArgument.FriendlyName()).ToArray()) + ">";

			return type.Name;
		}

		#endregion
	}
}