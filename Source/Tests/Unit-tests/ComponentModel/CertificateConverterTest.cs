using System;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.ComponentModel
{
	[TestClass]
	public class CertificateConverterTest
	{
		#region Fields

		private const string _certificateString = "MIIDHjCCAgagAwIBAgIQJ1+cmwSalp9JT+aSz0E88zANBgkqhkiG9w0BAQsFADAgMR4wHAYDVQQDDBVTaWduaW5nLUNlcnRpZmljYXRlLTEwIhgPMTg5OTEyMzEyMjAwMDBaGA85OTk5MTIzMTIxNTk1OFowIDEeMBwGA1UEAwwVU2lnbmluZy1DZXJ0aWZpY2F0ZS0xMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0cd/7MxwTSFzSPYevyeWi7/k26ApLMxOQTvgINq3it+qd88fvDQZ6ScQoclIiRu3jAXqILTG/uX0PQBqGs6ASNOxN6ZHV8tvLPrLaWPZ5Z/Cfl2xkX9dRPF15zY9Fn9wkxNGHJ2O2rB1Gz8f57FrrNpfp7g7cQhH/9Z41uaqDw+eQum/AapAf8sAHdLvqEUBWKlaaKPvYHAO1ekzes9/vOTDC7yYKXAgSCfPDNyVAV+nPSnTBKHH/DtrAcFcl5jnx7aBLxOxu9nsMZcxfzdPspOU5FtopBwoFUGnigIAI7e5wulqJEF0auD3NRPjSwyui0inwl6iXxnxRfrkJxIumQIDAQABo1AwTjAOBgNVHQ8BAf8EBAMCBaAwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMBMB0GA1UdDgQWBBTkCCxpz9rj5HrP5qz46oiJu3NHmTANBgkqhkiG9w0BAQsFAAOCAQEAmVaq1Bw29EH+5vYp++3gl4RtRvxp3JAbAxxZahjSk4L2h+GMZtBtWP9v2/npMxP2HTdiYULbUhobtrM4omW5JSCq9Hzu3V7oO6ZnEpJkYQyvJOL7sPjB75dEHXfkNZMTnmGXKNDokg9GaPEEh7r6OnpHA3ReoJEEUK1yXSXlW5N6J5MAm0vI/unsN8VROZLwtt47W0RLMgRCV+f2n9y16ym/diXOR0yLaqmXbTu1nBFguGLFJ8GQJLmIJ/UVwCE0+iRbZjxzXnG9hd135pQdWUXbCE7sKozrt3/RYxiIJxbWCVb8gY6Gd0669fJvhuQRrNP9KZ2tV+C8djVbD7AMTQ==";

		#endregion

		#region Properties

		protected internal virtual string CertificateString => _certificateString;

		#endregion

		#region Methods

		[TestMethod]
		public async Task BytesToString_Test()
		{
			await Task.CompletedTask;

			var bytes = Convert.FromBase64String(this.CertificateString);

			Assert.AreEqual(this.CertificateString, new CertificateConverter().BytesToString(bytes));
		}

		[TestMethod]
		public async Task StringToBytes_Test()
		{
			await Task.CompletedTask;

			var bytes = Convert.FromBase64String(this.CertificateString);

			Assert.IsTrue(bytes.SequenceEqual(new CertificateConverter().StringToBytes(this.CertificateString)));
		}

		#endregion
	}
}