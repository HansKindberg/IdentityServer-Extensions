using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Web.Mvc.Filters.Configuration
{
	public class SecurityHeaderOptions
	{
		#region Fields

		private static readonly ISet<string> _defaultPaths = new HashSet<string>(new[] { "/BankIdAuthentication/Login" }, StringComparer.OrdinalIgnoreCase);

		#endregion

		#region Properties

		public virtual ISet<string> AllowDataImagePaths { get; } = new HashSet<string>(_defaultPaths, StringComparer.OrdinalIgnoreCase);
		public virtual ISet<string> AllowInlineImagePaths { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		public virtual ISet<string> AllowInlineScriptPaths { get; } = new HashSet<string>(_defaultPaths, StringComparer.OrdinalIgnoreCase);
		public virtual ISet<string> AllowInlineStylePaths { get; } = new HashSet<string>(_defaultPaths, StringComparer.OrdinalIgnoreCase);

		#endregion
	}
}