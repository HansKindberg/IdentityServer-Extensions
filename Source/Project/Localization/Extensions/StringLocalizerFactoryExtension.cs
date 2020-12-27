using System;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace HansKindberg.IdentityServer.Localization.Extensions
{
	public static class StringLocalizerFactoryExtension
	{
		#region Fields

		private const char _namespaceSeparator = '.';

		#endregion

		#region Methods

		public static IStringLocalizer CreateFromType(this IStringLocalizerFactory stringLocalizerFactory, Type type)
		{
			if(stringLocalizerFactory == null)
				throw new ArgumentNullException(nameof(stringLocalizerFactory));

			if(type == null)
				throw new ArgumentNullException(nameof(type));

			var assemblyName = type.Assembly.GetName().Name ?? throw new ArgumentException("The type-assembly can not be null.", nameof(type));
			var typeFullName = type.FullName ?? throw new ArgumentException("The type-full-name can not be null.", nameof(type));

			return stringLocalizerFactory.Create(typeFullName, string.Join(_namespaceSeparator.ToString(CultureInfo.InvariantCulture), typeFullName.Split(_namespaceSeparator).Take(assemblyName.Split(_namespaceSeparator).Length)));
		}

		#endregion
	}
}