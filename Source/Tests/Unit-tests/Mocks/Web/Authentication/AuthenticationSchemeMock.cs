using System;
using RegionOrebroLan.Web.Authentication;

namespace UnitTests.Mocks.Web.Authentication
{
	public class AuthenticationSchemeMock : IAuthenticationScheme
	{
		#region Properties

		public virtual string DisplayName { get; set; }
		public virtual bool Enabled { get; set; }
		public virtual Type HandlerType { get; set; }
		public virtual string Icon { get; set; }
		public virtual int Index { get; set; }
		public virtual bool Interactive { get; set; }
		public virtual AuthenticationSchemeKind Kind { get; set; }
		public virtual string Name { get; set; }
		public virtual bool SignOutSupport { get; set; }

		#endregion
	}
}