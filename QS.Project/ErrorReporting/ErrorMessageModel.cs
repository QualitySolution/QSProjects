﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;

namespace QS.ErrorReporting
{
	public class ErrorMessageModel : ErrorMessageModelBase
	{
		public ErrorMessageModel(
			IErrorReporter errorReporter,
			IUserService userService,
			IUnitOfWorkFactory unitOfWorkFactory) : base(errorReporter)
		{
			this.userService = userService;
			this.unitOfWorkFactory = unitOfWorkFactory;
		}

		IUserService userService;
		IUnitOfWorkFactory unitOfWorkFactory;

		public override bool CanSendErrorReportManually => !String.IsNullOrWhiteSpace(Description);
		public override bool CanSendErrorReportAutomatically => errorReporter.CanSendAutomatically;
		public override bool IsEmailValid => new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$").IsMatch(Email ?? "");

		public override string ExceptionText =>
			string.Join("\n Следующее исключение:\n", Exceptions.Select(ex => ex.ToString()));

		public override string ErrorData =>
			$"Продукт: {errorReporter.ProductName}\n" +
			$"Версия: {errorReporter.Version}\n" +
			$"Редакция: {errorReporter.Edition}\n" +
			$"Ошибка: {string.Join("\n Следующее исключение:\n", Exceptions.Select(ex => ex.ToString()))}";

		public override void SendErrorReport()
		{
			if(ReportSent)
				return;
			if(!CanSendErrorReportManually && ErrorReportType != ErrorReportType.Automatic)
				return;
			if(!CanSendErrorReportAutomatically && ErrorReportType == ErrorReportType.Automatic)
				return;

			UserBase user = null;
			try {
				if(userService != null && unitOfWorkFactory != null) {
					using(IUnitOfWork uow = unitOfWorkFactory.CreateWithoutRoot()) {
						user = userService.GetCurrentUser(uow);
					}
				}
			} catch(Exception ex) { 
				AddDescription($"Не удалось автоматически получить пользователя ({ex.Message})"); 
			}

			ReportSent = errorReporter.SendErrorReport(
				Exceptions.ToArray(),
				ErrorReportType,
				Description,
				Email,
				user
			);
		}
	}
}
