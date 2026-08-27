using NUnit.Framework;
using QS.DbManagement.Entities;
using QS.Launcher.ViewModels.PageViewModels;

namespace QS.Launcher.Test.ViewModels {
	/// <summary>
	/// Строка доступа к базе: три галочки взаимно исключают друг друга, а подпись под именем
	/// базы обязана называть то, что из них получится на сервере. Права на обновление базы
	/// отдельной галочки не имеют - оно входит в обычный доступ.
	/// </summary>
	[TestFixture(TestOf = typeof(BaseAccessRowVM))]
	public class BaseAccessRowVM_Test {
		private static BaseAccessRowVM Row() =>
			new BaseAccessRowVM(new DbUserBaseAccess { BaseName = "base", Title = "База" }, showReadOnly: true);

		[Test(Description = "Без доступа обновлять нечего")]
		public void NoAccess_CannotUpdate() {
			var row = Row();

			Assert.That(row.CanUpdate, Is.False);
			Assert.That(row.LevelDescription, Is.EqualTo("Нет доступа"));
		}

		[Test(Description = "Обычный доступ включает право обновлять базу")]
		public void PlainAccess_CanUpdate() {
			var row = Row();

			row.HasAccess = true;

			Assert.That(row.CanUpdate, Is.True,
				"в обычный доступ входят ALTER/CREATE/DROP - это и есть право накатывать обновления");
			Assert.That(row.LevelDescription, Is.EqualTo("Пользование и обновление базы"));
		}

		[Test(Description = "Только просмотр отбирает право обновлять")]
		public void ReadOnly_CannotUpdate() {
			var row = Row();

			row.HasAccess = true;
			row.ReadOnly = true;

			Assert.That(row.CanUpdate, Is.False);
			Assert.That(row.LevelDescription, Is.EqualTo("Только просмотр, без обновления базы"));
		}

		[Test(Description = "Все права на базу тоже позволяют её обновлять")]
		public void FullRights_CanUpdate() {
			var row = Row();

			row.IsAdmin = true;

			Assert.That(row.HasAccess, Is.True, "галочка всех прав сама включает доступ");
			Assert.That(row.CanUpdate, Is.True);
			Assert.That(row.LevelDescription, Is.EqualTo("Все права на базу"));
		}

		[Test(Description = "Снятие доступа сбрасывает и остальные галочки")]
		public void AccessRemoved_ClearsOtherFlags() {
			var row = Row();
			row.IsAdmin = true;

			row.HasAccess = false;

			Assert.That(row.IsAdmin, Is.False);
			Assert.That(row.ReadOnly, Is.False);
			Assert.That(row.LevelDescription, Is.EqualTo("Нет доступа"));
		}

		[Test(Description = "Изменение галочек видно через уведомления - подпись пересчитывается сама")]
		public void FlagsChanged_RaisesLevelNotifications() {
			var row = Row();
			var changed = new System.Collections.Generic.List<string>();
			row.PropertyChanged += (sender, args) => changed.Add(args.PropertyName);

			row.HasAccess = true;

			Assert.That(changed, Does.Contain(nameof(BaseAccessRowVM.CanUpdate)));
			Assert.That(changed, Does.Contain(nameof(BaseAccessRowVM.LevelDescription)));
		}

		[Test(Description = "Пока галочки не трогали, сохранять нечего")]
		public void Untouched_IsNotDirty() {
			var row = new BaseAccessRowVM(
				new DbUserBaseAccess { BaseName = "base", Title = "База", HasAccess = true }, showReadOnly: true);

			Assert.That(row.IsDirty, Is.False);

			row.ReadOnly = true;
			Assert.That(row.IsDirty, Is.True, "смена уровня - это изменение, его нужно сохранить");
		}
	}
}
