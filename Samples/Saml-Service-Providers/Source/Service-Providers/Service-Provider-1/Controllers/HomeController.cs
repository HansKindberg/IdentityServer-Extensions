using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Application.Controllers
{
	public class HomeController : SiteController
	{
		#region Constructors

		public HomeController(ILoggerFactory loggerFactory) : base(loggerFactory) { }

		#endregion

		#region Methods

		public virtual async Task<IActionResult> Index()
		{
			return await Task.FromResult(this.View());
		}

		#endregion
	}
}