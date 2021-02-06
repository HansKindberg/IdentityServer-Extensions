using System.Diagnostics.CodeAnalysis;

namespace HansKindberg.IdentityServer.Configuration
{
	/// <summary>
	/// Global application-options.
	/// </summary>
	[SuppressMessage("Naming", "CA1724:Type names should not match namespaces")]
	public class Options
	{
		#region Properties

		/// <summary>
		/// If the application is enabled or not.
		/// </summary>
		public virtual bool Enabled { get; set; }

		#endregion
	}
}