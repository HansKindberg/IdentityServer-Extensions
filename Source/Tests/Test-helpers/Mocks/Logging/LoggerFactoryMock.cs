﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace TestHelpers.Mocks.Logging
{
	public class LoggerFactoryMock : ILoggerFactory
	{
		#region Constructors

		public LoggerFactoryMock(LoggerFactory internalLoggerFactory)
		{
			this.InternalLoggerFactory = internalLoggerFactory ?? throw new ArgumentNullException(nameof(internalLoggerFactory));
		}

		#endregion

		#region Properties

		public virtual LogLevelEnabledMode EnabledMode { get; set; }
		protected virtual LoggerFactory InternalLoggerFactory { get; }
		public virtual IEnumerable<LogMock> Logs => this.LogsInternal.ToArray();
		protected virtual IList<LogMock> LogsInternal { get; } = new List<LogMock>();

		#endregion

		#region Methods

		public virtual void AddProvider(ILoggerProvider provider)
		{
			this.InternalLoggerFactory.AddProvider(provider);
		}

		public static LoggerFactoryMock Create()
		{
			return new LoggerFactoryMock(new LoggerFactory(Enumerable.Empty<ILoggerProvider>()))
			{
				EnabledMode = LogLevelEnabledMode.Enabled
			};
		}

		public virtual ILogger CreateLogger(string categoryName)
		{
			return new LoggerMock(this.InternalLoggerFactory.CreateLogger(categoryName), this.LogsInternal)
			{
				EnabledMode = this.EnabledMode
			};
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(disposing)
				this.InternalLoggerFactory.Dispose();
		}

		#endregion
	}
}