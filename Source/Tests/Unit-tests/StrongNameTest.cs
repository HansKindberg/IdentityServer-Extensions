using System;
using System.Threading.Tasks;
using HansKindberg.IdentityServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
	[TestClass]
	public class StrongNameTest
	{
		#region Methods

		[TestMethod]
		public async Task Assembly_FullName_ShouldEndWithAPublicKeyToken()
		{
			await Task.CompletedTask;
			var assemblyFullName = typeof(IFacade).Assembly.FullName;
			// ReSharper disable PossibleNullReferenceException
			Assert.IsTrue(assemblyFullName.EndsWith(", PublicKeyToken=b4d51b5d6b50fc48", StringComparison.Ordinal));
			// ReSharper restore PossibleNullReferenceException
		}

		#endregion
	}
}