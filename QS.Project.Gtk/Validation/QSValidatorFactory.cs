using Microsoft.Extensions.Logging;
using System;

namespace QS.Validation {
	public class QSValidatorFactory<T> : IQSValidatorFactory<T> where T : class {
		private readonly ILogger<QSValidatorFactory<T>> _logger;

		public QSValidatorFactory(ILogger<QSValidatorFactory<T>> logger) {
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public IQSValidator<T> CreateForInstance(T instance) {
			if(instance is null) {
				var exception = new ArgumentNullException(nameof(instance));

				_logger.LogError(exception, "Unable to create new {ValidatorClass} for null instance of argument: {Argument}", nameof(QSValidator<T>), nameof(instance));

				throw exception;
			}

			return new QSValidator<T>(instance);
		}
	}
}
