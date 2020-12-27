using System.Threading.Tasks;
using HansKindberg.IdentityServer.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Security.Cryptography.Configuration;

namespace UnitTests.Configuration
{
	[TestClass]
	public class ExtendedIdentityServerOptionsTest
	{
		#region Methods

		[TestMethod]
		public async Task SigningCertificate_ShouldHaveADefaultValue()
		{
			await Task.CompletedTask;

			var signingCertificate = new ExtendedIdentityServerOptions().SigningCertificate;

			Assert.IsNotNull(signingCertificate);
			Assert.AreEqual("Options", signingCertificate.Options.Key);
			Assert.AreEqual("IdentityServer:SigningCertificate:Options", signingCertificate.Options.Path);
			Assert.AreEqual(@"CERT:\LocalMachine\My\CN=Identity-Server-Signing", signingCertificate.Options.GetSection("Path").Value);
			Assert.AreEqual(typeof(StoreResolverOptions).AssemblyQualifiedName, signingCertificate.Type);
		}

		#endregion
	}
}