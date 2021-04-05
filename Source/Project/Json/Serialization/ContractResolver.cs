using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HansKindberg.IdentityServer.Json.Serialization
{
	public class ContractResolver : DefaultContractResolver
	{
		#region Methods

		/// <summary>
		/// Sorts the properties in alphabetical order.
		/// </summary>
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			return base.CreateProperties(type, memberSerialization).OrderBy(jsonProperty => jsonProperty.PropertyName).ToList();
		}

		/// <summary>
		/// Remove empty arrays.
		/// </summary>
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = this.CreatePropertyInternal(member, memberSerialization);

			if(property.PropertyType != typeof(string) && property.PropertyType.GetInterface(nameof(IEnumerable)) != null)
				property.ShouldSerialize = instance =>
				{
					if(instance?.GetType().GetProperty(property.PropertyName)?.GetValue(instance) is IEnumerable propertyValue)
						return propertyValue.Cast<object>().Any();

					return false;
				};

			return property;
		}

		/// <summary>
		/// Creates a property by calling the base class.
		/// </summary>
		protected internal virtual JsonProperty CreatePropertyInternal(MemberInfo member, MemberSerialization memberSerialization)
		{
			return base.CreateProperty(member, memberSerialization);
		}

		#endregion
	}
}