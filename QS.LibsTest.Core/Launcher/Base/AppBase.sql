-- Скелет прикладной базы продукта - ровно то, что от неё нужно лаунчеру.
--
-- base_parameters читает ParametersService: по ним провайдер опознаёт, что база принадлежит
-- продукту, и достаёт заголовок с версией.
-- users - таблица пользователей самой базы, в неё лаунчер пишет профиль и снимает доступ
-- через deactivated.
--
-- name NOT NULL без дефолта - как в реальных базах продуктов; на этом держится проверка
-- того, что синхронизация подставляет логин вместо пустого имени.

CREATE TABLE `base_parameters` (
	`name` VARCHAR(64) NOT NULL,
	`str_value` VARCHAR(255) NULL,
	PRIMARY KEY (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `users` (
	`id` INT NOT NULL AUTO_INCREMENT,
	`login` VARCHAR(80) NOT NULL,
	`name` VARCHAR(255) NOT NULL,
	`email` VARCHAR(255) NULL,
	`description` TEXT NULL,
	`admin` TINYINT(1) NOT NULL DEFAULT 0,
	`deactivated` TINYINT(1) NOT NULL DEFAULT 0,
	PRIMARY KEY (`id`),
	UNIQUE KEY `uk_users_login` (`login`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
