-- phpMyAdmin SQL Dump
-- version 5.0.4deb2~bpo10+1
-- https://www.phpmyadmin.net/
--
-- Host: demeter.srv.qsolution.ru
-- Generation Time: Apr 30, 2026 at 01:52 PM
-- Server version: 10.3.39-MariaDB-0+deb10u2
-- PHP Version: 7.3.31-1~deb10u7

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `QSService`
--

-- --------------------------------------------------------

--
-- Table structure for table `accounts`
--

CREATE TABLE `accounts` (
  `id` int(10) UNSIGNED NOT NULL,
  `login` varchar(20) NOT NULL,
  `client_id` int(10) UNSIGNED DEFAULT NULL,
  `customer` varchar(50) NOT NULL,
  `email` varchar(50) DEFAULT NULL,
  `paid_until` date DEFAULT NULL,
  `notify_by_days` int(11) DEFAULT NULL COMMENT 'Уведомить за Н дней до окончания',
  `bases_limit` int(10) UNSIGNED NOT NULL DEFAULT 1,
  `users_limit` int(10) UNSIGNED NOT NULL DEFAULT 3,
  `space_limit` int(10) UNSIGNED NOT NULL DEFAULT 500,
  `deactivated` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `api_tokens`
--

CREATE TABLE `api_tokens` (
  `id` int(10) UNSIGNED NOT NULL,
  `base_id` int(10) UNSIGNED NOT NULL,
  `token` char(36) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `bases`
--

CREATE TABLE `bases` (
  `id` int(10) UNSIGNED NOT NULL,
  `account_id` int(10) UNSIGNED NOT NULL,
  `server_id` int(10) UNSIGNED NOT NULL,
  `base_title` varchar(64) DEFAULT NULL COMMENT 'Русское название базы для пользователя',
  `base_name` varchar(45) NOT NULL,
  `product_id` int(10) UNSIGNED NOT NULL,
  `real_name` varchar(64) DEFAULT NULL,
  `base_guid` char(36) DEFAULT NULL,
  `wear_lk` tinyint(1) NOT NULL DEFAULT 0,
  `number_of_lk_client` int(11) DEFAULT 0,
  `claims_lk` tinyint(1) NOT NULL DEFAULT 0,
  `postomats` tinyint(1) NOT NULL DEFAULT 0,
  `catalog` tinyint(1) NOT NULL DEFAULT 0,
  `comments` text DEFAULT NULL,
  `ratings` tinyint(1) NOT NULL DEFAULT 0,
  `appointment_lk` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'включение предварительной записи',
  `washing_lk` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Включение отображения стирки в мобильном кабинете',
  `speccoin_lk` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Включение функциональности спецкойнов',
  `size_editing_lk` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Редактирование размеров в мобилке',
  `size_editing_days_before` int(10) UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Запрет изменения размеров за указанное количество дней до выдачи.',
  `postomat_email_notification` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Дублируют ли постоматы уведомления на Email.',
  `stock_availability_enable` tinyint(1) NOT NULL DEFAULT 0,
  `stock_availability_warehouse_id` int(10) UNSIGNED DEFAULT NULL COMMENT 'id склада по которому показывать наличие',
  `choice_nomenclature_lk` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Выбор номенклатур сотрудником'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `bases_scripts`
--

CREATE TABLE `bases_scripts` (
  `id` int(10) UNSIGNED NOT NULL,
  `product_id` int(10) UNSIGNED NOT NULL,
  `start_version` varchar(15) DEFAULT NULL,
  `end_version` varchar(15) NOT NULL,
  `script` mediumtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `base_access`
--

CREATE TABLE `base_access` (
  `id` int(10) UNSIGNED NOT NULL,
  `user_id` int(10) UNSIGNED NOT NULL,
  `base_id` int(10) UNSIGNED NOT NULL,
  `admin` tinyint(1) NOT NULL DEFAULT 0,
  `read_only` tinyint(1) NOT NULL DEFAULT 0,
  `torpedo` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'База доступна в панели инструментов'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `base_parameters`
--

CREATE TABLE `base_parameters` (
  `name` varchar(20) NOT NULL,
  `str_value` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `bug_reports`
--

CREATE TABLE `bug_reports` (
  `id` int(10) UNSIGNED NOT NULL,
  `created` datetime DEFAULT NULL,
  `last_update` datetime DEFAULT NULL,
  `product_id` int(10) UNSIGNED NOT NULL,
  `edition` varchar(20) DEFAULT NULL,
  `version` varchar(16) NOT NULL,
  `fixed_in_version` varchar(16) DEFAULT NULL COMMENT 'Версия в которой баг пофикшен',
  `message` varchar(2000) DEFAULT NULL,
  `stack_trace` text DEFAULT NULL,
  `description` text DEFAULT NULL,
  `email` varchar(600) DEFAULT NULL,
  `count` int(10) UNSIGNED DEFAULT 1,
  `status` enum('New','InWork','NeedInfo','Rejected','Later','Known','Unreproducable','EndOfLife','Done') DEFAULT 'New',
  `comments` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `bug_reports_messages`
--

CREATE TABLE `bug_reports_messages` (
  `id` int(10) UNSIGNED NOT NULL,
  `created` datetime NOT NULL,
  `bug_reports_id` int(10) UNSIGNED NOT NULL,
  `email` varchar(254) DEFAULT NULL,
  `user_name` varchar(60) DEFAULT NULL,
  `messages` text DEFAULT NULL,
  `db_name` varchar(60) DEFAULT NULL,
  `report_type` enum('User','Automatic','Known') NOT NULL DEFAULT 'User',
  `log_file` mediumtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `clients`
--

CREATE TABLE `clients` (
  `id` int(10) UNSIGNED NOT NULL,
  `name` varchar(300) NOT NULL,
  `email` varchar(45) DEFAULT NULL,
  `email_notifications` varchar(200) DEFAULT NULL COMMENT 'Адреса для уведомлений',
  `city` varchar(45) DEFAULT NULL,
  `comments` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `cloud_users`
--

CREATE TABLE `cloud_users` (
  `id` int(10) UNSIGNED NOT NULL,
  `login` varchar(20) NOT NULL,
  `name` varchar(80) DEFAULT NULL,
  `password` varchar(81) NOT NULL,
  `disabled` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Пользователь отключен',
  `is_account_admin` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Администратор учетной записи',
  `post` varchar(200) DEFAULT NULL COMMENT 'Должность',
  `phone` varchar(16) DEFAULT NULL COMMENT 'Телефон',
  `email` varchar(60) DEFAULT NULL,
  `account_id` int(10) UNSIGNED NOT NULL,
  `multi_ip` tinyint(1) NOT NULL DEFAULT 0,
  `client_id` int(10) UNSIGNED DEFAULT NULL,
  `comment` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `products`
--

CREATE TABLE `products` (
  `id` int(10) UNSIGNED NOT NULL,
  `name` varchar(45) NOT NULL,
  `internal_name` varchar(45) NOT NULL,
  `not_support_ver_regexp` varchar(45) DEFAULT NULL,
  `telegram_notify` varchar(50) DEFAULT NULL COMMENT 'Отправлять уведомления о новых ошибках в чат'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `product_editions`
--

CREATE TABLE `product_editions` (
  `id` int(10) UNSIGNED NOT NULL,
  `product_id` int(10) UNSIGNED NOT NULL,
  `code_number` int(10) UNSIGNED DEFAULT NULL COMMENT 'Номер редакции, внутри продукта',
  `code_name` varchar(10) DEFAULT NULL COMMENT 'Кодовое имя редакции.',
  `name` varchar(100) DEFAULT NULL COMMENT 'Название редакции отображаемое для пользователя.'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `product_versions`
--

CREATE TABLE `product_versions` (
  `id` int(10) UNSIGNED NOT NULL,
  `product_id` int(10) UNSIGNED NOT NULL,
  `modification` varchar(25) DEFAULT NULL,
  `channel` enum('Current','Stable') NOT NULL DEFAULT 'Current',
  `disable` tinyint(1) NOT NULL DEFAULT 0,
  `version_major` int(10) UNSIGNED NOT NULL DEFAULT 0,
  `version_minor` int(10) UNSIGNED NOT NULL DEFAULT 0,
  `version_build` int(10) UNSIGNED NOT NULL DEFAULT 0,
  `version_revision` int(10) UNSIGNED NOT NULL DEFAULT 0,
  `date` date NOT NULL,
  `link_install` varchar(256) DEFAULT NULL,
  `link_news` varchar(256) DEFAULT NULL,
  `changes` text DEFAULT NULL,
  `db_update` enum('None','Required','BreakingChange') NOT NULL DEFAULT 'None',
  `comment` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `serial_numbers`
--

CREATE TABLE `serial_numbers` (
  `id` int(10) UNSIGNED NOT NULL,
  `client_id` int(10) UNSIGNED NOT NULL,
  `number` varchar(50) NOT NULL,
  `recall` tinyint(1) NOT NULL DEFAULT 0,
  `notify_by_days` int(11) DEFAULT NULL COMMENT 'Уведомить за Н дней до окончания',
  `active_until` date DEFAULT NULL,
  `serial_expiry_date` date DEFAULT NULL COMMENT 'Дата окончания действия серийного номера',
  `instance` int(10) UNSIGNED NOT NULL DEFAULT 1,
  `comment` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `servers`
--

CREATE TABLE `servers` (
  `id` int(10) UNSIGNED NOT NULL,
  `server_address` varchar(60) NOT NULL,
  `service_user` varchar(16) NOT NULL,
  `service_password` varchar(81) NOT NULL,
  `comment` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `sessions`
--

CREATE TABLE `sessions` (
  `id` int(10) UNSIGNED NOT NULL,
  `session_id` varchar(36) NOT NULL,
  `user_id` int(10) UNSIGNED DEFAULT NULL,
  `account_id` int(10) UNSIGNED NOT NULL,
  `base_id` int(10) UNSIGNED NOT NULL,
  `start_time` datetime NOT NULL,
  `end_time` datetime NOT NULL,
  `is_closed` tinyint(1) NOT NULL DEFAULT 0,
  `login` varchar(40) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `telemetry_statistics`
--

CREATE TABLE `telemetry_statistics` (
  `id` int(10) UNSIGNED NOT NULL,
  `last_update` datetime NOT NULL,
  `ip_address` varchar(39) DEFAULT NULL,
  `product` varchar(20) NOT NULL,
  `edition` varchar(20) DEFAULT NULL,
  `version` varchar(20) NOT NULL,
  `os` varchar(100) DEFAULT NULL,
  `net_framework` varchar(100) DEFAULT NULL,
  `is_demo` tinyint(1) NOT NULL DEFAULT 0,
  `app_edition` int(10) UNSIGNED DEFAULT NULL COMMENT 'Редакция программы',
  `base_employees` int(10) UNSIGNED DEFAULT NULL COMMENT 'Количество сотрудников в базе',
  `counter` mediumtext NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `update_info`
--

CREATE TABLE `update_info` (
  `id` int(10) UNSIGNED NOT NULL,
  `product` varchar(25) NOT NULL,
  `edition` varchar(25) DEFAULT NULL,
  `serial_number` varchar(45) DEFAULT NULL,
  `start_version_major` int(10) UNSIGNED DEFAULT 0,
  `start_version_minor` int(10) UNSIGNED DEFAULT 0,
  `start_version_build` int(10) UNSIGNED DEFAULT 0,
  `start_version_revision` int(10) UNSIGNED DEFAULT 0,
  `new_version_major` int(10) UNSIGNED DEFAULT 0,
  `new_version_minor` int(10) UNSIGNED DEFAULT 0,
  `new_version_build` int(10) UNSIGNED DEFAULT 0,
  `new_version_revision` int(10) UNSIGNED DEFAULT 0,
  `link_install` varchar(256) NOT NULL,
  `link_news` varchar(256) DEFAULT NULL,
  `use_common` tinyint(1) DEFAULT 0,
  `update_description` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `update_statistics`
--

CREATE TABLE `update_statistics` (
  `id` int(10) UNSIGNED NOT NULL,
  `product_id` int(10) UNSIGNED DEFAULT NULL,
  `edition` varchar(25) DEFAULT NULL,
  `serial_number` varchar(45) DEFAULT NULL,
  `client_version` varchar(16) NOT NULL,
  `new_version` varchar(16) DEFAULT NULL,
  `date` datetime NOT NULL DEFAULT utc_timestamp(),
  `client_ip` varchar(15) DEFAULT NULL,
  `channel` enum('Current','Stable') NOT NULL DEFAULT 'Current',
  `status` enum('NoUpdates','NeedUpdate','Expired','Recalled','LicenceNotFound') DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `id` int(10) UNSIGNED NOT NULL,
  `name` varchar(45) NOT NULL,
  `login` varchar(45) NOT NULL,
  `deactivated` tinyint(1) NOT NULL DEFAULT 0,
  `email` varchar(60) DEFAULT NULL,
  `description` text DEFAULT NULL,
  `admin` tinyint(1) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `accounts`
--
ALTER TABLE `accounts`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `login_UNIQUE` (`login`),
  ADD KEY `fk_accounts_1_idx` (`client_id`);

--
-- Indexes for table `api_tokens`
--
ALTER TABLE `api_tokens`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `token_UNIQUE` (`token`),
  ADD KEY `fk_api_tokens_1_idx` (`base_id`);

--
-- Indexes for table `bases`
--
ALTER TABLE `bases`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `idx1_bases` (`base_name`,`account_id`),
  ADD UNIQUE KEY `base_guid_UNIQUE` (`base_guid`),
  ADD KEY `fk1_bases_idx` (`account_id`),
  ADD KEY `fk2_bases_idx` (`server_id`),
  ADD KEY `fk3_bases_idx` (`product_id`);

--
-- Indexes for table `bases_scripts`
--
ALTER TABLE `bases_scripts`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk1_bases_scripts_idx` (`product_id`);

--
-- Indexes for table `base_access`
--
ALTER TABLE `base_access`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `base_access_idx` (`user_id`,`base_id`),
  ADD KEY `fk1_base_access_idx` (`user_id`),
  ADD KEY `fk1_base_access_idx1` (`base_id`);

--
-- Indexes for table `base_parameters`
--
ALTER TABLE `base_parameters`
  ADD PRIMARY KEY (`name`);

--
-- Indexes for table `bug_reports`
--
ALTER TABLE `bug_reports`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_bug_reports_1_idx` (`product_id`),
  ADD KEY `bug_reports_created` (`created`),
  ADD KEY `bug_reports_last_update` (`last_update`),
  ADD KEY `bug_reports_edition` (`edition`),
  ADD KEY `bug_reports_version` (`version`),
  ADD KEY `bug_reports_status` (`status`);

--
-- Indexes for table `bug_reports_messages`
--
ALTER TABLE `bug_reports_messages`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_bug_reports_messages_1_idx` (`bug_reports_id`);

--
-- Indexes for table `clients`
--
ALTER TABLE `clients`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `cloud_users`
--
ALTER TABLE `cloud_users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `idx1_users` (`login`,`account_id`),
  ADD KEY `fk1_users_idx` (`account_id`),
  ADD KEY `fk_cloud_users_1_idx` (`client_id`);

--
-- Indexes for table `products`
--
ALTER TABLE `products`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `internal_name_UNIQUE` (`internal_name`);

--
-- Indexes for table `product_editions`
--
ALTER TABLE `product_editions`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_product_id_idx` (`product_id`);

--
-- Indexes for table `product_versions`
--
ALTER TABLE `product_versions`
  ADD PRIMARY KEY (`id`),
  ADD KEY `_idx` (`product_id`);

--
-- Indexes for table `serial_numbers`
--
ALTER TABLE `serial_numbers`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `number_UNIQUE` (`number`),
  ADD KEY `fk_serial_numbers_1_idx` (`client_id`);

--
-- Indexes for table `servers`
--
ALTER TABLE `servers`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `sessions`
--
ALTER TABLE `sessions`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `session_id_UNIQUE` (`session_id`),
  ADD KEY `fk1_sessions_idx` (`account_id`),
  ADD KEY `fk2_sessions_idx` (`user_id`),
  ADD KEY `fk3_sessions_idx` (`base_id`),
  ADD KEY `end_time_idx` (`end_time`),
  ADD KEY `is_closed_idx` (`is_closed`);

--
-- Indexes for table `telemetry_statistics`
--
ALTER TABLE `telemetry_statistics`
  ADD PRIMARY KEY (`id`),
  ADD KEY `index_telemetry_statistics_last_update` (`last_update`),
  ADD KEY `index_telemetry_statistics_ip` (`ip_address`),
  ADD KEY `index_telemetry_statistics_product` (`product`),
  ADD KEY `index_telemetry_statistics_edition` (`edition`),
  ADD KEY `index_telemetry_statistics_version` (`version`),
  ADD KEY `inxex_telemetry_statistics_os` (`os`);

--
-- Indexes for table `update_info`
--
ALTER TABLE `update_info`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `update_statistics`
--
ALTER TABLE `update_statistics`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `id_UNIQUE` (`id`),
  ADD KEY `fk_update_statistics_1_idx` (`product_id`),
  ADD KEY `update_statistics_edition_idx` (`edition`),
  ADD KEY `update_statistics_serial_idx` (`serial_number`),
  ADD KEY `update_statistics_client_version_idx` (`client_version`),
  ADD KEY `update_statistics_ip_idx` (`client_ip`),
  ADD KEY `update_statistics_new_version_idx` (`new_version`),
  ADD KEY `update_statistics_date_idx` (`date`),
  ADD KEY `update_statistics_channel_idx` (`channel`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `accounts`
--
ALTER TABLE `accounts`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `api_tokens`
--
ALTER TABLE `api_tokens`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `bases`
--
ALTER TABLE `bases`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `bases_scripts`
--
ALTER TABLE `bases_scripts`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `base_access`
--
ALTER TABLE `base_access`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `bug_reports`
--
ALTER TABLE `bug_reports`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `bug_reports_messages`
--
ALTER TABLE `bug_reports_messages`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `clients`
--
ALTER TABLE `clients`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `cloud_users`
--
ALTER TABLE `cloud_users`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `products`
--
ALTER TABLE `products`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `product_editions`
--
ALTER TABLE `product_editions`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `product_versions`
--
ALTER TABLE `product_versions`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `serial_numbers`
--
ALTER TABLE `serial_numbers`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `servers`
--
ALTER TABLE `servers`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `sessions`
--
ALTER TABLE `sessions`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `telemetry_statistics`
--
ALTER TABLE `telemetry_statistics`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `update_info`
--
ALTER TABLE `update_info`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `update_statistics`
--
ALTER TABLE `update_statistics`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `accounts`
--
ALTER TABLE `accounts`
  ADD CONSTRAINT `fk_accounts_1` FOREIGN KEY (`client_id`) REFERENCES `clients` (`id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Constraints for table `api_tokens`
--
ALTER TABLE `api_tokens`
  ADD CONSTRAINT `fk_api_tokens_1` FOREIGN KEY (`base_id`) REFERENCES `bases` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `bases`
--
ALTER TABLE `bases`
  ADD CONSTRAINT `fk1_bases` FOREIGN KEY (`account_id`) REFERENCES `accounts` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `fk2_bases` FOREIGN KEY (`server_id`) REFERENCES `servers` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `fk3_bases` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `bases_scripts`
--
ALTER TABLE `bases_scripts`
  ADD CONSTRAINT `fk1_bases_scripts` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `base_access`
--
ALTER TABLE `base_access`
  ADD CONSTRAINT `fk1_base_access` FOREIGN KEY (`user_id`) REFERENCES `cloud_users` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `fk2_base_access` FOREIGN KEY (`base_id`) REFERENCES `bases` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `bug_reports`
--
ALTER TABLE `bug_reports`
  ADD CONSTRAINT `fk_bug_reports_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE ON UPDATE NO ACTION;

--
-- Constraints for table `bug_reports_messages`
--
ALTER TABLE `bug_reports_messages`
  ADD CONSTRAINT `fk_bug_reports_messages_1` FOREIGN KEY (`bug_reports_id`) REFERENCES `bug_reports` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `cloud_users`
--
ALTER TABLE `cloud_users`
  ADD CONSTRAINT `fk1_users` FOREIGN KEY (`account_id`) REFERENCES `accounts` (`id`) ON DELETE CASCADE ON UPDATE NO ACTION,
  ADD CONSTRAINT `fk_cloud_users_1` FOREIGN KEY (`client_id`) REFERENCES `clients` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `product_editions`
--
ALTER TABLE `product_editions`
  ADD CONSTRAINT `fk_product_id` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `product_versions`
--
ALTER TABLE `product_versions`
  ADD CONSTRAINT `fk_product_versions_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `serial_numbers`
--
ALTER TABLE `serial_numbers`
  ADD CONSTRAINT `fk_serial_numbers_1` FOREIGN KEY (`client_id`) REFERENCES `clients` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `sessions`
--
ALTER TABLE `sessions`
  ADD CONSTRAINT `fk1_sessions` FOREIGN KEY (`account_id`) REFERENCES `accounts` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `fk2_sessions` FOREIGN KEY (`user_id`) REFERENCES `cloud_users` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION,
  ADD CONSTRAINT `fk3_sessions` FOREIGN KEY (`base_id`) REFERENCES `bases` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Constraints for table `update_statistics`
--
ALTER TABLE `update_statistics`
  ADD CONSTRAINT `fk_update_statistics_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
