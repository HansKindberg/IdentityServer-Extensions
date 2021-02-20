using System;
using System.Threading.Tasks;
using HansKindberg.RoleService.Controllers;
using HansKindberg.RoleService.Models;
using HansKindberg.RoleService.Models.Configuration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTests.Controllers
{
	[TestClass]
	public class ErrorControllerTest
	{
		#region Methods

		protected internal virtual async Task<ErrorController> CreateErrorControllerAsync(ExceptionHandlingOptions exceptionHandlingOptions, HttpContext httpContext, ILoggerFactory loggerFactory)
		{
			var exceptionHandlingOptionsMonitorMock = new Mock<IOptionsMonitor<ExceptionHandlingOptions>>();
			exceptionHandlingOptionsMonitorMock.Setup(exceptionHandlingOptionsMonitor => exceptionHandlingOptionsMonitor.CurrentValue).Returns(exceptionHandlingOptions);

			var errorController = new ErrorController(loggerFactory, await this.CreateExceptionHandlingOptionsMonitorAsync(exceptionHandlingOptions))
			{
				ControllerContext = new ControllerContext()
				{
					HttpContext = httpContext
				}
			};

			return await Task.FromResult(errorController);
		}

		protected internal virtual async Task<ErrorController> CreateErrorControllerAsync(ExceptionHandlingOptions exceptionHandlingOptions, IExceptionHandlerFeature exceptionHandlerFeature, ILoggerFactory loggerFactory)
		{
			var features = new FeatureCollection();
			features.Set(exceptionHandlerFeature);

			return await this.CreateErrorControllerAsync(exceptionHandlingOptions, new DefaultHttpContext(features), loggerFactory);
		}

		protected internal virtual async Task<ErrorController> CreateErrorControllerAsync(ExceptionHandlingOptions exceptionHandlingOptions, ILoggerFactory loggerFactory, Exception exception = null)
		{
			return await this.CreateErrorControllerAsync(exceptionHandlingOptions, await this.CreateExceptionHandlerFeatureAsync(exception), loggerFactory);
		}

		protected internal virtual async Task<IExceptionHandlerFeature> CreateExceptionHandlerFeatureAsync(Exception exception = null)
		{
			var exceptionHandlerFeatureMock = new Mock<IExceptionHandlerFeature>();
			exceptionHandlerFeatureMock.Setup(exceptionHandlerFeature => exceptionHandlerFeature.Error).Returns(exception);

			return await Task.FromResult(exceptionHandlerFeatureMock.Object);
		}

		protected internal virtual async Task<IOptionsMonitor<ExceptionHandlingOptions>> CreateExceptionHandlingOptionsMonitorAsync(ExceptionHandlingOptions exceptionHandlingOptions)
		{
			var exceptionHandlingOptionsMonitorMock = new Mock<IOptionsMonitor<ExceptionHandlingOptions>>();
			exceptionHandlingOptionsMonitorMock.Setup(exceptionHandlingOptionsMonitor => exceptionHandlingOptionsMonitor.CurrentValue).Returns(exceptionHandlingOptions);

			return await Task.FromResult(exceptionHandlingOptionsMonitorMock.Object);
		}

		[TestMethod]
		public async Task Index_Test()
		{
			using(var loggerFactory = new NullLoggerFactory())
			{
				var exceptionHandlingOptions = new ExceptionHandlingOptions();
				var errorController = await this.CreateErrorControllerAsync(exceptionHandlingOptions, loggerFactory);
				var actionResult = await errorController.Index();
				var objectResult = (ObjectResult)actionResult;
				var problemDetails = (ProblemDetails)objectResult.Value;
				Assert.IsNotNull(problemDetails);
				Assert.IsNull(problemDetails.Detail);
				Assert.AreEqual("Error", problemDetails.Title);

				exceptionHandlingOptions = new ExceptionHandlingOptions {Detailed = true};
				errorController = await this.CreateErrorControllerAsync(exceptionHandlingOptions, loggerFactory);
				actionResult = await errorController.Index();
				objectResult = (ObjectResult)actionResult;
				problemDetails = (ProblemDetails)objectResult.Value;
				Assert.IsNotNull(problemDetails);
				Assert.IsNull(problemDetails.Detail);
				Assert.AreEqual("Error", problemDetails.Title);

				exceptionHandlingOptions = new ExceptionHandlingOptions();
				errorController = await this.CreateErrorControllerAsync(exceptionHandlingOptions, loggerFactory, new InvalidOperationException("Test-exception"));
				actionResult = await errorController.Index();
				objectResult = (ObjectResult)actionResult;
				problemDetails = (ProblemDetails)objectResult.Value;
				Assert.IsNotNull(problemDetails);
				Assert.IsNull(problemDetails.Detail);
				Assert.AreEqual("Error", problemDetails.Title);

				exceptionHandlingOptions = new ExceptionHandlingOptions {Detailed = true};
				errorController = await this.CreateErrorControllerAsync(exceptionHandlingOptions, loggerFactory, new InvalidOperationException("Test-exception"));
				actionResult = await errorController.Index();
				objectResult = (ObjectResult)actionResult;
				problemDetails = (ProblemDetails)objectResult.Value;
				Assert.IsNotNull(problemDetails);
				Assert.AreEqual("System.InvalidOperationException: Test-exception", problemDetails.Detail);
				Assert.AreEqual("Error", problemDetails.Title);

				exceptionHandlingOptions = new ExceptionHandlingOptions();
				errorController = await this.CreateErrorControllerAsync(exceptionHandlingOptions, loggerFactory, new ServiceException("Test-exception"));
				actionResult = await errorController.Index();
				objectResult = (ObjectResult)actionResult;
				problemDetails = (ProblemDetails)objectResult.Value;
				Assert.IsNotNull(problemDetails);
				Assert.IsNull(problemDetails.Detail);
				Assert.AreEqual("Test-exception", problemDetails.Title);

				exceptionHandlingOptions = new ExceptionHandlingOptions {Detailed = true};
				errorController = await this.CreateErrorControllerAsync(exceptionHandlingOptions, loggerFactory, new ServiceException("Test-exception"));
				actionResult = await errorController.Index();
				objectResult = (ObjectResult)actionResult;
				problemDetails = (ProblemDetails)objectResult.Value;
				Assert.IsNotNull(problemDetails);
				Assert.AreEqual("HansKindberg.RoleService.Models.ServiceException: Test-exception", problemDetails.Detail);
				Assert.AreEqual("Test-exception", problemDetails.Title);
			}
		}

		#endregion
	}
}