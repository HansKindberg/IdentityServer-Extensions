using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace HansKindberg.IdentityServer.ComponentModel
{
	public class CertificateConverter : TypeConverter
	{
		#region Methods

		protected internal virtual string BytesToString(byte[] bytes)
		{
			return Convert.ToBase64String(bytes);
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return this.CanConvertFromInternal(sourceType) || base.CanConvertFrom(context, sourceType);
		}

		protected internal virtual bool CanConvertFromInternal(Type type)
		{
			return type == typeof(string) || type == typeof(X509Certificate2);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return this.CanConvertFromInternal(destinationType) || destinationType == typeof(InstanceDescriptor) || base.CanConvertTo(context, destinationType);
		}

		protected internal virtual bool CanConvertToInternal(Type type)
		{
			return type == typeof(X509Certificate2);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return value switch
			{
				string certificateString => this.StringToCertificate(certificateString),
				X509Certificate2 certificate => this.CopyCertificate(certificate),
				_ => base.ConvertFrom(context, culture, value)
			};
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if(destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// ReSharper disable InvertIf
			if(value is X509Certificate2 certificate)
			{
				if(destinationType == typeof(InstanceDescriptor))
				{
					var constructor = typeof(X509Certificate2).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(byte[]) }, null);

					return new InstanceDescriptor(constructor, new object[] { certificate.GetRawCertData() });
				}

				if(destinationType == typeof(string))
					return this.BytesToString(certificate.GetRawCertData());

				if(destinationType == typeof(X509Certificate2))
					return new X509Certificate2(certificate.GetRawCertData());
			}
			// ReSharper restore InvertIf

			return base.ConvertTo(context, culture, value, destinationType);
		}

		protected internal virtual X509Certificate2 CopyCertificate(X509Certificate2 certificate)
		{
			if(certificate == null)
				throw new ArgumentNullException(nameof(certificate));

			return new X509Certificate2(certificate.GetRawCertData());
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// ReSharper disable InvertIf
			if(value is string certificateString)
			{
				try
				{
					this.StringToCertificate(certificateString);
				}
				catch
				{
					return false;
				}
			}
			// ReSharper restore InvertIf

			return value is X509Certificate2;
		}

		protected internal virtual byte[] StringToBytes(string value)
		{
			return Convert.FromBase64String(value);
		}

		protected internal virtual X509Certificate2 StringToCertificate(string value)
		{
			try
			{
				return new X509Certificate2(Convert.FromBase64String(value));
			}
			catch(Exception exception)
			{
				throw new FormatException($"Could not convert from \"{typeof(string)}\" to \"{typeof(X509Certificate2)}\".", exception);
			}
		}

		#endregion
	}
}