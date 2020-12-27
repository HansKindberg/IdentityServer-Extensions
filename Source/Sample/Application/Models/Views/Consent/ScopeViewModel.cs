namespace Application.Models.Views.Consent
{
	public class ScopeViewModel
	{
		#region Properties

		public virtual bool Checked { get; set; } = true;
		public virtual string Description { get; set; }
		public virtual string DisplayName { get; set; }
		public virtual bool Emphasize { get; set; }
		public virtual string Name { get; set; }
		public virtual bool Required { get; set; }

		#endregion
	}
}