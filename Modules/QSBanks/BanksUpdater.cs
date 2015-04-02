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
using NHibernate.Criterion;
using QSSupportLib;
using QSProjectsLib;
using Gtk;

namespace QSBanks
{
	public static class BanksUpdater
	{
		const string ARCHIVE_LINK = @"http://cbrates.rbc.ru/bnk/bnk.zip";
		const string BANKS_LIST_FILE = "bnkseek.txt";

		public static int updatePeriod = 3;

		public static void Update (bool forceUpdate)
		{
			int accountsDeactivated = 0, banksRemoved = 0, banksDeactivated = 0, banksAdded = 0, banksFixed = 0;
			DateTime lastModified = new DateTime ();

			if (MainSupport.BaseParameters.All.ContainsKey ("last_banks_update"))
				lastModified = DateTime.Parse (MainSupport.BaseParameters.All ["last_banks_update"]);

			if (!forceUpdate && (DateTime.Now - lastModified).Days < updatePeriod)
				return;

			if (!forceUpdate) {
				MessageDialog md = new MessageDialog (null, 
					                   DialogFlags.DestroyWithParent, 
					                   MessageType.Question,
					                   ButtonsType.YesNo,
					                   "Cправочник банков обновлялся более 2-х дней назад. Обновить?");
				md.Show ();
				int Result = md.Run ();
				md.Destroy ();
				if ((ResponseType)Result != ResponseType.Yes)
					return;
			}
			
			//Качаем архив.
			List<Bank> loadedBanksList = getBanksFromRbc ();
			//Получаем имеющиеся банки.
			ISession session = OrmMain.OpenSession ();
			List<Bank> oldBanksList = (List<Bank>)session.CreateCriteria<Bank> ()
				.Add (Restrictions.Eq ("Deleted", false)).List<Bank> ();
			List<Account> accountsList = (List<Account>)session.CreateCriteria<Account> ()
				.Add (Restrictions.Eq ("Inactive", false)).List<Account> ();

			//Добавляем новые и исправляем старые
			foreach (Bank loadedBank in loadedBanksList) {
				int i;
				if ((i = oldBanksList.FindIndex (oldBank => oldBank.Bik == loadedBank.Bik)) != -1) {
					if (Bank.EqualsWithoutId (oldBanksList [i], loadedBank))
						continue;
					oldBanksList [i].City = loadedBank.City;
					oldBanksList [i].CorAccount = loadedBank.CorAccount;
					oldBanksList [i].Name = loadedBank.Name;
					oldBanksList [i].Region = loadedBank.Bik.Length > 4 ? loadedBank.Bik.Substring (2, 2) : "";
					banksFixed++;
					continue;
				}
				oldBanksList.Add (loadedBank);
				session.Persist (loadedBank);
				banksAdded++;
			}
			//Удаляем неактуальные банки
			foreach (Bank oldBank in oldBanksList) {
				if (loadedBanksList.FindIndex (loadedBank => loadedBank.City == oldBank.City &&
				    loadedBank.Bik == oldBank.Bik &&
				    loadedBank.CorAccount == oldBank.CorAccount &&
				    loadedBank.Name == oldBank.Name &&
				    loadedBank.Region == oldBank.Region) == -1) {
					int accountIdx = -1;
					if ((accountIdx = accountsList.FindIndex (a => a.InBank == oldBank)) == -1) {
						session.Delete (oldBank);
						banksRemoved++;
					} else {
						oldBank.Deleted = accountsList [accountIdx].Inactive = true;
						accountsDeactivated++;
						banksDeactivated++;
						continue;
					}
				}
			}
			session.Flush ();
			session.Close ();
			//Выводим статистику
			MessageDialog infoDlg = new MessageDialog (null, 
				                        DialogFlags.DestroyWithParent, 
				                        MessageType.Info,
				                        ButtonsType.Ok,
				                        "Обновление справочника успешно завершено.\n" +
				                        "Добавлено банков: {0}\nИсправлено банков: {1}\nУдалено банков: {2}\n" +
				                        "Деактивировано счетов: {3}\nДеактивировано банков: {4}",
				                        banksAdded, banksFixed, banksRemoved, accountsDeactivated, banksDeactivated);
			infoDlg.Show ();
			infoDlg.Run ();
			infoDlg.Destroy ();
			//Записываем дату
			MainSupport.BaseParameters.UpdateParameter (QSMain.ConnectionDB, "last_banks_update", lastModified.ToString ());
		}

		static List<Bank> getBanksFromRbc ()
		{
			String zipFileName = Path.Combine (Path.GetTempPath (), String.Format ("bnk.zip"));
			using (WebClient webClient = new WebClient ())
				webClient.DownloadFile (new Uri (ARCHIVE_LINK), zipFileName);

			//Распаковываем архив
			MemoryStream zipStream = new MemoryStream ();
			ZipConstants.DefaultCodePage = Encoding.UTF8.CodePage;
			using (FileStream fs = new FileStream (zipFileName, FileMode.Open, FileAccess.Read)) {
				byte[] buffer = new byte[4096];
				StreamUtils.Copy (fs, zipStream, buffer);
			}
			ZipFile banksZip = new ZipFile (zipStream);

			//Читаем файл с банками
			ZipEntry zipEntry = banksZip.GetEntry (BANKS_LIST_FILE);
			List<Bank> loadedBanksList = new List<Bank> ();
			using (Stream banks = banksZip.GetInputStream (zipEntry))
			using (StreamReader sr = new StreamReader (banks, Encoding.GetEncoding (1251))) {
				string bankInfoString;
				while (!String.IsNullOrEmpty ((bankInfoString = sr.ReadLine ()))) {
					var bankRegexMatch = Regex.Match (bankInfoString, @"^\d+\t(.+)\t\d+\t(.+)\t\t(\d+)\t(\d*)$");
					Bank bank = new Bank ();
					bank.City = bankRegexMatch.Groups [1].Value;
					bank.Name = bankRegexMatch.Groups [2].Value;
					bank.Bik = bankRegexMatch.Groups [3].Value;
					bank.CorAccount = bankRegexMatch.Groups [4].Value;
					bank.Region = bank.Bik.Length > 4 ? bank.Bik.Substring (2, 2) : "";
					loadedBanksList.Add (bank);
				}
			}
			zipStream.Close ();

			//Удаляем казначейства и прочие организации, не имеющие кор. счета.
			loadedBanksList.RemoveAll (m => String.IsNullOrEmpty (m.CorAccount));

			return loadedBanksList;
		}
	}
}

