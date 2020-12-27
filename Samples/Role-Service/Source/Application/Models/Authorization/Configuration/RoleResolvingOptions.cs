namespace HansKindberg.RoleService.Models.Authorization.Configuration
{
	public class RoleResolvingOptions
	{
		#region Properties

		protected internal virtual string ClientIdKey { get; set; } = "client_id";
		public virtual bool MachineRolesEnabled { get; set; }

		#endregion
	}
}