using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using Gtk;
using ICSharpCode.SharpZipLib.Zip;
using QS.Banks.Domain;
using QS.BaseParameters;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Utilities;
using QSBanks.CBRSource;
using QSBanks.Repositories;
using QSProjectsLib;

namespace QSBanks
{
	public class BanksUpdater
	{
		static int UpdatePeriod = 5;

		public static void CheckBanksUpdate(bool forceUpdate)
		{
			if(!forceUpdate) {
				dynamic parameters = new ParametersService(new MySqlConnectionFactory(QSMain.ConnectionString));
				DateTime lastModified = parameters.last_banks_update ?? default(DateTime);

				int withoutUpdate = (int)DateTime.Now.Subtract(lastModified).TotalDays;
				if(withoutUpdate < UpdatePeriod) {
					return;
				}
				var runUpdate = MessageDialogWorks.RunQuestionDialog(
					lastModified == default(DateTime) ? "Справочник банков никогда не обновлялся. Обновить?" :
					NumberToTextRus.FormatCase(withoutUpdate, "Cправочник банков обновлялся\n{0} день назад. Обновить?",
					"Cправочник банков обновлялся\n{0} дня назад. Обновить?",
					"Cправочник банков обновлялся\n{0} дней назад. Обновить?"));
				if(!runUpdate) {
					return;
				}
			}
			BanksUpdateWindow updateWindow = new BanksUpdateWindow();
			updateWindow.Show();
		}

		public event EventHandler<int> OnProgress;
		public event EventHandler<string> OnOutputMessage;

		int accountsDeactivated = 0, banksRemoved = 0, banksDeactivated = 0, banksAdded = 0, banksFixed = 0;
		Stream bicZipFile;
		BIC bicDocument;
		RegionsEnum regions;
		List<Bank> allBanksList;
		List<Bank> activeBanksList;
		List<Account> accountsList;
		List<BankRegion> regionsList;
		private readonly ParametersService parametersService;

		public BanksUpdater(ParametersService parametersService)
		{
			this.parametersService = parametersService;
		}

		private void LoadBanks(IUnitOfWork uow)
		{
			allBanksList = BankRepository.GetAllBanks(uow).ToList();
			activeBanksList = allBanksList.Where(x => !x.Deleted).ToList();
			accountsList = AccountsRepository.GetAllAccounts(uow).ToList();
			regionsList = BankRepository.GetAllBankRegions(uow).ToList();
		}

		private void Progress(int current, int max)
		{
			var p = (int)((float)current / (float)max * 100f); ;
			if(p > 100) {
				p = 100;
			}
			if(p < 0) {
				p = 0;
			}
			OnProgress?.Invoke(this, p);
		}

		private void OutputMessage(string message)
		{
			OnOutputMessage?.Invoke(this, message);
		}

		public void UpdateBanks(Stream bicZipFile)
		{
			this.bicZipFile = bicZipFile;
			UpdateBanks();
		}

		public void UpdateBanks()
		{
			accountsDeactivated = banksRemoved = banksDeactivated = banksAdded = banksFixed = 0;
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot($"[BU]Обновление банков")) {
				LoadBanks(uow);

				bool successfullyUpdate = UpdateBanksFromCBR(uow);

				if(!successfullyUpdate) {
					MessageDialog error = new MessageDialog(null,
											  DialogFlags.Modal,
											  MessageType.Error,
											  ButtonsType.Ok,
											  "Не удалось загрузить обновленный справочник банков.\n" +
											  "Пожалуйста проверьте интернет соединение или повторите попытку позже.");
					error.SetPosition(WindowPosition.Center);
					error.ShowAll();
					error.Run();
					error.Destroy();
					return;
				}

				//Выводим статистику
				bool wasUpdated = ((accountsDeactivated | banksAdded | banksDeactivated | banksFixed | banksRemoved) != 0);
				if(!wasUpdated)
					OutputMessage("Данные справочника актуальны.");
				else
					OutputMessage(String.Format("Обновление справочника банков успешно завершено.\n\n" +
					"Добавлено банков: {0}\nИсправлено банков: {1}\nУдалено банков: {2}\n" +
					"Деактивировано счетов: {3}\nДеактивировано банков: {4}",
						banksAdded, banksFixed, banksRemoved, accountsDeactivated, banksDeactivated));
				//Записываем дату
				parametersService.UpdateParameter("last_banks_update", DateTime.Now.ToString("O"));
			}
		}

		private bool DownloadRegions()
		{
			OutputMessage("Загрузка справочника регионов");
			regions = RegionsEnum.GetRegions();
			return regions != null;
		}

		private bool DownloadBIC()
		{
			OutputMessage("Загрузка справочника банков");
			ZipFile banksZip;
			using(Stream bicZipData = DownloadBICFile()) {
				if(bicZipData == null) {
					return false;
				}
				banksZip = new ZipFile(bicZipData);
				ZipConstants.DefaultCodePage = Encoding.UTF8.CodePage;
				ZipEntry zipEntry = banksZip.GetEntry($"{DateTime.Today.Year}{DateTime.Today.Month.ToString("D2")}{DateTime.Today.Day.ToString("D2")}_ED807_full.xml");
				XmlSerializer ser = new XmlSerializer(typeof(ED807));
				using(Stream banks = banksZip.GetInputStream(zipEntry)) {
					bicDocument = BIC.GetBICDocument(banks);
				}
			}
			return bicDocument != null;
		}

