using System;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using NUnit.Framework;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.HistoryLog.Domain;
using QS.HistoryLog;
using Testcontainers.MariaDb;

namespace QS.Test.HistoryLog {
	public class ChangeSetWriterTest {
		private MariaDbContainer _mariaDbContainer;
		private string connectionString;
		private const string DbName = "test_HistoryLog";

		[OneTimeSetUp]
		public async Task OneTimeSetUp() {
			_mariaDbContainer = new MariaDbBuilder()
				.WithDatabase(DbName)
				.Build();

			await _mariaDbContainer.StartAsync();
			connectionString = _mariaDbContainer.GetConnectionString();
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDown() {
			if (_mariaDbContainer != null) {
				await _mariaDbContainer.DisposeAsync();
			}
		}

		[Test]
		public async Task HistoryLog() {
			//ARRANGE 
			using(var connection = new MySqlConnection(connectionString)) {
				await connection.OpenAsync();
				await PrepareDatabase(connection);

				var changeSet = new ChangeSetDto {
					ActionName = "UnitTestAction",
					UserId = 1,
					UserLogin = "test_user"
				};

				var entity = new ChangedEntityDto {
					ChangeTime = DateTime.Now,
					Operation = EntityChangeOperation.Create,
					EntityClassName = "TestEntity",
					EntityId = 123,
					EntityTitle = "Test Entity Title"
				};

				entity.Changes.Add(new FieldChangeDto {
					Path = "Name",
					OldValue = "Куртка",
					NewValue = "Шапка",
					OldId = 123,
					NewId = 321,
					Type = FieldChangeType.Added
				});

				changeSet.Entities.Add(entity);

				//act
				var persister = new ChangeSetWriter(connectionString);
				persister.Save(changeSet);

				// assert
				var csCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM history_changeset;");
				Assert.AreEqual(1, csCount, "changeset count");

				var entitiesCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM history_changed_entities;");
				Assert.AreEqual(1, entitiesCount, "changed entities count");

				var changesCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM history_changes;");
				Assert.AreEqual(1, changesCount, "field changes count");
			}
		}

		[Test]
		public async Task HibernateTracker_SaveChangeSet() {
			//ARRANGE 
			using(var connection = new MySqlConnection(connectionString)) {
				await connection.OpenAsync();
				await PrepareDatabase(connection);

				QS.Project.Repositories.UserRepository.GetCurrentUserId = () => 1;

				var tracker = new HibernateTracker(connectionString);
				// получаем через рефлексию поле
				var changesField = typeof(QS.HistoryLog.HibernateTracker).GetField("changes", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				var list = (System.Collections.Generic.List<ChangedEntity>)changesField.GetValue(tracker);

				var entity = new TestDomainEntity { Id = 123, Name = "Test Entity" };

				var fc = new CovariantCollection<FieldChange>(){
					new FieldChange {
						Path = "Name",
						OldValue = "Куртка",
						NewValue = "Шапка",
						OldId = 123,
						NewId = 321,
						Type = FieldChangeType.Added
					}
				};

				var changedEntity = new ChangedEntity(EntityChangeOperation.Create, entity, fc);
				list.Add(changedEntity);

				//act
				var uow = new TestUnitOfWork();

				tracker.SaveChangeSet(uow);

				// assert
				var csCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM history_changeset;");
				Assert.AreEqual(1, csCount, "changeset count");

				var entitiesCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM history_changed_entities;");
				Assert.AreEqual(1, entitiesCount, "changed entities count");

				var changesCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM history_changes;");
				Assert.AreEqual(1, changesCount, "field changes count");
			}
		}

		async Task PrepareDatabase(MySqlConnection connection) {
			await connection.ExecuteAsync($"DROP DATABASE IF EXISTS {DbName};");
			await connection.ExecuteAsync($"CREATE DATABASE {DbName};");
			await connection.ExecuteAsync($"USE {DbName};");
			string query = @"
SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

CREATE TABLE IF NOT EXISTS `users` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  `login` VARCHAR(45) NOT NULL,
  `deactivated` TINYINT(1) NOT NULL DEFAULT 0,
  `email` VARCHAR(60) NULL DEFAULT NULL,
  `description` TEXT NULL DEFAULT NULL,
  `admin` TINYINT(1) NOT NULL DEFAULT FALSE,
  `can_delete` TINYINT(1) NOT NULL DEFAULT TRUE,
  `can_accounting_settings` TINYINT(1) NOT NULL DEFAULT TRUE,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


CREATE TABLE IF NOT EXISTS `history_changeset` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `user_login` VARCHAR(50) NULL DEFAULT NULL,
  `action_name` VARCHAR(100) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_history_changeset_1_idx` (`user_id` ASC),
  INDEX `history_changeset_login_idx` (`user_login` ASC),
  CONSTRAINT `fk_history_changeset_1`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;

CREATE TABLE IF NOT EXISTS `history_changed_entities` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `changeset_id` INT(10) UNSIGNED NOT NULL,
  `datetime` DATETIME NOT NULL,
  `operation` ENUM('Create', 'Change', 'Delete') NOT NULL,
  `entity_class` VARCHAR(45) NOT NULL,
  `entity_id` INT(10) UNSIGNED NOT NULL,
  `entity_title` VARCHAR(200) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `ix_changeset_operation` (`operation` ASC),
  INDEX `fk_history_changed_entities_1_idx` (`changeset_id` ASC),
  INDEX `history_changed_entities_datetime_IDX` USING BTREE (`datetime`),
  INDEX `history_changed_entities_entity_class_IDX` USING BTREE (`entity_class`),
  INDEX `history_changed_entities_entity_id_IDX` USING BTREE (`entity_id`),
  CONSTRAINT `fk_history_changed_entities_1`
    FOREIGN KEY (`changeset_id`)
    REFERENCES `history_changeset` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;

CREATE TABLE IF NOT EXISTS `history_changes` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `changed_entity_id` INT(10) UNSIGNED NOT NULL,
  `type` ENUM('Added', 'Changed', 'Removed', 'Unchanged') NOT NULL DEFAULT 'Unchanged',
  `field_name` VARCHAR(80) NOT NULL,
  `old_value` TEXT NULL DEFAULT NULL,
  `old_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `new_value` TEXT NULL DEFAULT NULL,
  `new_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_change_entity_id_idx` (`changed_entity_id` ASC),
  INDEX `index_changesets_path` (`field_name` ASC),
  INDEX `history_changes_old_id_IDX` USING BTREE (`old_id`),
  INDEX `history_changes_new_id_IDX` USING BTREE (`new_id`),
  INDEX `history_changes_old_value_IDX` USING BTREE (`old_value`(100)),
  INDEX `history_changes_new_value_IDX` USING BTREE (`new_value`(100)),
  CONSTRAINT `fk_change_entity_id`
    FOREIGN KEY (`changed_entity_id`)
    REFERENCES `history_changed_entities` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;
";

			await connection.ExecuteAsync(query, commandTimeout: 120);

			await connection.ExecuteAsync("INSERT INTO users (id,name,login) VALUES (1,'test_user', '+7-965-590-24-11');");
		}

		// Вспомогательные типы для теста
		[QS.HistoryLog.HistoryTrace]
		class TestDomainEntity : QS.DomainModel.Entity.IDomainObject {
			public int Id { get; set; }
			public string Name { get; set; }
		}

		class TestUnitOfWork : QS.DomainModel.UoW.IUnitOfWork {
			public QS.DomainModel.UoW.UnitOfWorkTitle ActionTitle { get; } = new QS.DomainModel.UoW.UnitOfWorkTitle("test", "member", "file.cs", 1);
			public NHibernate.ISession Session => null;
			public object RootObject => null;
			public bool IsNew => false;
			public bool IsAlive => true;
			public bool HasChanges => false;
			public void Save<TEntity>(TEntity entity, bool orUpdate = true) where TEntity : QS.DomainModel.Entity.IDomainObject { }
			public void Save() { }
			public void TrySave(object entity, bool orUpdate = true) { }
			public void TryDelete(object entity) { }
			public System.Linq.IQueryable<T> GetAll<T>() where T : QS.DomainModel.Entity.IDomainObject { throw new NotImplementedException(); }
			public NHibernate.IQueryOver<T, T> Query<T>() where T : class { throw new NotImplementedException(); }
			public NHibernate.IQueryOver<T, T> Query<T>(System.Linq.Expressions.Expression<System.Func<T>> alias) where T : class { throw new NotImplementedException(); }
			public T GetById<T>(int id) where T : QS.DomainModel.Entity.IDomainObject { throw new NotImplementedException(); }
			public T GetInSession<T>(T origin) where T : class, QS.DomainModel.Entity.IDomainObject { throw new NotImplementedException(); }
			public System.Collections.Generic.IList<T> GetById<T>(int[] ids) where T : class, QS.DomainModel.Entity.IDomainObject { throw new NotImplementedException(); }
			public System.Collections.Generic.IList<T> GetById<T>(System.Collections.Generic.IEnumerable<int> ids) where T : class, QS.DomainModel.Entity.IDomainObject { throw new NotImplementedException(); }
			public object GetById(Type clazz, int id) { throw new NotImplementedException(); }
			public void Commit() { }
			public void Delete<TEntity>(TEntity entity) where TEntity : QS.DomainModel.Entity.IDomainObject { }
			public event EventHandler<EntityUpdatedEventArgs> SessionScopeEntitySaved;
			public void RaiseSessionScopeEntitySaved(object[] entities) { }
			public void Dispose() { }
		}
	}
}
