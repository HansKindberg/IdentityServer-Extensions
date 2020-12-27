namespace Application.Models.Views.Shared
{
	public class RedirectViewModel
	{
		#region Properties

		public virtual string RedirectUrl { get; set; }
		public virtual byte SecondsBeforeRedirect { get; set; }

		#endregion
	}
}