		private void UpdateRegions(IUnitOfWork uow)
		{
			OutputMessage("Обновление регионов");
			var UpdatedObject = new List<object>();
			int index = 0;
			Progress(0, 1);
			foreach(var reg in regions.Items) {
				var foundedRegion = regionsList.FirstOrDefault(x => x.RegionNum == (int)reg.RegCode);
				if(foundedRegion == null) {
					foundedRegion = new BankRegion { RegionNum = (int)reg.RegCode };
				}
				foundedRegion.Region = reg.CNAME;
				uow.Save(foundedRegion);
				Progress(index, regions.Items.Count());
				index++;
			}
		}

		/// <summary>
		/// Загрузка справочника банков с Центрального банка
		/// </summary>
		private bool UpdateBanksFromCBR(IUnitOfWork uow)
		{
			if(!DownloadRegions() || !DownloadBIC()) {
				return false;
			}
			var UpdatedObject = new List<object>();

			UpdateRegions(uow);

			OutputMessage("Обновление банков");
			var bicList = bicDocument.BICDirectoryEntry.Where(x => x.Accounts != null);
			Dictionary<string, Dictionary<string, AccountsType>> loadedAccounts = bicList
				.ToDictionary(x => x.BIC, x => x.Accounts.ToDictionary(y => y.Account));
			var storedBanksDict = allBanksList.ToDictionary(x => x.Bik);
			var index = 0;
			foreach(var item in bicList) {
				Progress(index, bicList.Count());
				index++;
				Bank bank = null;
				if(storedBanksDict.ContainsKey(item.BIC)) {
					bank = storedBanksDict[item.BIC];
				}
				if(bank == null) {
					bank = new Bank();
					banksAdded++;
				} else {
					if(!CompareBank(bank, item)) {
						UpdatedObject.Add(bank);
						banksFixed++;
					} else {
						continue;
					}
				}
				bank.Bik = item.BIC;
				bank.City = item.ParticipantInfo.Nnp ?? "";
				bank.Name = item.ParticipantInfo.NameP;
				bank.Region = regionsList.FirstOrDefault(x => x.RegionNum == int.Parse(item.ParticipantInfo.Rgn));
				var currentBankLoadedAccounts = loadedAccounts[item.BIC];
				foreach(var account in bank.CorAccounts.ToList()) {
					if(currentBankLoadedAccounts.ContainsKey(account.CorAccountNumber)) {
						currentBankLoadedAccounts.Remove(account.CorAccountNumber);
					} else {
						bank.CorAccounts.Remove(account);
					}
				}
				foreach(var acc in currentBankLoadedAccounts) {
					bank.CorAccounts.Add(new CorAccount { CorAccountNumber = acc.Key, InBank = bank });
				}
				uow.Save(bank);
				if(bank.DefaultCorAccount == null) {
					bank.DefaultCorAccount = bank.CorAccounts.First();
					uow.Save(bank);
				}
			}
			OutputMessage("Обновление счетов, удаление неактуальных банков");
			var bl = activeBanksList.Where(bank => !bicList.Any(y => y.BIC == bank.Bik));
			index = 0;
			foreach(var item in bl) {
				Progress(index, bl.Count());
				index++;
				var account = accountsList.Where(x => !x.Inactive).FirstOrDefault(a => a.InBank == item);
				if(account != null) {
					if(item.Deleted) {
						continue;
					}
					account.Inactive = true;
					UpdatedObject.Add(account);
					accountsDeactivated++;

					item.Deleted = true;
					UpdatedObject.Add(item);
					banksDeactivated++;
				} else {
					uow.Delete(item);
					banksRemoved++;
				}
			}

			foreach(var bank in allBanksList) {
				if(bank.DefaultCorAccount == null) {
					if(bank.CorAccounts.Any()) {
						bank.DefaultCorAccount = bank.CorAccounts.First();
					} else {
						uow.Delete(bank);
						banksRemoved++;
					}
				}
			}

			uow.Commit();
			return true;
		}

		public static bool CompareBank(Bank bank, BICDirectoryEntryType loadedBank)
		{
			return bank.Bik == loadedBank.BIC &&
			bank.CorAccounts.All(x => loadedBank.Accounts.Any(y => y.Account == x.CorAccountNumber)) &&
			bank.Name == loadedBank.ParticipantInfo.NameP &&
			bank.Region != null && bank.Region.RegionNum == int.Parse(loadedBank.ParticipantInfo.Rgn);
		}

		private Stream DownloadBICFile()
		{
			if(bicZipFile != null) {
				return bicZipFile;
			}
			string url;
			DateTime date = DateTime.Today;
			//если не найдена ссылка на файл за текущий день, ищем рабочую ссылку за прошлые дни
			for(int daysAgo = 0; daysAgo < 3; daysAgo++) {
				url = GetLastFileURLIfExists(date.AddDays(-daysAgo));
				if(!string.IsNullOrEmpty(url)) {
					using(WebClient wc = new WebClient()) {
						return new MemoryStream(wc.DownloadData(url));
					}
				}
			}
			return null;
		}

		private string GetLastFileURLIfExists(DateTime date)
		{
			string baseURL = "http://www.cbr.ru/VFS/mcirabis/BIKNew/";
			string actualFileURL = "";
			for(byte dailyNumber = 1; dailyNumber < 99; dailyNumber++) {
				string currentFile = $"{date.Year}{date.Month.ToString("D2")}{date.Day.ToString("D2")}ED{dailyNumber.ToString("D2")}OSBR.zip";
				try {
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseURL + currentFile);
					request.Method = "HEAD";
					HttpWebResponse response = (HttpWebResponse)request.GetResponse();
					if(response.StatusCode == HttpStatusCode.OK) {
						actualFileURL = baseURL + currentFile;
						continue;
					}
					break;
				} catch(Exception) {
					break;
				}
			}
			return actualFileURL;
		}
	}
}

