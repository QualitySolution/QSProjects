using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using QS.DbManagement;
using QS.Dialog;
using QS.Launcher;

namespace QS.Test.Launcher {
	[TestFixture(TestOf = typeof(Configurator))]
	public class ConfiguratorTest {
		
		#region Helpers
		
		private string tempDir;
		private string testConfigFile;
		private LauncherOptions options;
		private IInteractiveMessage interactive;
		private readonly List<ConnectionTypeBase> connectionTypes = new List<ConnectionTypeBase>();
		
		[SetUp]
		public void Setup() {
			tempDir = Path.Combine(Path.GetTempPath(), "configurator_test_" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(tempDir);
			testConfigFile = Path.Combine(tempDir, "connections.json");
			
			options = new LauncherOptions {
				ConnectionsJsonFileName = testConfigFile
			};
			
			interactive = Substitute.For<IInteractiveMessage>();

			// Для тестов миграции/чтения нужны типы, иначе Configurator не создаст Connection
			connectionTypes.Add(new TestConnectionType("MariaDB"));
			connectionTypes.Add(new TestConnectionType("QSCloud"));
		}
		
		[TearDown]
		public void Cleanup() {
			if(Directory.Exists(tempDir)) {
				// Ждем освобождения файлов
				for(int i = 0; i < 5; i++) {
					try {
						Directory.Delete(tempDir, true);
						break;
					}
					catch(IOException) {
						Thread.Sleep(100);
					}
				}
			}
		}
		
		private void CreateTestConfig(string content) {
			File.WriteAllText(testConfigFile, content);
		}
		
		#endregion
		
		[Test(Description = "Проверка создания файла конфигурации при первом запуске")]
		public void SaveConnections_CreatesNewFile_WhenNoFileExists() {
			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = new List<Connection>();
			
			configurator.SaveConnections(connections);
			
			Assert.That(File.Exists(testConfigFile), Is.True, "Файл конфигурации должен быть создан");
		}
		
		[Test(Description = "Проверка сохранения и чтения подключений")]
		public void SaveAndReadConnections_PreservesData() {
			var configurator = new Configurator(options, interactive, connectionTypes);
			
			// Создаем тестовые данные
			CreateTestConfig(@"[]");
			
			var connections = configurator.ReadConnections();
			
			Assert.That(connections, Is.Not.Null, "Список подключений должен быть создан");
			
			// Сохраняем обратно
			configurator.SaveConnections(connections);
			
			// Читаем снова
			var reloadedConnections = configurator.ReadConnections();
			
			Assert.That(reloadedConnections, Is.Not.Null, "После пересохранения список должен существовать");
		}
		
		[Test(Description = "Проверка что временный файл удаляется после успешной записи")]
		public void SaveConnections_RemovesTempFile_AfterSuccess() {
			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = new List<Connection>();
			
			configurator.SaveConnections(connections);
			
			Assert.That(File.Exists(testConfigFile + ".tmp"), Is.False, "Временный файл должен быть удален");
		}
		
		[Test(Description = "Проверка что файл блокировки удаляется после записи")]
		public void SaveConnections_RemovesLockFile_AfterCompletion() {
			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = new List<Connection>();
			
			configurator.SaveConnections(connections);
			
			Assert.That(File.Exists(testConfigFile + ".lock"), Is.False, "Файл блокировки должен быть удален");
		}
		
		[Test(Description = "Проверка восстановления из backup при пустом основном файле")]
		public void ReadConnections_RestoresFromBackup_WhenMainFileIsEmpty() {
			// Создаем backup с пустым массивом
			CreateTestConfig(@"[]");
			File.Copy(testConfigFile, testConfigFile + ".backup", true);
			
			// Создаем пустой основной файл
			File.WriteAllText(testConfigFile, "");
			
			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = configurator.ReadConnections();
			
			Assert.That(connections, Is.Not.Null, "Конфигурация должна быть восстановлена из backup");
			interactive.Received().ShowMessage(
				ImportanceLevel.Warning, 
				Arg.Is<string>(s => s.Contains("восстановлен из резервной копии")), 
				Arg.Any<string>()
			);
		}
		
		[Test(Description = "Проверка атомарности записи - основной файл не должен быть пустым при сбое")]
		public void SaveConnections_DoesNotCorruptMainFile_OnInterruption() {
			// Создаем существующий файл с данными
			CreateTestConfig(@"[]");
			
			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = new List<Connection>();
			
			// Нормальное сохранение
			configurator.SaveConnections(connections);
			
			// Проверяем что файл не пустой
			var fileInfo = new FileInfo(testConfigFile);
			Assert.That(fileInfo.Length, Is.GreaterThan(0), "Файл конфигурации не должен быть пустым");
		}
		
		[Test(Description = "Проверка защиты от конкурентного доступа")]
		public void SaveConnections_PreventsRaceCondition_WithFileLock() {
			var configurator1 = new Configurator(options, interactive, connectionTypes);
			var configurator2 = new Configurator(options, interactive, connectionTypes);
			
			var connections = new List<Connection>();
			
			// Первая запись создает блокировку
			var task1 = Task.Run(() => {
				for(int i = 0; i < 5; i++) {
					configurator1.SaveConnections(connections);
					Thread.Sleep(10);
				}
			});
			
			var task2 = Task.Run(() => {
				Thread.Sleep(5); // Небольшая задержка чтобы первый поток успел создать блокировку
				for(int i = 0; i < 5; i++) {
					configurator2.SaveConnections(connections);
					Thread.Sleep(10);
				}
			});
			
			Assert.DoesNotThrow(() => Task.WaitAll(task1, task2), 
				"Конкурентная запись не должна приводить к исключениям");
			
			// Проверяем что файл не поврежден
			Assert.That(File.Exists(testConfigFile), Is.True);
			var fileInfo = new FileInfo(testConfigFile);
			Assert.That(fileInfo.Length, Is.GreaterThan(0), "Файл не должен быть пустым после конкурентной записи");
		}
		
		[Test(Description = "Проверка что файл имеет корректный JSON формат после записи")]
		public void SaveConnections_CreatesValidJsonFile() {
			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = new List<Connection>();
			
			configurator.SaveConnections(connections);
			
			// Пытаемся прочитать как JSON
			Assert.DoesNotThrow(() => {
				var content = File.ReadAllText(testConfigFile);
				System.Text.Json.JsonDocument.Parse(content);
			}, "Файл должен содержать валидный JSON");
		}
		
		[Test(Description = "Проверка чтения пустого списка подключений")]
		public void ReadConnections_ReturnsEmptyList_WhenFileContainsEmptyArray() {
			CreateTestConfig("[]");
			
			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = configurator.ReadConnections();
			
			Assert.That(connections, Is.Not.Null, "Список не должен быть null");
			Assert.That(connections.Count, Is.EqualTo(0), "Список должен быть пустым");
		}
		
		[Test(Description = "Проверка что при отсутствии файла создается дефолтная конфигурация")]
		public void ReadConnections_CreatesDefaultConfig_WhenNoFilesExist() {
			options.MakeDefaultConnections = () => new List<Dictionary<string, string>>();
			
			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = configurator.ReadConnections();
			
			Assert.That(connections, Is.Not.Null, "Должна быть создана дефолтная конфигурация");
		}
		
		[Test(Description = "Проверка восстановления при поврежденном JSON")]
		public void ReadConnections_RestoresFromBackup_WhenJsonIsCorrupted() {
			// Создаем валидный backup
			CreateTestConfig(@"[]");
			File.Copy(testConfigFile, testConfigFile + ".backup", true);
			
			// Портим основной файл
			File.WriteAllText(testConfigFile, "{ invalid json [");
			
			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = configurator.ReadConnections();
			
			Assert.That(connections, Is.Not.Null, "Должна быть восстановлена конфигурация из backup");
		}
		
		[Test(Description = "Проверка множественных последовательных сохранений")]
		public void SaveConnections_HandlesMultipleSequentialWrites() {
			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = new List<Connection>();
			
			// Множественные сохранения
			for(int i = 0; i < 10; i++) {
				Assert.DoesNotThrow(() => configurator.SaveConnections(connections), 
					$"Сохранение #{i} не должно вызывать исключение");
			}
			
			// Проверяем что файл в порядке
			Assert.That(File.Exists(testConfigFile), Is.True);
			var fileInfo = new FileInfo(testConfigFile);
			Assert.That(fileInfo.Length, Is.GreaterThan(0));
		}
		
		[Test(Description = "Проверка создания директории если она не существует")]
		public void SaveConnections_CreatesDirectory_WhenItDoesNotExist() {
			var subDir = Path.Combine(tempDir, "subdir", "nested");
			var nestedConfigFile = Path.Combine(subDir, "connections.json");
			options.ConnectionsJsonFileName = nestedConfigFile;
			
			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = new List<Connection>();
			
			configurator.SaveConnections(connections);
			
			Assert.That(Directory.Exists(subDir), Is.True, "Вложенные директории должны быть созданы");
			Assert.That(File.Exists(nestedConfigFile), Is.True, "Файл должен быть создан в новой директории");
		}
		
		[Test(Description = "Проверка что backup создаётся со 2-го сохранения и не удаляется после успешной записи")]
		public void SaveConnections_CreatesAndKeepsBackupFile_AfterSecondSave() {
			// Чтобы backup создавался, нужен предыдущий файл (не пустой)
			CreateTestConfig("[]");

			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = new List<Connection>();

			// 1-е сохранение: существующий файл копируется в backup
			configurator.SaveConnections(connections);
			Assert.That(File.Exists(testConfigFile + ".backup"), Is.True,
				"Backup должен быть создан при наличии предыдущей версии файла");

			// 2-е сохранение: backup должен остаться
			configurator.SaveConnections(connections);
			Assert.That(File.Exists(testConfigFile + ".backup"), Is.True,
				"Backup не должен удаляться после успешного сохранения");
		}

		[Test(Description = "Проверка что backup содержит предыдущую версию файла")]
		public void SaveConnections_BackupContainsPreviousVersion() {
			// Делаем main отличимым от результата SaveConnections (который запишет пустой массив)
			var previousJson = "[{\"x\":1}]";
			CreateTestConfig(previousJson);

			var configurator = new Configurator(options, interactive, connectionTypes);
			configurator.SaveConnections(new List<Connection>());

			var backupPath = testConfigFile + ".backup";
			Assert.That(File.Exists(backupPath), Is.True, "Backup должен быть создан");
			Assert.That(File.ReadAllText(backupPath), Is.EqualTo(previousJson),
				"Backup должен содержать предыдущую сохранённую версию (байт-в-байт)");

			Assert.That(File.ReadAllText(testConfigFile).Trim(), Is.EqualTo("[]"),
				"Основной файл должен быть обновлён новой версией");
		}
		
		[Test(Description = "Миграция: загрузка подключений из старого INI-конфига (OldConfigFilename)")]
		public void ReadConnections_LoadsFromOldIniConfig_WhenNoJsonExists() {
			var oldIniPath = Path.Combine(tempDir, "old_config.ini");
			options.OldConfigFilename = oldIniPath;

			File.WriteAllText(oldIniPath,
				"[Login]\n" +
				"UserLogin=andrey\n" +
				"Server=\n" +
				"ConnectionName=Облако\n" +
				"Type=1\n" +
				"Account=qsolution\n" +
				"DataBase=ClientManager\n" +
				"\n" +
				"[Default]\n" +
				"ConnectionName=Облако\n" +
				"\n" +
				"[AppUpdater]\n" +
				"SkipVersion=1.3.3\n");

			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = configurator.ReadConnections();

			Assert.That(connections, Is.Not.Null);
			Assert.That(connections.Count, Is.EqualTo(1), "Должно быть загружено 1 подключение из INI");

			var defs = connections[0].GetConfigDefinitions();
			Assert.That(defs["Type"], Is.EqualTo("QSCloud"));
			Assert.That(defs["Login"], Is.EqualTo("andrey"));
			Assert.That(defs["Account"], Is.EqualTo("qsolution"));
			Assert.That(defs["Title"], Does.Contain("qsolution"));
			Assert.That(defs["Title"], Does.Contain("andrey"));

			// Дальше smoke-check что сохраняется обратно
			Assert.DoesNotThrow(() => configurator.SaveConnections(connections));
			Assert.That(File.Exists(testConfigFile), Is.True);
		}

		[Test(Description = "Миграция: для MariaDB подключения к одному Server+UserLogin объединяются (3 секции -> 2 подключения)")]
		public void ReadConnections_MariaDbConnectionsWithSameServerAndLogin_AreMerged() {
			var oldIniPath = Path.Combine(tempDir, "old_config.ini");
			options.OldConfigFilename = oldIniPath;

			// Важно:
			// - Type=0 => MariaDB
			// - Уникальность определяется мигратором через Hash: [MariaDB]{UserLogin}@{Server}
			// - Login11 и Login16 одинаковые (root@plutus...) => одно подключение
			// - Login17 другой пользователь (andrey@plutus...) => второе подключение
			File.WriteAllText(oldIniPath,
				"[Login11]\n" +
				"ConnectionName=2.7\n" +
				"Server=plutus.srv.qsolution.ru\n" +
				"Type=0\n" +
				"DataBase=workwear_dev27\n" +
				"UserLogin=root\n" +
				"\n" +
				"[Login16]\n" +
				"ConnectionName=Плутос тест\n" +
				"Server=plutus.srv.qsolution.ru\n" +
				"Type=0\n" +
				"DataBase=test_PersonalAreaService\n" +
				"UserLogin=root\n" +
				"\n" +
				"[Login17]\n" +
				"ConnectionName=2.8 релиз\n" +
				"Server=plutus.srv.qsolution.ru\n" +
				"Type=0\n" +
				"DataBase=workwear_28\n" +
				"UserLogin=andrey\n");

			var configurator = new Configurator(options, interactive, connectionTypes);
			var connections = configurator.ReadConnections();

			Assert.That(connections, Is.Not.Null);
			Assert.That(connections.Count, Is.EqualTo(2), "Ожидаем 2 подключения после объединения");

			// Проверим что оба подключения типа MariaDB и с нужными логинами/сервером
			var defs = new List<Dictionary<string, string>> {
				connections[0].GetConfigDefinitions(),
				connections[1].GetConfigDefinitions()
			};

			Assert.That(defs.TrueForAll(d => d["Type"] == "MariaDB"));
			Assert.That(defs.TrueForAll(d => d["Server"] == "plutus.srv.qsolution.ru"));
			CollectionAssert.AreEquivalent(new[] { "root", "andrey" }, defs.ConvertAll(d => d["Login"]));

			// Smoke-check: сохранить обратно должно получиться
			Assert.DoesNotThrow(() => configurator.SaveConnections(connections));
			Assert.That(File.Exists(testConfigFile), Is.True);
		}

		private class TestConnectionType : ConnectionTypeBase {
			public TestConnectionType(string name) {
				ConnectionTypeName = name;
				Title = $"Test {name}";

				// Минимальный набор параметров чтобы Connection заполнил CustomParameters
				Parameters.Add(new ConnectionParameter("Login", "Login"));
				Parameters.Add(new ConnectionParameter("Account", "Account"));
				Parameters.Add(new ConnectionParameter("Server", "Server"));
			}

			public override bool CanConnect(IEnumerable<ConnectionParameterValue> parameters) => true;

			public override IDbProvider CreateProvider(IList<ConnectionParameterValue> parameters, string password = null)
				=> Substitute.For<IDbProvider>();
		}
	}
}
