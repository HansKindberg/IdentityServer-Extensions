using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TestHelpers.Mocks.Logging;

namespace UnitTests
{
	[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
	public static class Global
	{
		#region Methods

		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Should be disposed by the caller.")]
		public static LoggerFactoryMock CreateLoggerFactoryMock()
		{
			return CreateLoggerFactoryMock(new LoggerFactory());
		}

		public static LoggerFactoryMock CreateLoggerFactoryMock(LoggerFactory loggerFactory)
		{
			if(loggerFactory == null)
				throw new ArgumentNullException(nameof(loggerFactory));

			return new LoggerFactoryMock(loggerFactory);
		}

		#endregion
	}
}