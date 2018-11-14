﻿using System;
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
using QS.Project.DB;
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
		const string BANKS_REGIONS_FILE = "reg.txt";

		public static int UpdatePeriod = 5;

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

			int withoutUpdate = (int)DateTime.Now.Subtract (lastModified).TotalDays;
			if (!forceUpdate && withoutUpdate < UpdatePeriod)
				return;

			if (!forceUpdate) {
				if (!MessageDialogWorks.RunQuestionDialog (
					    lastModified == default(DateTime) ? "Справочник банков никогда не обновлялся. Обновить?" :
					RusNumber.FormatCase (withoutUpdate, "Cправочник банков обновлялся\n{0} день назад. Обновить?",
						    "Cправочник банков обновлялся\n{0} дня назад. Обновить?", 
						    "Cправочник банков обновлялся\n{0} дней назад. Обновить?")))
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
			ISession session = OrmConfig.OpenSession ();
			List<Bank> oldBanksList = (List<Bank>)session.CreateCriteria<Bank> ()
				.Add (Restrictions.Eq ("Deleted", false)).List<Bank> ();
			List<Account> accountsList = (List<Account>)session.CreateCriteria<Account> ()
				.Add (Restrictions.Eq ("Inactive", false)).List<Account> ();

			//Добавляем новые и исправляем старые
			var UpdatedObject = new List<object> ();
			foreach (Bank loadedBank in loadedBanksList) {
				Bank foundBank = oldBanksList.Find (oldBank => oldBank.Bik == loadedBank.Bik);
				if (foundBank != null) {
					if (Bank.EqualsWithoutId (foundBank, loadedBank))
						continue;
					foundBank.City = loadedBank.City;
					foundBank.CorAccount = loadedBank.CorAccount;
					foundBank.Name = loadedBank.Name;
					foundBank.Region = loadedBank.Region;
					UpdatedObject.Add (foundBank);
					banksFixed++;
				} else {
					oldBanksList.Add (loadedBank);
					session.Persist (loadedBank);
					UpdatedObject.Add (loadedBank);
					banksAdded++;
				}
			}
			//Удаляем неактуальные банки
			foreach (Bank forRemoveBank in oldBanksList.FindAll 
				(oldBank => !loadedBanksList.Exists (loadedBank => Bank.EqualsWithoutId (loadedBank, oldBank)))) {
				if (accountsList.Exists (a => a.InBank == forRemoveBank)) {
					if (forRemoveBank.Deleted)
						continue;
					forRemoveBank.Deleted = true;
					UpdatedObject.Add (forRemoveBank);
					banksDeactivated++;
				} else {
					session.Delete (forRemoveBank);
					banksRemoved++;
				}
			}

			//Деактивируем счета
			foreach (Account acc in accountsList.FindAll (a => !a.Inactive && a.InBank != null && a.InBank.Deleted)) {
				acc.Inactive = true;
				UpdatedObject.Add (acc);
				accountsDeactivated++;
			}
			session.Flush ();
			session.Close ();

			OrmMain.NotifyObjectUpdated (UpdatedObject.ToArray ());
			updateWindow.Destroy ();
			//Выводим статистику
			string message;
			bool wasUpdated = ((accountsDeactivated | banksAdded | banksDeactivated | banksFixed | banksRemoved) != 0);
			if (!wasUpdated)
				message = "Данные справочника актуальны.";
			else
				message = String.Format ("Обновление справочника банков успешно завершено.\n\n" +
				"Добавлено банков: {0}\nИсправлено банков: {1}\nУдалено банков: {2}\n" +
				"Деактивировано счетов: {3}\nДеактивировано банков: {4}",
					banksAdded, banksFixed, banksRemoved, accountsDeactivated, banksDeactivated);
			MessageDialog infoDlg = new MessageDialog (null, 
				                        DialogFlags.Modal, 
				                        MessageType.Info,
				                        ButtonsType.Ok,
				                        message);
			//Если будет нужен более подробный вывод того, что произошло.
			//if (wasUpdated)
			//	infoDlg.AddButton ("Подробнее...", 42);
			infoDlg.SetPosition (WindowPosition.Center);
			infoDlg.Show ();
			infoDlg.Run ();
			infoDlg.Destroy ();
			//Записываем дату
			MainSupport.BaseParameters.UpdateParameter (QSMain.ConnectionDB, "last_banks_update", DateTime.Now.ToString ("O"));
		}

		static List<Bank> getBanksFromRbc ()
		{
			String zipFileName = Path.Combine (Path.GetTempPath (), String.Format ("bnk.zip"));
			try {
				using (WebClient webClient = new WebClient ())
					webClient.DownloadFile (new Uri (ARCHIVE_LINK), zipFileName);
			} catch (Exception ex) {
				logger.Error (ex, "Не удалось загрузить обновленную информацию о банках с сайта РБК.");
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

			//Загружаем список регионов.
			ZipEntry zipEntry = banksZip.GetEntry (BANKS_REGIONS_FILE);
			List<BankRegion> regions = new List<BankRegion> ();
			using (Stream banks = banksZip.GetInputStream (zipEntry))
			using (StreamReader sr = new StreamReader (banks, Encoding.GetEncoding (1251))) {
				string regionInfoString;
				while (!String.IsNullOrEmpty ((regionInfoString = sr.ReadLine ()))) {
					var regionRegexMatch = Regex.Match (regionInfoString, @"^(\d+)\t(.+)\t(.*)$");
					BankRegion region = new BankRegion ();
					region.RegionNum = Int32.Parse (regionRegexMatch.Groups [1].Value);
					region.Region = regionRegexMatch.Groups [2].Value;
					region.City = regionRegexMatch.Groups [3].Value;
					regions.Add (region);
				}
			}
			regions = fillRegions (regions);

			//Читаем файл с банками
			zipEntry = banksZip.GetEntry (BANKS_LIST_FILE);
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
					bank.Region = bank.Bik.Length > 4 ? 
						regions.Find (m => m.RegionNum == Int32.Parse (bank.Bik.Substring (2, 2))) : null;
					loadedBanksList.Add (bank);
				}
			}
			//Удаляем казначейства и прочие организации, не имеющие кор. счета.
			loadedBanksList.RemoveAll (m => String.IsNullOrEmpty (m.CorAccount));

			zipStream.Close ();

			return loadedBanksList;
		}

		static List<BankRegion> fillRegions (List<BankRegion> newRegions)
		{
			ISession session = OrmConfig.OpenSession ();
			List<BankRegion> oldRegionsList = (List<BankRegion>)session.CreateCriteria<BankRegion> ().List<BankRegion> ();
			List<Bank> banksList = (List<Bank>)session.CreateCriteria<Bank> ().List<Bank> ();

			foreach (BankRegion region in newRegions) {
				int i;
				if ((i = oldRegionsList.FindIndex (oldR => oldR.RegionNum == region.RegionNum)) != -1) {
					if (BankRegion.EqualsWithoutId (oldRegionsList [i], region))
						continue;
					oldRegionsList [i].City = region.City;
					oldRegionsList [i].Region = region.City;
				}
				oldRegionsList.Add (region);
				session.Persist (region);
				continue;
				//Тут могла бы быть ваша статистика.
			}
			//Удаляем старые
			foreach (BankRegion oldR in oldRegionsList) {
				if (newRegions.FindIndex (newR => newR.City == oldR.City &&
				    newR.Region == oldR.Region &&
				    newR.RegionNum == oldR.RegionNum) == -1) {
					List<Bank> banksToFix = banksList.FindAll (bank => bank.Region == oldR);
					if (banksToFix.Count > 0) {
						foreach (Bank b in banksToFix)
							b.Region = null;
					}
					session.Delete (oldR);
				}
			}
			session.Flush ();
			oldRegionsList = (List<BankRegion>)session.CreateCriteria<BankRegion> ().List<BankRegion> ();
			session.Close ();
			return oldRegionsList;
		}
	}
}

