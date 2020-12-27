using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
	[TestClass]
	public class Test
	{
		#region Methods

		[TestMethod]
		public async Task Test_1()
		{
			using(var webApplicationFactory = new WebApplicationFactory())
			{
				using(var httpClient = webApplicationFactory.CreateClient())
				{
					var result = await httpClient.GetStringAsync(string.Empty);

					Assert.IsNotNull(result);
				}
			}
		}

		#endregion
	}
}