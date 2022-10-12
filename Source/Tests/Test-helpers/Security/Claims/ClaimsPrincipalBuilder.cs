using System.Security.Claims;
using RegionOrebroLan.Security.Claims;

namespace TestHelpers.Security.Claims
{
	public class ClaimsPrincipalBuilder
	{
		#region Properties

		public virtual string AuthenticationType
		{
			get => this.ClaimsIdentityBuilder.AuthenticationType;
			set => this.ClaimsIdentityBuilder.AuthenticationType = value;
		}

		public virtual ClaimBuilderCollection Claims => this.ClaimsIdentityBuilder.Claims;
		protected internal virtual ClaimsIdentityBuilder ClaimsIdentityBuilder { get; } = new();

		public virtual string NameType
		{
			get => this.ClaimsIdentityBuilder.NameType;
			set => this.ClaimsIdentityBuilder.NameType = value;
		}

		public virtual string RoleType
		{
			get => this.ClaimsIdentityBuilder.RoleType;
			set => this.ClaimsIdentityBuilder.RoleType = value;
		}

		#endregion

		#region Methods

		public virtual ClaimsPrincipal Build()
		{
			return new ClaimsPrincipal(this.ClaimsIdentityBuilder.Build());
		}

		#endregion
	}
}