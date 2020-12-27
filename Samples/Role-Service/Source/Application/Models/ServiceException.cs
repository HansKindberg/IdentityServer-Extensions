using System;
using System.Runtime.Serialization;

namespace HansKindberg.RoleService.Models
{
	[Serializable]
	public class ServiceException : Exception
	{
		#region Constructors

		public ServiceException() { }
		public ServiceException(string message) : base(message) { }
		public ServiceException(string message, Exception innerException) : base(message, innerException) { }
		protected ServiceException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

		#endregion
	}
}