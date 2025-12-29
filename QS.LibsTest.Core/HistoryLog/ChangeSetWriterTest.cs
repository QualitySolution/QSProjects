using System;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using QS.HistoryLog.Domain;
using QS.HistoryLog;
using QS.Testing.DB;

namespace QS.Test.HistoryLog {
	[TestFixture(TestOf = typeof(ChangeSetWriter))]
	public class ChangeSetWriterTest : MariaDbTestContainerSqlFixtureBase {
		public override async Task OneTimeSetUp() {
			await base.OneTimeSetUp();
			await PrepareDatabase(CreateHistoryLogSchema());
		}

		[Test]
		public async Task ChangeSetWriter_SaveAsyncTest() {
			//ARRANGE 
			using(var connection = CreateConnection(enableUserVariables: true)) {
				await connection.OpenAsync();
				await connection.ExecuteAsync($"USE `{DefaultDbName}`;");

				var changeSet = new SimpleChangeSet {
					ActionName = "UnitTestAction",
					UserId = 1,
					UserLogin = "test_user"
				};

				// Создаём 12 сущностей, чтобы при batch size = 5 получилось 3 batch-а (5 + 5 + 2)
				for(int i = 1; i <= 12; i++) {
					var entity = new SimpleChangedEntity {
						ChangeTime = DateTime.Now,
						Operation = EntityChangeOperation.Create,
						EntityClassName = "TestEntity",
						EntityId = 100 + i,
						EntityTitle = $"Test Entity {i}"
					};

					entity.Changes.Add(new SimpleFieldChange {
						Path = "Name",
						OldValue = $"Старое значение {i}",
						NewValue = $"Новое значение {i}",
						OldId = 1000 + i,
						NewId = 2000 + i,
						Type = FieldChangeType.Changed
					});

					changeSet.Entities.Add(entity);
				}

				//act
				// Используем маленький batch size для проверки записи в несколько batch-ей
				var writer = new ChangeSetWriter(GetConnectionString(), batchSize: 5);
				await writer.SaveAsync(changeSet);

				// assert
				var csCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM history_changeset;");
				Assert.AreEqual(1, csCount, "changeset count");

				var entitiesCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM history_changed_entities;");
				Assert.AreEqual(12, entitiesCount, "changed entities count");

				var changesCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM history_changes;");
				Assert.AreEqual(12, changesCount, "field changes count");
			}
		}
		
		private string CreateHistoryLogSchema() {
			return @"
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
DEFAULT CHARACTER SET = utf8mb4;


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
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4;

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
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4;

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
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4;

INSERT INTO users (id,name,login) VALUES (1,'test_user', '+7-965-590-24-11');
";
		}
	}
}
