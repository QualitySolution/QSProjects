using System;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using NHibernate;
using QSOrmProject;

namespace QSBanks
{
	public static class BanksUpdater
	{
		const string ARCHIVE_LINK = @"http://cbrates.rbc.ru/bnk/bnk.zip";
		const string BANKS_LIST_FILE = "bnkseek.txt";

		public static void Update ()
		{
			
			//Получаем версию дату изменения справочника.
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (ARCHIVE_LINK);
			request.Method = "HEAD";
			HttpWebResponse resp = (HttpWebResponse)request.GetResponse ();
			Console.WriteLine (resp.LastModified);

			//TODO: Сравнение даты и если новый - спросить действие.

			//Качаем архив.
			String zipFileName = Path.Combine (Path.GetTempPath (), String.Format ("bnk.zip"));
			using (WebClient webClient = new WebClient ()) {
				webClient.DownloadFile (new Uri (ARCHIVE_LINK), zipFileName);
			}
			//Распаковываем архив
			MemoryStream zipStream = new MemoryStream ();
			ZipConstants.DefaultCodePage = System.Text.Encoding.UTF8.CodePage;
			using (FileStream fs = new FileStream (zipFileName, FileMode.Open, FileAccess.Read)) {
				byte[] buffer = new byte[4096];
				StreamUtils.Copy (fs, zipStream, buffer);
			}
			ZipFile banksZip = new ZipFile (zipStream);
			//Читаем файл с банками
			ZipEntry entry = banksZip.GetEntry (BANKS_LIST_FILE);
			List<Bank> actualBanksList = new List<Bank> ();
			using (Stream banks = banksZip.GetInputStream (entry))
			using (StreamReader sr = new StreamReader (banks, Encoding.GetEncoding (1251))) {
				string bankInfo;

				while (!String.IsNullOrEmpty ((bankInfo = sr.ReadLine ()))) {
					var bank = Regex.Match (bankInfo, @"^\d+\t(.+)\t\d+\t(.+)\t\t(\d+)\t(\d*)$");
					Bank b = new Bank ();
					b.City = bank.Groups [1].Value;
					b.Name = bank.Groups [2].Value;
					b.Bik = bank.Groups [3].Value;
					b.CorAccount = bank.Groups [4].Value;
					b.Region = b.Bik.Length > 4 ? b.Bik.Substring (2, 2) : "";
					actualBanksList.Add (b);
				}
			}
			zipStream.Close ();
			//Удаляем казначейства и прочие организации, не имеющие кор. счета.
			actualBanksList.RemoveAll (m => String.IsNullOrEmpty (m.CorAccount));
			//Получаем имеющиеся банки.
			ISession session = OrmMain.OpenSession ();
			List<Bank> oldBanksList = (List<Bank>)session.CreateCriteria<Bank> ().List<Bank> ();
			List<Account> accountsList = (List<Account>)session.CreateCriteria<Account> ().List<Account> ();
			//Добавляем новые и исправляем старые
			int accountsDeactivated = 0, banksRemoved = 0, banksDeactivated = 0, banksAdded = 0, banksFixed = 0;
			foreach (Bank b in actualBanksList) {
				if (oldBanksList.FindIndex (m => m == b) != -1)
					continue;
				int i;
				if ((i = oldBanksList.FindIndex (m => m.Bik == b.Bik)) != -1) {
					oldBanksList [i].City = b.City;
					oldBanksList [i].CorAccount = b.CorAccount;
					oldBanksList [i].Name = b.Name;
					oldBanksList [i].Region = b.Bik.Length > 4 ? b.Bik.Substring (2, 2) : "";
					banksFixed++;
					continue;
				}
				oldBanksList.Add (b);
				banksAdded++;
			}
			//Удаляем неактуальные банки
			foreach (Bank b in oldBanksList) {
				if (actualBanksList.FindIndex (m => m.City == b.City &&
				    m.Bik == b.Bik &&
				    m.CorAccount == b.CorAccount &&
				    m.Name == b.Name &&
				    m.Region == b.Region) == -1) {
					if (accountsList.FindIndex (a => a.InBank == b) == -1) {
						//oldBanksList.Remove (b);
						banksRemoved++;
					} else {
						accountsDeactivated++;
						banksDeactivated++;
						continue; //TODO: Поставить банку значение удален и счету значение неактивен.
					}
				}
			}
			//TODO: Сохранение изменений в базу. Созранение даты последнего обновления.
			session.Close ();
		}
	}
}

