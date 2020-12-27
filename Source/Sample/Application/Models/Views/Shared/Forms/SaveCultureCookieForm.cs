namespace Application.Models.Views.Shared.Forms
{
	public class SaveCultureCookieForm : ReturnForm
	{
		#region Properties

		public virtual string Culture { get; set; }
		public virtual string UiCulture { get; set; }

		#endregion
	}
}