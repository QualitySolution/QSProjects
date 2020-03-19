﻿using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace QS.ErrorReporting
{
	public abstract class ErrorMessageModelBase : PropertyChangedBase
	{
		public ErrorMessageModelBase(IErrorReporter errorReporter)
		{
			this.errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
			Exceptions = new List<Exception>();
		}

		protected IErrorReporter errorReporter;

		public abstract bool CanSendErrorReportManually { get; }
		public abstract bool CanSendErrorReportAutomatically { get; }
		public abstract string ExceptionText { get; }
		public abstract string ErrorData { get; }
		public abstract bool SendErrorReport();

		public virtual bool IsEmailValid { get; } = true;

		private string email;
		public virtual string Email {
			get => email;
			set { SetField(ref email, value, () => Email); }
		}

		private string description;
		public virtual string Description {
			get => description;
			set { SetField(ref description, value, () => Description); }
		}

		private IList<Exception> exceptions;
		public virtual IList<Exception> Exceptions {
			get => exceptions;
			set { SetField(ref exceptions, value, () => Exceptions); }
		}

		private ErrorReportType errorReportType;
		public virtual ErrorReportType ErrorReportType {
			get => errorReportType;
			set { SetField(ref errorReportType, value, () => ErrorReportType); }
		}
	}
}
