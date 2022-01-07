using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Web.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Web.Http.Extensions
{
	[TestClass]
	public class QueryCollectionExtensionTest
	{
		#region Methods

		[TestMethod]
		public async Task ToDictionary_Test()
		{
			await Task.CompletedTask;

			var initialDictionary = QueryHelpers.ParseQuery("?key-5=a&Key-1=b&Key-5=b&key-1=f&Key-2=a&key-1=a");

			var queryCollection = new QueryCollection(initialDictionary);

			var dictionary = queryCollection.ToDictionary();

			Assert.AreEqual(3, dictionary.Count);
			Assert.AreEqual("key-5", dictionary.ElementAt(0).Key);
			Assert.AreEqual("a,b", dictionary.ElementAt(0).Value);
			Assert.AreEqual("Key-1", dictionary.ElementAt(1).Key);
			Assert.AreEqual("b,f,a", dictionary.ElementAt(1).Value);
			Assert.AreEqual("Key-2", dictionary.ElementAt(2).Key);
			Assert.AreEqual("a", dictionary.ElementAt(2).Value);
		}

		[TestMethod]
		public async Task ToSortedDictionary_Test()
		{
			await Task.CompletedTask;

			var initialDictionary = QueryHelpers.ParseQuery("?key-5=a&Key-1=b&Key-5=b&key-1=f&Key-2=a&key-1=a");

			var queryCollection = new QueryCollection(initialDictionary);

			var dictionary = queryCollection.ToSortedDictionary();

			Assert.AreEqual(3, dictionary.Count);
			Assert.AreEqual("Key-1", dictionary.ElementAt(0).Key);
			Assert.AreEqual("b,f,a", dictionary.ElementAt(0).Value);
			Assert.AreEqual("Key-2", dictionary.ElementAt(1).Key);
			Assert.AreEqual("a", dictionary.ElementAt(1).Value);
			Assert.AreEqual("key-5", dictionary.ElementAt(2).Key);
			Assert.AreEqual("a,b", dictionary.ElementAt(2).Value);
		}

		#endregion
	}
}