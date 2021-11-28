using System;
using System.Globalization;
using Application.Models.Views.Shared.Parts;
using HansKindberg.IdentityServer;
using Microsoft.AspNetCore.Localization;

namespace Application.Models.Views.Shared
{
	public class LayoutViewModel
	{
		#region Fields

		private HeaderViewModel _header;
		private CultureInfo _uiCulture;

		#endregion

		#region Constructors

		public LayoutViewModel(IFacade facade)
		{
			this.Facade = facade ?? throw new ArgumentNullException(nameof(facade));
		}

		#endregion

		#region Properties

		public virtual IFacade Facade { get; }
		public virtual HeaderViewModel Header => this._header ??= new HeaderViewModel(this.Facade);
		public virtual CultureInfo UiCulture => this._uiCulture ??= this.Facade.HttpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.UICulture;

		#endregion
	}
}