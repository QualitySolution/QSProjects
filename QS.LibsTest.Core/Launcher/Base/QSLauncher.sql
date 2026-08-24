CREATE TABLE `products` (
	`id` TINYINT UNSIGNED NOT NULL,
	`name` VARCHAR(64) NOT NULL,
	PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `server_users` (
	`id` INT NOT NULL AUTO_INCREMENT,
	`product_id` TINYINT UNSIGNED NULL,
	`login` VARCHAR(80) NOT NULL,
	`name` VARCHAR(255) NULL,
	`email` VARCHAR(255) NULL,
	`phone` VARCHAR(50) NULL,
	`is_admin` TINYINT(1) NOT NULL DEFAULT 0,
	`disabled` TINYINT(1) NOT NULL DEFAULT 0,
	PRIMARY KEY (`id`),
	UNIQUE KEY `uk_server_users_login` (`login`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `bases` (
	`id` INT NOT NULL AUTO_INCREMENT,
	`product_id` TINYINT UNSIGNED NOT NULL,
	`base_name` VARCHAR(64) NOT NULL,
	`base_title` VARCHAR(255) NULL,
	`version` VARCHAR(32) NULL,
	`disabled` TINYINT(1) NOT NULL DEFAULT 0,
	PRIMARY KEY (`id`),
	UNIQUE KEY `uk_bases_identity` (`product_id`, `base_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `base_update_rights` (
	`user_id` INT NOT NULL,
	`base_id` INT NOT NULL,
	`can_update` TINYINT(1) NOT NULL DEFAULT 0,
	PRIMARY KEY (`user_id`, `base_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
