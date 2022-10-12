namespace HansKindberg.IdentityServer.Security.Claims.County
{
	public class Selection
	{
		#region Properties

		public virtual Commission Commission { get; set; }
		public virtual string EmployeeHsaId { get; set; }
		public virtual string GivenName { get; set; }
		public virtual string Mail { get; set; }
		public virtual string PaTitleCode { get; set; }
		public virtual string PersonalIdentityNumber { get; set; }
		public virtual string PersonalPrescriptionCode { get; set; }
		public virtual string Surname { get; set; }
		public virtual string SystemRole { get; set; }

		#endregion
	}
}