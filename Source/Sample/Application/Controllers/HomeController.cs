using System.Threading.Tasks;
using HansKindberg.IdentityServer;
using HansKindberg.IdentityServer.FeatureManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Application.Controllers
{
	[FeatureGate(Feature.Home)]
	public class HomeController : SiteController
	{
		#region Constructors

		public HomeController(IFacade facade) : base(facade) { }

		#endregion

		#region Methods

		public virtual async Task<IActionResult> Index()
		{
			return await Task.FromResult(this.View());
		}

		#endregion
	}
}