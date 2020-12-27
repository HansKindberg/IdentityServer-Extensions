namespace HansKindberg.IdentityServer.Data.Transferring
{
	public class ImportOptions
	{
		#region Properties

		/// <summary>
		/// Delete all entities that are not in the configuration.
		/// </summary>
		public virtual bool DeleteAllOthers { get; set; }

		/// <summary>
		/// Do not import, verify only.
		/// </summary>
		public virtual bool VerifyOnly { get; set; }

		#endregion
	}
}