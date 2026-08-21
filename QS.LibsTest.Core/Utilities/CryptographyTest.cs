using NUnit.Framework;
using QS.Utilities.Security;
using System.Collections.Generic;
using System.Linq;

namespace QS.LibsTest.Core.Utilities {
	/// <summary>
	/// Хеш паролей: формат менять нельзя - по нему проверяются пароли, уже лежащие в базах,
	/// а соль обязана быть разной у каждого вызова.
	/// </summary>
	[TestFixture(TestOf = typeof(Cryptography))]
	public class CryptographyTest {

		[Test(Description = "Свой пароль проходит проверку, чужой - нет")]
		public void ComputeHash_VerifiedByOwnPasswordOnly() {
			string hash = Cryptography.ComputeHash("правильный-пароль");

			Assert.That(Cryptography.VerifyHash("правильный-пароль", hash), Is.True);
			Assert.That(Cryptography.VerifyHash("другой-пароль", hash), Is.False);
		}

		[Test(Description = "Формат хеша - 64 символа хеша, двоеточие, 16 символов соли")]
		public void ComputeHash_KeepsStoredFormat() {
			string[] parts = Cryptography.ComputeHash("пароль").Split(':');

			Assert.That(parts.Length, Is.EqualTo(2), "разделитель ровно один");
			Assert.That(parts[0].Length, Is.EqualTo(64), "SHA256 в шестнадцатеричном виде");
			Assert.That(parts[1].Length, Is.EqualTo(16), "соль");
			Assert.That(parts[0], Does.Match("^[0-9A-F]+$"), "хеш в верхнем регистре без разделителей");
		}

		[Test(Description = "Соль у каждого вызова своя - иначе одинаковые пароли дают одинаковые хеши")]
		public void ComputeHash_ConsecutiveCalls_ProduceDifferentSalt() {
			// именно на этом ломался прежний вариант: new Random() засеян временем,
			// и вызовы подряд получали одну и ту же соль
			var salts = Enumerable.Range(0, 50)
				.Select(_ => Cryptography.ComputeHash("одинаковый-пароль").Split(':')[1])
				.ToList();

			Assert.That(new HashSet<string>(salts).Count, Is.EqualTo(salts.Count),
				"все соли должны быть разными");
		}

		[Test(Description = "Один пароль, посчитанный дважды, даёт разные хеши, но оба проверяются")]
		public void ComputeHash_SamePasswordTwice_BothVerify() {
			string first = Cryptography.ComputeHash("пароль");
			string second = Cryptography.ComputeHash("пароль");

			Assert.That(first, Is.Not.EqualTo(second));
			Assert.That(Cryptography.VerifyHash("пароль", first), Is.True);
			Assert.That(Cryptography.VerifyHash("пароль", second), Is.True);
		}

		[Test(Description = "Хеш, лежащий в базе с прежней реализации, продолжает проверяться")]
		public void VerifyHash_HashFromPreviousImplementation_StillValid() {
			// SHA256("qwerty" + "ABCDEFGHIJKLMNOP") в верхнем регистре - ровно то, что писал
			// прежний код. Если сломается, перестанут пускать всех существующих пользователей
			const string storedHash =
				"888E66BD6ACA161777DC23DCB36A61C8AADBA80ADAC5BCB11F516220D5E368E5:ABCDEFGHIJKLMNOP";

			Assert.That(Cryptography.VerifyHash("qwerty", storedHash), Is.True);
			Assert.That(Cryptography.VerifyHash("qwerty1", storedHash), Is.False);
		}

		[Test(Description = "Логин дополняется до 16 символов и приводится к верхнему регистру")]
		public void GenerateLogin_PadsToSixteenUpperCaseChars() {
			string login = Cryptography.GenerateLogin("user");

			Assert.That(login.Length, Is.EqualTo(16));
			Assert.That(login, Is.EqualTo(login.ToUpperInvariant()));
		}

		[Test(Description = "Длинный логин обрезается до 12 символов, дальше идёт случайная часть")]
		public void GenerateLogin_LongInput_TruncatedToTwelve() {
			string login = Cryptography.GenerateLogin("оченьдлинныйлогинпользователя".ToUpperInvariant());

			Assert.That(login.Length, Is.EqualTo(16));
			Assert.That(login.Substring(0, 12), Is.EqualTo("ОЧЕНЬДЛИННЫЙ"));
		}

		[Test(Description = "Логины не повторяются - под ними заводятся сессионные учётки на сервере")]
		public void GenerateLogin_ConsecutiveCalls_AreDifferent() {
			var logins = Enumerable.Range(0, 50).Select(_ => Cryptography.GenerateLogin("user")).ToList();

			Assert.That(new HashSet<string>(logins).Count, Is.EqualTo(logins.Count));
		}

		[Test(Description = "Пароль нужной длины и не повторяется")]
		public void GeneratePassword_HasRequestedLengthAndIsUnique() {
			var passwords = Enumerable.Range(0, 50).Select(_ => Cryptography.GeneratePassword(12)).ToList();

			Assert.That(passwords, Is.All.Length.EqualTo(12));
			Assert.That(new HashSet<string>(passwords).Count, Is.EqualTo(passwords.Count));
		}

		[Test(Description = "Нулевая длина пароля не роняет генератор")]
		public void GeneratePassword_ZeroLength_ReturnsEmpty() {
			Assert.That(Cryptography.GeneratePassword(0), Is.Empty);
		}
	}
}
