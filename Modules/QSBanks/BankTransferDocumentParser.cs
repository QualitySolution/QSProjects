using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace QSBanks
{
	public class BankTransferDocumentParser
	{
		public string DocumentPath { get; private set; }

		public List<TransferDocument> TransferDocuments;

		private Dictionary<string, string> documentProperties;
		private List<Dictionary<string, string>> accounts;
		private List<Dictionary<string, string>> documents;

		public BankTransferDocumentParser (string documentPath)
		{
			documentProperties = new Dictionary<string, string> ();
			accounts = new List<Dictionary<string, string>> ();
			documents = new List<Dictionary<string, string>> ();
			DocumentPath = documentPath;
		}

		public void Parse ()
		{
			//TODO Сделать выбор кодировки. Возможна кодировка DOS 866.
			using (var reader = new StreamReader (DocumentPath, Encoding.GetEncoding (1251))) {

				//Проверяем заголовок документа
				if (reader.ReadLine () != "1CClientBankExchange")
					return;

				//Читаем тело документа
				while (reader.Peek () >= 0) {

					string data = reader.ReadLine ();

					//Читаем свойства документа
					while (!data.StartsWith ("СекцияРасчСчет")) {
						if (!String.IsNullOrWhiteSpace (data)) {
							var dataArray = data.Split (new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
							if (dataArray.Length == 2) {
								documentProperties.Add (dataArray [0], dataArray [1]);
							}
						}
						data = reader.ReadLine ();
					}

					//Читаем рассчетные счета
					int i = -1;
					while (!data.StartsWith ("СекцияДокумент")) {
						if (!String.IsNullOrWhiteSpace (data)) {
							if (data.StartsWith ("СекцияРасчСчет"))
								i++;
							if (accounts.Count <= i)
								accounts.Add (new Dictionary<string, string> ());
							var dataArray = data.Split (new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
							if (dataArray.Length == 2) {
								accounts [i].Add (dataArray [0], dataArray [1]);
							}
						}
						data = reader.ReadLine ();
					}

					//Читаем документы
					i = -1;
					while (!data.StartsWith ("КонецФайла")) {
						if (!String.IsNullOrWhiteSpace (data)) {
							if (data.StartsWith ("СекцияДокумент"))
								i++;
							if (documents.Count <= i)
								documents.Add (new Dictionary<string, string> ());
							var dataArray = data.Split (new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
							if (dataArray.Length == 2) {
								documents [i].Add (dataArray [0], dataArray [1]);
							} else if (dataArray.Length == 1) {
								documents [i].Add (dataArray [0], String.Empty);
							}
						}
						data = reader.ReadLine ();
					}
				}
			}
			fillData ();
		}

		private void fillData ()
		{
			var culture = CultureInfo.CreateSpecificCulture ("ru-RU");
			TransferDocuments = new List<TransferDocument> ();
			foreach (var document in documents) {
				TransferDocument doc = new TransferDocument ();
				doc.DocumentType = TransferDocument.GetDocTypeFromString (document ["СекцияДокумент"]);
				doc.Number = document ["Номер"];
				doc.Date = DateTime.Parse (document ["Дата"], culture);
				doc.Total = Decimal.Parse (document ["Сумма"]);
				doc.PayerAccount = document ["ПлательщикСчет"];
				if (!String.IsNullOrWhiteSpace (document ["ДатаСписано"]))
					doc.WriteoffDate = DateTime.Parse (document ["ДатаСписано"], culture);
				doc.PayerName = document ["Плательщик"];
				doc.PayerInn = document ["ПлательщикИНН"];
				doc.PayerKpp = document ["ПлательщикКПП"];
				doc.PayerCheckingAccount = document ["ПлательщикРасчСчет"];
				doc.PayerBank = document ["ПлательщикБанк1"];
				doc.PayerBik = document ["ПлательщикБИК"];
				doc.PayerCorrespondentAccount = document ["ПлательщикКорсчет"];
				doc.RecipientAccount = document ["ПолучательСчет"];
				if (!String.IsNullOrWhiteSpace (document ["ДатаПоступило"]))
					doc.ReceiptDate = DateTime.Parse (document ["ДатаПоступило"], culture);
				doc.RecipientName = document ["Получатель"];
				doc.RecipientInn = document ["ПолучательИНН"];
				doc.RecepientKpp = document ["ПолучательКПП"];
				doc.RecipientCheckingAccount = document ["ПолучательРасчСчет"];
				doc.RecepientBank = document ["ПолучательБанк1"];
				doc.RecepientBik = document ["ПолучательБИК"];
				doc.RecepientCorrespondentAccount = document ["ПолучательКорсчет"];
				doc.PaymentPurpose = document ["НазначениеПлатежа"];

				TransferDocuments.Add (doc);
			}
		}
	}
}