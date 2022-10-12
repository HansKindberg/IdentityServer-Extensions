using System.Security.Claims;
using IdentityModel;
using RegionOrebroLan.Security.Claims;

namespace TestHelpers.Security.Claims
{
	public class ClaimsIdentityBuilder
	{
		#region Properties

		public virtual string AuthenticationType { get; set; } = "Test";
		public virtual ClaimBuilderCollection Claims { get; } = new();
		public virtual string NameType { get; set; } = JwtClaimTypes.Name;
		public virtual string RoleType { get; set; } = JwtClaimTypes.Role;

		#endregion

		#region Methods

		public virtual ClaimsIdentity Build()
		{
			return new ClaimsIdentity(this.Claims.Build(), this.AuthenticationType, this.NameType, this.RoleType);
		}

		#endregion
	}
}