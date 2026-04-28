using Gtk;
using NUnit.Framework;
using QS.Test.GtkUI;
using QS.Widgets;
using Color = Gdk.Color;
using Window = Gtk.Window;
using WindowType = Gtk.WindowType;

namespace QS.Test.Widgets.GtkUI
{
	[TestFixture(TestOf = typeof(ValidatedEntry))]
	public class ValidatedEntryTest
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			GtkInit.AtOnceInitGtk();
		}

		[TestCase("user@example.com")]
		[TestCase("user.name+tag@example-domain.com")]
		[TestCase("user_name@example.com")]
		[TestCase("user@mail.example.com")]
		[TestCase("ALLINMAV@KOS.SIBUR.RU")]
		[TestCase("team@corp.eu.west.example.com")]
		public void EmailValidation_KeepsDefaultColor_ForValidAddresses(string email)
		{
			var entry = CreateEmailEntry();
			var hostWindow = PrepareWidget(entry);

			try {
				var defaultColor = GetTextColor(entry);

				entry.Text = email;
				FlushGtkEvents();

				Assert.That(entry.Text, Is.EqualTo(email));
				Assert.That(AreEqualColors(GetTextColor(entry), defaultColor), Is.True,
					$"Ожидался цвет текста по умолчанию для валидного email '{email}'.");
			}
			finally {
				hostWindow.Destroy();
				FlushGtkEvents();
			}
		}

		[Test]
		public void EmailValidation_RemovesSpacesAndNewLines_BeforeValidation()
		{
			var entry = CreateEmailEntry();
			var hostWindow = PrepareWidget(entry);

			try {
				var defaultColor = GetTextColor(entry);

				entry.Text = " user.name+tag@exa\nmple-domain.com ";
				FlushGtkEvents();

				Assert.That(entry.Text, Is.EqualTo("user.name+tag@example-domain.com"));
				Assert.That(AreEqualColors(GetTextColor(entry), defaultColor), Is.True,
					"После удаления пробелов и переводов строки валидный email должен остаться без красной подсветки.");
			}
			finally {
				hostWindow.Destroy();
				FlushGtkEvents();
			}
		}

		[TestCase("userexample.com")]
		[TestCase("user@example")]
		[TestCase("user@_example.com")]
		[TestCase("user@-example.com")]
		[TestCase("user@example-.com")]
		[TestCase("u@b.c")]
		public void EmailValidation_MarksInvalidAddressesRed(string email)
		{
			var entry = CreateEmailEntry();
			var hostWindow = PrepareWidget(entry);

			try {
				entry.Text = email;
				FlushGtkEvents();

				Assert.That(entry.Text, Is.EqualTo(email));
				Assert.That(AreEqualColors(GetTextColor(entry), new Color(255, 0, 0)), Is.True,
					$"Невалидный email '{email}' должен подсвечиваться красным.");
			}
			finally {
				hostWindow.Destroy();
				FlushGtkEvents();
			}
		}

		[Test]
		public void EmailValidation_ReturnsToDefaultColor_AfterReplacingInvalidEmailWithValid()
		{
			var entry = CreateEmailEntry();
			var hostWindow = PrepareWidget(entry);

			try {
				var defaultColor = GetTextColor(entry);

				entry.Text = "user@example";
				FlushGtkEvents();
				Assert.That(AreEqualColors(GetTextColor(entry), new Color(255, 0, 0)), Is.True,
					"Невалидный email должен сначала окрасить текст в красный цвет.");

				entry.Text = "user@example.com";
				FlushGtkEvents();

				Assert.That(entry.Text, Is.EqualTo("user@example.com"));
				Assert.That(AreEqualColors(GetTextColor(entry), defaultColor), Is.True,
					"После ввода валидного email подсветка должна сбрасываться к обычному цвету.");
			}
			finally {
				hostWindow.Destroy();
				FlushGtkEvents();
			}
		}

		private static ValidatedEntry CreateEmailEntry()
		{
			return new ValidatedEntry {
				ValidationMode = ValidationType.Email
			};
		}

		private static Window PrepareWidget(Widget widget)
		{
			var window = new Window(WindowType.Popup);
			window.Add(widget);
			window.ShowAll();
			FlushGtkEvents();
			return window;
		}

		private static void FlushGtkEvents()
		{
			while(Application.EventsPending()) {
				Application.RunIteration();
			}
		}

		private static Color GetTextColor(ValidatedEntry entry)
		{
			return entry.Style.Text(StateType.Normal);
		}

		private static bool AreEqualColors(Color actual, Color expected)
		{
			return actual.Red == expected.Red
				&& actual.Green == expected.Green
				&& actual.Blue == expected.Blue;
		}
	}
}

