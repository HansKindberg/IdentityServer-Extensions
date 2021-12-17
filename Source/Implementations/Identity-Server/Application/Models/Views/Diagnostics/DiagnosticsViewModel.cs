using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace HansKindberg.IdentityServer.Application.Models.Views.Diagnostics
{
	public class DiagnosticsViewModel
	{
		#region Properties

		public virtual X509Certificate2 ClientCertificate { get; set; }
		public virtual string ConnectionId { get; set; }
		public virtual IPAddress LocalIpAddress { get; set; }
		public virtual int LocalPort { get; set; }
		public virtual string MachineName { get; set; }
		public virtual IPAddress RemoteIpAddress { get; set; }
		public virtual int RemotePort { get; set; }

		#endregion
	}
}