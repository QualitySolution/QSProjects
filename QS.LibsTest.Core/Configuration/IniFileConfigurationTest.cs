using System.IO;
using NUnit.Framework;
using QS.Configuration;

namespace QS.Test.Configuration {
	[TestFixture(TestOf = typeof(IniFileConfiguration))]
	public class IniFileConfigurationTest {

		#region Helpers
		private readonly string tempIniFile = Path.GetTempPath() + "test_config.ini";

		[TearDown]
		public void DeleteFile() {
			if(File.Exists(tempIniFile))
			{
				File.Delete(tempIniFile);
			}
		}

		#endregion

		[Test(Description = "Проверяем что можем создать файл.")]
		public void CanCreateFile() {
			DeleteFile();
			var config = new IniFileConfiguration(tempIniFile);
			config["Default:ConnectionName"] = "Водоканал Краснодар";
			
			Assert.That(File.Exists(tempIniFile));
			Assert.That(File.ReadAllText(tempIniFile).TrimEnd(), 
				Is.EqualTo("[Default]\r\nConnectionName = Водоканал Краснодар"));
		}

		[Test(Description = "Проверяем что правильно читаем файл.")]
		public void ReadValue() {
			File.WriteAllText(tempIniFile, @"[AppUpdater]
SkipVersion = 2.6.1.0
[OracleConnection]
DataSource = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = tor.nlmk)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = tor.nlmk)))
User = GANKOV_AV
Password = gankov_av
[Login10]
ConnectionName = Мой ноутбук
Server = 192.168.50.50
Type = 0
Account = 
DataBase = workwear_osmbt_copy
UserLogin = andrey
[CardReader]
Address = 0000001:1
CardTypes = EF_MIFARE");
			
			var config = new IniFileConfiguration(tempIniFile);
			//Обычное чтение
			Assert.That(config["AppUpdater:SkipVersion"], Is.EqualTo("2.6.1.0"));
			Assert.That(config["OracleConnection:DataSource"], Is.EqualTo("(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = tor.nlmk)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = tor.nlmk)))"));
			Assert.That(config["Login10:ConnectionName"], Is.EqualTo("Мой ноутбук"));
			Assert.That(config["Login10:Account"], Is.EqualTo(""));
			Assert.That(config["CardReader:Address"], Is.EqualTo("0000001:1"));
			//Проверяем что не упадем при чтении отсутствующей секции
			Assert.That(config["Login:ConnectionName"], Is.Null.Or.Empty);
			//Проверяем что не упадем при чтении отсутствующего параметра
			Assert.That(config["CardReader:NotExist"], Is.Null.Or.Empty);
		}
		
		[Test(Description = "Проверяем что можем удалять ключи из файла.")]
		public void DeleteValue() {
			File.WriteAllText(tempIniFile, @"[OracleConnection]
DataSource = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = tor.nlmk)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = tor.nlmk)))
User = GANKOV_AV
Password = gankov_av
[Login10]
ConnectionName = Мой ноутбук
Server = 192.168.50.50
Type = 0
Account = 
DataBase = workwear_osmbt_copy
UserLogin = andrey
[AppUpdater]
SkipVersion = 2.6.1.0
[CardReader]
Address = 0000001:1
CardTypes = EF_MIFARE");
			
			var config = new IniFileConfiguration(tempIniFile);
			//Удаляем только поле
			config["OracleConnection:Password"] = null;
			//Удаляем секцию целиком, так как в ней не остается параметров
			config["AppUpdater:SkipVersion"] = null;
			//Убираем видновый окончания чтобы сравнивать, так же новый движок добавляет разделительную пустую строку между секциями, тоже ее убираем.
			Assert.That(File.ReadAllText(tempIniFile).Replace("\r","").Replace("\n\n", "\n"), Is.EqualTo(@"[OracleConnection]
DataSource = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = tor.nlmk)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = tor.nlmk)))
User = GANKOV_AV
[Login10]
ConnectionName = Мой ноутбук
Server = 192.168.50.50
Type = 0
Account = 
DataBase = workwear_osmbt_copy
UserLogin = andrey
[CardReader]
Address = 0000001:1
CardTypes = EF_MIFARE
")); 
		}
		
		[Test(Description = "Проверяем что можем удалить целиком секцию из файла.")]
		public void DeleteSection() {
			File.WriteAllText(tempIniFile, @"[OracleConnection]
DataSource = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = tor.nlmk)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = tor.nlmk)))
User = GANKOV_AV
Password = gankov_av
[Login10]
ConnectionName = Мой ноутбук
Server = 192.168.50.50
Type = 0
Account = 
DataBase = workwear_osmbt_copy
UserLogin = andrey
[AppUpdater]
SkipVersion = 2.6.1.0
[CardReader]
Address = 0000001:1
CardTypes = EF_MIFARE");
			
			var config = new IniFileConfiguration(tempIniFile);
			//Удаляем секцию
			config["Login10:"] = null;
			//Убираем видновый окончания чтобы сравнивать, так же новый движок добавляет разделительную пустую строку между секциями, тоже ее убираем.
			Assert.That(File.ReadAllText(tempIniFile).Replace("\r","").Replace("\n\n", "\n"), Is.EqualTo(@"[OracleConnection]
DataSource = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = tor.nlmk)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = tor.nlmk)))
User = GANKOV_AV
Password = gankov_av
[AppUpdater]
SkipVersion = 2.6.1.0
[CardReader]
Address = 0000001:1
CardTypes = EF_MIFARE
")); 
		}
		
		[Test(Description = "Проверяем что не создаем пустых секций при проверке значения.")]
		[Category("real case")]
		public void DontMakeEmptySections() {
			File.WriteAllText(tempIniFile, @"[OracleConnection]
DataSource = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = tor.nlmk)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = tor.nlmk)))
User = GANKOV_AV
Password = gankov_av
[Login10]
ConnectionName = Мой ноутбук
Server = 192.168.50.50
Type = 0
Account = 
DataBase = workwear_osmbt_copy
UserLogin = andrey
[AppUpdater]
SkipVersion = 2.6.1.0
[CardReader]
Address = 0000001:1
CardTypes = EF_MIFARE");
			
			var config = new IniFileConfiguration(tempIniFile);
			//Читаем несуществующее поле.
			Assert.That(config["Login50:ConnectionName"], Is.Null);
			//Для того чтобы записался конфиг
			config["Login11:ConnectionName"] = "Тест";
			
			//Ниже не должно создаться секции Login50 мы только проверяли наличие в ней поля.
			Assert.That(File.ReadAllText(tempIniFile).Replace("\r","").Replace("\n\n", "\n"), Is.EqualTo(@"[OracleConnection]
DataSource = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = tor.nlmk)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = tor.nlmk)))
User = GANKOV_AV
Password = gankov_av
[Login10]
ConnectionName = Мой ноутбук
Server = 192.168.50.50
Type = 0
Account = 
DataBase = workwear_osmbt_copy
UserLogin = andrey
[AppUpdater]
SkipVersion = 2.6.1.0
[CardReader]
Address = 0000001:1
CardTypes = EF_MIFARE
[Login11]
ConnectionName = Тест
")); 
		}
	}
}
