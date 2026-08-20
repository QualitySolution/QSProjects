using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace QS.Utilities.Security
{
	public static class Cryptography
	{
		private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		private const string PasswordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()?";

		/// <summary>
		/// Вычисляет SHA256 хэш заданной строки с солью.
		/// </summary>
		/// <returns>64 символа хэша и 16 символов соли, разделенные знаком ":".</returns>
		/// <param name="input">Входная строка.</param>
		public static string ComputeHash(string input)
		{
			string salt = RandomChars(chars, 16);

			return Hash(input + salt) + ":" + salt;
		}

		/// <summary>
		/// Проверяет строку на соответствие заданному хэшу.
		/// </summary>
		/// <returns><c>true</c>, если проверка успешна, в противном случае <c>false</c>.</returns>
		/// <param name="pass">Входная строка.</param>
		/// <param name="saltedHash">64 символа хэша и 16 символов соли, разделенные знаком ":".</param>
		public static bool VerifyHash(string pass, string saltedHash)
		{
			string[] splitted = saltedHash.Split(':');

			return splitted[0] == Hash(pass + splitted[1]);
		}

		/// <summary>
		/// Генерирует логин на основе максимум 12 первых символов входной строки.
		/// </summary>
		/// <returns>Логин из 16 символов.</returns>
		/// <param name="login">Входная строка.</param>
		public static string GenerateLogin(string login)
		{
			if(login.Length > 12)
				login = login.Substring(0, 12);
			return (login + RandomChars(chars, 16 - login.Length)).ToUpperInvariant();
		}

		/// <summary>
		/// Генерирует пароль заданной длины.
		/// </summary>
		/// <returns>Сгенерированный пароль</returns>
		/// <param name="length">Длина пароля.</param>
		public static string GeneratePassword(int length) => RandomChars(PasswordChars, length);

		private static string Hash(string input)
		{
			using(var algorithm = SHA256.Create())
			{
				byte[] hashedBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
				return BitConverter.ToString(hashedBytes).Replace("-", string.Empty);
			}
		}

		private static string RandomChars(string alphabet, int count)
		{
			if(count <= 0)
				return string.Empty;

			using(var rng = RandomNumberGenerator.Create())
			{
				var bytes = new byte[count * sizeof(uint)];
				rng.GetBytes(bytes);

				return new string(Enumerable.Range(0, count)
					.Select(i => alphabet[(int)(BitConverter.ToUInt32(bytes, i * sizeof(uint)) % (uint)alphabet.Length)])
					.ToArray());
			}
		}
	}
}
