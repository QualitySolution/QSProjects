using System.Collections.Generic;
using NUnit.Framework;
using QS.Banks.Domain;
using QSBanks;
using QSBanks.CBRSource;

namespace QS.Test.Banks
{
	[TestFixture()]
	public class BanksUpdaterTests
	{
		[TestCaseSource("CompareBankCases")]
		public void CompareBankTestCase(object bank, object bankSource, object expectedResult, object message)
		{
			var result = BanksUpdater.CompareBank(bank as Bank, bankSource as BICDirectoryEntryType);
			Assert.AreEqual(result, expectedResult, 
				$"{message} Результат должен быть: {expectedResult}, Результат проверки: {result}" );
		}

		static object[] CompareBankCases()
		{
			//source 1
			var region1 = CreateNewRegion();
			var bank1 = CreateNewBank(region1);
			var corAccounts1 = CreateNewCorAccounts(bank1, "123456789123456", "987654321987654");
			bank1.CorAccounts = corAccounts1;
			bank1.DefaultCorAccount = corAccounts1[0];

			var bankSpr1 = new BICDirectoryEntryType {
				BIC = bank1.Bik,
				ParticipantInfo = new ParticipantInfoType {
					NameP = bank1.Name,
					Rgn = region1.RegionNum.ToString()
				},
				Accounts = new AccountsType[] {
					new AccountsType { Account = corAccounts1[0].CorAccountNumber },
					new AccountsType { Account = corAccounts1[1].CorAccountNumber }
				}
			};
			var source1 = new object[] { bank1, bankSpr1, true, "Банки полностью идентичны" };

			//source 2
			var region2 = CreateNewRegion();
			var bank2 = CreateNewBank(region2);
			var corAccounts2 = CreateNewCorAccounts(bank2, "123456789123456", "987654321987654");
			bank2.CorAccounts = corAccounts2;
			bank2.DefaultCorAccount = corAccounts2[0];

			var bankSpr2 = new BICDirectoryEntryType {
				BIC = bank2.Bik,
				ParticipantInfo = new ParticipantInfoType {
					NameP = bank2.Name,
					Rgn = region2.RegionNum.ToString()
				},
				Accounts = new AccountsType[] {
					new AccountsType { Account = corAccounts2[0].CorAccountNumber }
					//специально отсутствует счет corAccounts2[1]
				}
			};

			var source2 = new object[] { bank2, bankSpr2, false, "Банки отличаются отсутствием счета в банке из справочника" };
			return new object[] { source1, source2 };
		}
		static BankRegion CreateNewRegion()
		{
			return new BankRegion {
				City = "TestRegionCity",
				Region = "TestRegion",
				RegionNum = 33
			};
		}
		static Bank CreateNewBank(BankRegion region)
		{
			return new Bank {
				Name = "TestBank",
				Bik = "12345678",
				Deleted = false,
				Region = region
			};
		}
		static List<CorAccount> CreateNewCorAccounts(Bank bank, params string[] accounts)
		{
			var accs = new List<CorAccount>();
			foreach(var acc in accounts) {
				accs.Add(new CorAccount { CorAccountNumber = acc, InBank = bank });
			}
			return accs;
		}

		[Test]
		public void EqualsWithoutIdTestCase() 
		{
			var region1 = CreateNewRegion();
			var bank1 = CreateNewBank(region1);
			var corAccounts1 = CreateNewCorAccounts(bank1, "123456789123456", "987654321987654");
			bank1.CorAccounts = corAccounts1;
			bank1.DefaultCorAccount = corAccounts1[0];

			var region2 = CreateNewRegion();
			region2.Id = 2;
			var bank2 = CreateNewBank(region2);
			bank2.Id = 2;
			var corAccounts2 = CreateNewCorAccounts(bank2, "123456789123456", "987654321987654");
			corAccounts2[0].Id = 2;
			corAccounts2[1].Id = 3;
			bank2.CorAccounts = corAccounts2;
			bank2.DefaultCorAccount = corAccounts2[0];

			Assert.IsTrue(Bank.EqualsWithoutId(bank1, bank2), "Сравнение одинаковых банков");
		}
	}
}
