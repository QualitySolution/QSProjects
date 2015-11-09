using System;
using System.Data.Bindings;

namespace QSBanks
{
	public class TransferDocument
	{
		public TransferDocumentType DocumentType;
		public string Number;
		public DateTime Date;
		public decimal Total;

		#region Плательщик

		public string PayerAccount;
		public string PayerCheckingAccount;
		public DateTime? WriteoffDate;
		public string PayerInn;
		public string PayerName;
		public string PayerKpp;
		public string PayerBank;
		public string PayerBik;
		public string PayerCorrespondentAccount;

		#endregion

		#region Получатель

		public string RecipientAccount;
		public string RecipientCheckingAccount;
		public DateTime? ReceiptDate;
		public string RecipientInn;
		public string RecipientName;
		public string RecipientKpp;
		public string RecipientBank;
		public string RecipientBik;
		public string RecipientCorrespondentAccount;
		public string PaymentPurpose;

		#endregion

		public static TransferDocumentType GetDocTypeFromString (string type)
		{
			switch (type) {
			case "Банковский ордер":
				return TransferDocumentType.BankOrder;
			case "Платежное поручение":
				return TransferDocumentType.PaymentDraft;
			case "Инкассовое поручение":
				return TransferDocumentType.IncassDraft;
			default: 
				throw new NotSupportedException (String.Format ("Тип банковского документа {0} неизвестен.", type));
			}
		}
	}

	public enum TransferDocumentType
	{
		[ItemTitleAttribute ("Банковский ордер")]
		BankOrder,
		[ItemTitleAttribute ("Платежное поручение")]
		PaymentDraft,
		[ItemTitleAttribute ("Инкассовое поручение")]
		IncassDraft
	}

	public class TransferDocumentTypeStringType : NHibernate.Type.EnumStringType
	{
		public TransferDocumentTypeStringType () : base (typeof(TransferDocumentType))
		{
		}
	}
}