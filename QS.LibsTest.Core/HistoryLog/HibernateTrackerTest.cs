using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using NUnit.Framework;
using QS.HistoryLog.Domain;
using QS.HistoryLog;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Test.TestApp.Domain;
using QS.Testing.DB;

namespace QS.Test.HistoryLog {
	[TestFixture(TestOf = typeof(HibernateTracker))]
	public class HibernateTrackerTest : MariaDbTestContainerFixtureBase {
		
		[OneTimeSetUp]
		public async Task OneTimeSetUp() {
			UseTracking = true;
			await InitialiseMariaDb(typeof(TrackedEntity).Assembly, typeof(ChangeSet).Assembly, typeof(UserBase).Assembly);
		}

		[OneTimeTearDown]
		public async Task OneTimeTearDown() {
			await DisposeMariaDb();
		}

		[Test]
		public async Task HibernateTracker_SaveChangeSet() {
			HistoryMain.Enable(ConnectionStringBuilder);
			using(var uow = UnitOfWorkFactory.Create(userActionTitle: "Test Action")) {
				var user = new UserBase { Name = "Test User" };
				UserRepository.GetCurrentUserId = () => user.Id;
				uow.Save(user);
				
				var anotherEntity = new SimpleEntity { Text = "Another Entity" };
				uow.Save(anotherEntity);
				
				var anotherEntity2 = new SimpleEntity { Text = "Another Entity 2" };
				uow.Save(anotherEntity2);
				
				uow.Commit();
				
				var entity = new TrackedEntity {
					Name = "Test Entity",
					Description = "This is a test entity",
					AnotherEntity = anotherEntity
				};
				uow.Save(entity);
				uow.Commit();
				
				entity.Description = "Updated description";
				entity.AnotherEntity = anotherEntity2;
				uow.Save(entity);
				uow.Commit();
				
				uow.Delete(entity);
				uow.Commit();
				
				// Проверяем, что изменения были зафиксированы в базе данных
				var changeSets = uow.Session.QueryOver<ChangeSet>()
					.Fetch(SelectMode.Fetch, s => s.Entities)
					.List();
				Assert.AreEqual(3, changeSets.Count, "Должно быть три набора изменений: создание, обновление, удаление."); //Внимание юзера не трекаем, если вдруг это включится надо будет поменять на 4
				Assert.That(changeSets, Has.All.Matches<ChangeSet>(cs => cs.Entities.Count == 1), "Каждый набор изменений должен содержать одну сущность.");
				Assert.That(changeSets.All(x => x.ActionName == "Test Action"), Is.True, "Имя действия должно быть 'Test Action'");
				Assert.That(changeSets.All(x => x.User.Id == user.Id), Is.True, "Пользователь должен быть 'Test User'");
				var createdSet = changeSets.FirstOrDefault(cs => cs.Entities.Any(e => e.Operation == EntityChangeOperation.Create));
				Assert.IsNotNull(createdSet, "Должен быть набор изменений для создания.");
				var updatedSet = changeSets.FirstOrDefault(cs => cs.Entities.Any(e => e.Operation == EntityChangeOperation.Change));
				Assert.IsNotNull(updatedSet, "Должен быть набор изменений для обновления.");
				var deletedSet = changeSets.FirstOrDefault(cs => cs.Entities.Any(e => e.Operation == EntityChangeOperation.Delete));
				Assert.IsNotNull(deletedSet, "Должен быть набор изменений для удаления.");
				
				var createdEntity = createdSet.Entities.First();
				Assert.AreEqual(nameof(TrackedEntity), createdEntity.EntityClassName, "Имя класса созданной сущности должно быть 'TrackedEntity'");
				Assert.AreEqual(entity.Id, createdEntity.EntityId, "Id созданной сущности должно совпадать.");
				Assert.AreEqual(entity.Name, createdEntity.EntityTitle, "Заголовок созданной сущности должно совпадать.");
				
				var createdFields = createdEntity.Changes;
				Assert.AreEqual(3, createdFields.Count, "Должно быть три поля при создании.");
				
				var nameField = createdFields.FirstOrDefault(f => f.Path == nameof(TrackedEntity.Name));
				Assert.That(nameField, Is.Not.Null);
				Assert.That(nameField.FieldTitle, Is.EqualTo("Название"));
				Assert.That(nameField.OldValue, Is.Null.Or.Empty);
				Assert.That(nameField.NewValue, Is.EqualTo("Test Entity"));
				
				var anotherField = createdFields.FirstOrDefault(f => f.Path == nameof(TrackedEntity.AnotherEntity));
				Assert.That(anotherField, Is.Not.Null);
				Assert.That(anotherField.OldId, Is.Null);
				Assert.That(anotherField.NewId, Is.EqualTo(anotherEntity.Id));
				Assert.That(anotherField.NewValueText, Is.EqualTo($"[Простая сущность Another Entity]"));
				Assert.That(anotherField.OldValueText, Is.Null.Or.Empty);
				
				var updatedEntity = updatedSet.Entities.First();
				Assert.That(updatedEntity.EntityClassName, Is.EqualTo("TrackedEntity"));
				Assert.That(updatedEntity.EntityTitle, Is.EqualTo("Test Entity"));
				Assert.That(updatedEntity.Changes.Count, Is.EqualTo(2));
				
				var descriptionField = updatedEntity.Changes.FirstOrDefault(c => c.Path == nameof(TrackedEntity.Description));
				Assert.That(descriptionField, Is.Not.Null);
				Assert.That(descriptionField.OldValue, Is.EqualTo("This is a test entity"));
				Assert.That(descriptionField.NewValue, Is.EqualTo("Updated description"));
				
				var anotherField2 = updatedEntity.Changes.FirstOrDefault(f => f.Path == nameof(TrackedEntity.AnotherEntity));
				Assert.That(anotherField2, Is.Not.Null);
				Assert.That(anotherField2.OldId, Is.EqualTo(anotherEntity.Id));
				Assert.That(anotherField2.NewId, Is.EqualTo(anotherEntity2.Id));
				Assert.That(anotherField2.OldValueText, Is.EqualTo($"[Простая сущность Another Entity]"));
				Assert.That(anotherField2.NewValueText, Is.EqualTo($"[Простая сущность Another Entity 2]"));
				
				var deletedEntity = deletedSet.Entities.First();
				Assert.That(deletedEntity.EntityClassName, Is.EqualTo("TrackedEntity"));
				Assert.That(deletedEntity.EntityTitle, Is.EqualTo("Test Entity"));
				//Если при удалении будет записывать состояние удаляемого объекта, надо будет добавить проверку на поля.
			}
		}
	}
}
