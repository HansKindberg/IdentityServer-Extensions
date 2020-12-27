using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Helper
{
	public static class CertificateLoader
	{
		#region Fields

		public const string ClientCertificateFileName = "Test-IdentityServer-client-certificate";
		public const string ClientCertificateWithEmailAndUpnFileName = "Test-IdentityServer-client-certificate-with-email-and-UPN";
		public const string ClientCertificateWithEmailFileName = "Test-IdentityServer-client-certificate-with-email";
		public const string ClientCertificateWithUpnFileName = "Test-IdentityServer-client-certificate-with-UPN";
		public const string InvalidClientCertificateFileName = "Test-IdentityServer-invalid-client-certificate";

		#endregion

		#region Methods

		public static X509Certificate2 Get(string fileName)
		{
			var filePath = Path.Combine(GetCertificateDirectoryPath(), $"{fileName}.pfx");
			const string password = "P@ssword12";

			if(!File.Exists(filePath))
				throw new InvalidOperationException($"The certificate-file \"{filePath}\" does not exist.");

			try
			{
				return new X509Certificate2(filePath, password);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not get certificate with file-path \"{filePath}\" using password \"{password}\".", exception);
			}
		}

		private static string GetCertificateDirectoryPath()
		{
			return Path.Combine(GetSolutionDirectoryPath(), ".Global", "Certificates");
		}

		public static X509Certificate2 GetClientCertificate()
		{
			return Get(ClientCertificateFileName);
		}

		public static X509Certificate2 GetClientCertificateWithEmail()
		{
			return Get(ClientCertificateWithEmailFileName);
		}

		public static X509Certificate2 GetClientCertificateWithEmailAndUpn()
		{
			return Get(ClientCertificateWithEmailAndUpnFileName);
		}

		public static X509Certificate2 GetClientCertificateWithUpn()
		{
			return Get(ClientCertificateWithUpnFileName);
		}

		public static X509Certificate2 GetInvalidClientCertificate()
		{
			return Get(InvalidClientCertificateFileName);
		}

		private static string GetSolutionDirectoryPath()
		{
			// ReSharper disable PossibleNullReferenceException
			return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.Parent.Parent.Parent.FullName;
			// ReSharper restore PossibleNullReferenceException
		}

		public static void ValidateTrustedRootCertificate()
		{
			var filePath = Path.Combine(GetCertificateDirectoryPath(), "Test-IdentityServer-Root-CA.cer");

			if(!File.Exists(filePath))
				throw new InvalidOperationException($"The certificate-file \"{filePath}\" does not exist.");

			var certificate = new X509Certificate2(filePath);

			using(var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
			{
				store.Open(OpenFlags.ReadOnly);

				if(store.Certificates.Contains(certificate))
					return;
			}

			throw new InvalidOperationException($"You must add the certificate \"{filePath}\" to \"CERT:\\LocalMachine\\Root\".");
		}

		#endregion
	}
}