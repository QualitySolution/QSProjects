using System;
using System.IO;
using System.Linq;
using System.Text;

namespace QS.DbManagement {
	public static class SqlDumpFileValidator {
		private const int InspectBytes = 8 * 1024;

		private static readonly string[] SqlStartTokens = {
			"--", "/*", "#", "CREATE", "INSERT", "REPLACE", "DROP", "ALTER",
			"SET ", "USE ", "LOCK", "DELIMITER", "START TRANSACTION"
		};

		/// <summary>
		/// Бросает исключение, если файл отсутствует, пуст, бинарный или не начинается с SQL синтаксиса
		/// </summary>
		public static void EnsureLooksLikeSqlDump(string filePath) {
			if(string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Не указан путь к файлу дампа", nameof(filePath));
			if(!File.Exists(filePath))
				throw new FileNotFoundException("Файл дампа не найден", filePath);
			if(new FileInfo(filePath).Length == 0)
				throw new InvalidDataException("Файл дампа пуст");

			string head = ReadHead(filePath, InspectBytes);

			// Бинарный файл почти всегда содержит нулевые байты
			if(head.IndexOf('\0') >= 0)
				throw new InvalidDataException(
					"Файл не похож на SQL-дамп: это бинарный файл, а не текстовый SQL-скрипт");

			string trimmed = head.TrimStart('﻿', ' ', '\t', '\r', '\n');
			bool looksLikeSql = SqlStartTokens.Any(
				token => trimmed.StartsWith(token, StringComparison.OrdinalIgnoreCase));
			if(!looksLikeSql)
				throw new InvalidDataException(
					"Файл не похож на SQL-дамп: в начале файла нет SQL-инструкций");
		}

		private static string ReadHead(string filePath, int maxBytes) {
			using(var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				int toRead = (int)Math.Min(maxBytes, stream.Length);
				var buffer = new byte[toRead];
				int read = stream.Read(buffer, 0, toRead);
				return Encoding.UTF8.GetString(buffer, 0, read);
			}
		}
	}
}
