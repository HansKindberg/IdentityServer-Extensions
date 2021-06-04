using System;
using Duende.IdentityServer.Models;

namespace Application.Models.Views.Error
{
	public class ErrorViewModel
	{
		#region Properties

		public virtual string ActivityId { get; set; }
		public virtual bool Detailed { get; set; }
		public virtual Exception Exception { get; set; }
		public virtual ErrorMessage IdentityServerError { get; set; }
		public virtual string Path { get; set; }
		public virtual string TraceIdentifier { get; set; }

		#endregion
	}
}