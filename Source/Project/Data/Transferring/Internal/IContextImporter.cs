using System.Threading.Tasks;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public interface IContextImporter : IPartialImporter
	{
		#region Methods

		Task<int> CommitAsync();

		#endregion
	}
}