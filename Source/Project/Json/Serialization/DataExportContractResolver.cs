using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using IdentityServer4.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rsk.Saml.Models;
using Rsk.WsFederation.Models;

namespace HansKindberg.IdentityServer.Json.Serialization
{
	/// <summary>
	/// Contract-resolver to only serialize non default values.
	/// </summary>
	public class DataExportContractResolver : ContractResolver
	{
		#region Fields

		private static IDictionary<Tuple<Type, string>, object> _defaultValues;
		private static readonly object _defaultValuesLock = new object();

		#endregion

		#region Properties

		[SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code")]
		protected internal virtual IDictionary<Tuple<Type, string>, object> DefaultValues
		{
			get
			{
				// ReSharper disable InvertIf
				if(_defaultValues == null)
				{
					lock(this.DefaultValuesLock)
					{
						if(_defaultValues == null)
						{
							var defaultValues = new Dictionary<Tuple<Type, string>, object>();

							this.PopulateDefaultValues<ApiResource>(defaultValues);
							this.PopulateDefaultValues<ApiScope>(defaultValues);
							this.PopulateDefaultValues<Client>(defaultValues);
							this.PopulateDefaultValues<ClientClaim>(defaultValues);
							this.PopulateDefaultValues<IdentityResource>(defaultValues);
							this.PopulateDefaultValues<RelyingParty>(defaultValues);
							this.PopulateDefaultValues<Secret>(defaultValues);
							this.PopulateDefaultValues<Service>(defaultValues);
							this.PopulateDefaultValues<ServiceProvider>(defaultValues);

							_defaultValues = defaultValues;
						}
					}
				}
				// ReSharper restore InvertIf

				return _defaultValues;
			}
		}

		protected internal virtual object DefaultValuesLock => _defaultValuesLock;

		#endregion

		#region Methods

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = this.CreatePropertyInternal(member, memberSerialization);

			if(property.PropertyType != typeof(string) && property.PropertyType.GetInterface(nameof(IEnumerable)) != null)
			{
				property.ShouldSerialize = instance => (instance?.GetType().GetProperty(property.PropertyName)?.GetValue(instance) as IEnumerable<object>)?.Count() > 0;
			}
			else if(this.DefaultValues.Keys.Select(key => key.Item1).Distinct().Any(type => property.DeclaringType.IsAssignableFrom(type)))
			{
				property.ShouldSerialize = instance =>
				{
					if(!this.TryGetDefaultValue(member?.Name, instance?.GetType(), out var defaultValue))
						return true;

					var value = instance?.GetType().GetProperty(property.PropertyName)?.GetValue(instance);

					return !Equals(value, defaultValue);
				};
			}

			return property;
		}

		protected internal virtual void PopulateDefaultValues<T>(IDictionary<Tuple<Type, string>, object> defaultValues) where T : new()
		{
			if(defaultValues == null)
				throw new ArgumentNullException(nameof(defaultValues));

			var instance = new T();
			var type = instance.GetType();

			foreach(var property in type.GetProperties())
			{
				var value = property.GetValue(instance);
				defaultValues.Add(new Tuple<Type, string>(type, property.Name), value);
			}
		}

		protected internal virtual bool TryGetDefaultValue(string propertyName, Type type, out object value)
		{
			value = null;

			if(propertyName == null || type == null)
				return false;

			return this.DefaultValues.TryGetValue(new Tuple<Type, string>(type, propertyName), out value);
		}

		#endregion
	}
}