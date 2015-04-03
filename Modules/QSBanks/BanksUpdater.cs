using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Gtk;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using NHibernate;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSSupportLib;

namespace QSBanks
{
	public static class BanksUpdater
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		const string ARCHIVE_LINK = @"http://cbrates.rbc.ru/bnk/bnk.zip";
		const string BANKS_LIST_FILE = "bnkseek.txt";

		public static int UpdatePeriod = 3;

		static Window updateWindow = new Window ("Идет обновление справочника банков...");

		public static void ShowProgress ()
		{
			Label updateInProgress = new Label ();
			updateInProgress.Text = "Пожалуйста, подождите...";
			updateInProgress.Justify = Justification.Center;
			VBox vbox = new VBox ();
			vbox.PackStart (updateInProgress, true, true, 0);
			vbox.ShowAll ();
			updateWindow.SetSizeRequest (300, 25); 
			updateWindow.Resizable = false;
			updateWindow.SetPosition (WindowPosition.Center);
			updateWindow.Add (vbox);

			updateWindow.ShowAll ();
			QSMain.WaitRedraw ();
		}

		public static void Update (bool forceUpdate)
		{
			int accountsDeactivated = 0, banksRemoved = 0, banksDeactivated = 0, banksAdded = 0, banksFixed = 0;
			DateTime lastModified = new DateTime ();

			if (MainSupport.BaseParameters.All.ContainsKey ("last_banks_update"))
				lastModified = DateTime.Parse (MainSupport.BaseParameters.All ["last_banks_update"]);

			if (!forceUpdate && (DateTime.Now - lastModified).Days < UpdatePeriod)
				return;

			if (!forceUpdate) {
				MessageDialog md = new MessageDialog (null, 
					                   DialogFlags.Modal, 
					                   MessageType.Question,
					                   ButtonsType.YesNo,
					                   "Cправочник банков обновлялся более 2-х дней назад. Обновить?");
				md.SetPosition (WindowPosition.Center);
				md.Show ();
				int Result = md.Run ();
				md.Destroy ();
				if ((ResponseType)Result != ResponseType.Yes)
					return;
			}
			ShowProgress ();
			//Качаем архив.
			List<Bank> loadedBanksList = getBanksFromRbc ();
			if (loadedBanksList == null) {
				updateWindow.Destroy ();
				MessageDialog error = new MessageDialog (null,
					                      DialogFlags.Modal,
					                      MessageType.Error,
					                      ButtonsType.Ok,
					                      "Не удалось загрузить обновленный справочник банков.\n" +
					                      "Пожалуйста проверьте интернет соединение или повторите попытку позже.");
				error.SetPosition (WindowPosition.Center);
				error.ShowAll ();
				error.Run ();
				error.Destroy ();
				return;
			}
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
					if ((accountsList.FindIndex (a => a.InBank == oldBank)) == -1) {
						session.Delete (oldBank);
						banksRemoved++;
					} else
						oldBank.Deleted = true;
				}
			}

			//Деактивируем счета
			foreach (Account acc in accountsList)
				if (acc.InBank != null && acc.InBank.Deleted) {
					acc.Inactive = true;
					accountsDeactivated++;
				}
			session.Flush ();
			session.Close ();

			OrmMain.NotifyObjectUpdated (new Bank ());
			updateWindow.Destroy ();
			//Выводим статистику
			MessageDialog infoDlg = new MessageDialog (null, 
				                        DialogFlags.Modal, 
				                        MessageType.Info,
				                        ButtonsType.Ok,
				                        "Обновление справочника успешно завершено.\n" +
				                        "Добавлено банков: {0}\nИсправлено банков: {1}\nУдалено банков: {2}\n" +
				                        "Деактивировано счетов: {3}\nДеактивировано банков: {4}",
				                        banksAdded, banksFixed, banksRemoved, accountsDeactivated, banksDeactivated);
			infoDlg.SetPosition (WindowPosition.Center);
			infoDlg.Show ();
			infoDlg.Run ();
			infoDlg.Destroy ();
			//Записываем дату
			MainSupport.BaseParameters.UpdateParameter (QSMain.ConnectionDB, "last_banks_update", DateTime.Now.ToString ());
		}

		static List<Bank> getBanksFromRbc ()
		{
			String zipFileName = Path.Combine (Path.GetTempPath (), String.Format ("bnk.zip"));
			try {
				using (WebClient webClient = new WebClient ())
					webClient.DownloadFile (new Uri (ARCHIVE_LINK), zipFileName);
			} catch (Exception ex) {
				logger.ErrorException ("Не удалось загрузить обновленную информацию о банках с сайта РБК.", ex);
				return null;
			}

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

