-- Схема метабазы QSLauncher для интеграционных тестов.
--
-- В репозитории приложения DDL метабазы нет намеренно: это серверная метабаза, аналог облака,
-- она разворачивается вне лаунчера. Здесь схема воспроизведена по тому, что читает и пишет код
-- QS.DbManagement.MariaDb.QSLauncher - и заодно служит её описанием.
--
-- Внешних ключей нет специально: тестам нужно уметь собирать заведомо рассогласованные состояния
-- (доступ на несуществующую базу, база без записи в метабазе и т.п.).
--
-- Индексы расставлены аккуратно: LauncherColumnMapper.KeyColumns берёт ВСЕ проиндексированные
-- колонки и исключает их из ON DUPLICATE KEY UPDATE в UpsertBases. Поэтому обновляемые при
-- синхронизации колонки (real_name, base_title, version, base_guid, disabled) индексировать нельзя.

CREATE TABLE `accounts` (
	`id` INT NOT NULL AUTO_INCREMENT,
	`login` VARCHAR(80) NOT NULL,
	`name` VARCHAR(255) NULL,
	PRIMARY KEY (`id`),
	UNIQUE KEY `uk_accounts_login` (`login`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `server_users` (
	`id` INT NOT NULL AUTO_INCREMENT,
	`account_id` INT NOT NULL,
	`product_id` TINYINT UNSIGNED NULL,
	`login` VARCHAR(80) NOT NULL,
	`password` VARCHAR(255) NULL,
	`name` VARCHAR(255) NULL,
	`email` VARCHAR(255) NULL,
	`phone` VARCHAR(50) NULL,
	`post` VARCHAR(255) NULL,
	`comment` TEXT NULL,
	`is_account_admin` TINYINT(1) NOT NULL DEFAULT 0,
	`disabled` TINYINT(1) NOT NULL DEFAULT 0,
	PRIMARY KEY (`id`),
	UNIQUE KEY `uk_server_users_login` (`login`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `bases` (
	`id` INT NOT NULL AUTO_INCREMENT,
	`account_id` INT NOT NULL,
	`product_id` TINYINT UNSIGNED NOT NULL,
	`base_name` VARCHAR(64) NOT NULL,
	`real_name` VARCHAR(64) NULL,
	`base_title` VARCHAR(255) NULL,
	`version` VARCHAR(32) NULL,
	-- не CHAR(36): такую колонку MySqlConnector отдаёт как System.Guid, а код читает её строкой
	`base_guid` VARCHAR(36) NULL,
	`disabled` TINYINT(1) NOT NULL DEFAULT 0,
	PRIMARY KEY (`id`),
	-- ключ upsert'а синхронизации: база опознаётся по аккаунту, продукту и имени
	UNIQUE KEY `uk_bases_identity` (`account_id`, `product_id`, `base_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `base_access` (
	`user_id` INT NOT NULL,
	`base_id` INT NOT NULL,
	`admin` TINYINT(1) NOT NULL DEFAULT 0,
	`read_only` TINYINT(1) NOT NULL DEFAULT 0,
	PRIMARY KEY (`user_id`, `base_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Зависимости базы: при удалении записи о базе чистятся раньше неё (BaseDependencies)
CREATE TABLE `sessions` (
	`id` INT NOT NULL AUTO_INCREMENT,
	`base_id` INT NOT NULL,
	`user_id` INT NULL,
	`opened` DATETIME NULL,
	PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `api_tokens` (
	`id` INT NOT NULL AUTO_INCREMENT,
	`base_id` INT NOT NULL,
	`token` VARCHAR(255) NULL,
	PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
