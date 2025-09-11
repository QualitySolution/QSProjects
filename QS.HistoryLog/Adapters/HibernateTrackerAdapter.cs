using System;
using System.Linq;
using System.Threading.Tasks;
using QS.HistoryLog.Domain;
using System.Collections.Generic;
using QS.SqlLog.Domain;
using QS.SqlLog.Interfaces;

namespace QS.HistoryLog.Adapters {
	internal static class HibernateTrackerAdapter {

		internal static void Save(ChangeSet changeSet, IChangeSetPersister persister) {
			if(changeSet == null) throw new ArgumentNullException(nameof(changeSet));
			if(persister == null) throw new ArgumentNullException(nameof(persister));

			var dto = ToDto(changeSet);
			persister.Save(dto);
		}

		internal static Task SaveAsync(ChangeSet changeSet, IChangeSetPersister persister) {
			if(changeSet == null) throw new ArgumentNullException(nameof(changeSet));
			if(persister == null) throw new ArgumentNullException(nameof(persister));

			var dto = ToDto(changeSet);
			return persister.SaveAsync(dto);
		}

		#region Конвертеры

		private static ChangeSetDto ToDto(ChangeSet cs) {
			var dto = new ChangeSetDto {
				ActionName = cs.ActionName,
				UserId = cs.UserId,
				UserLogin = cs.UserLogin
			};

			foreach(var e in cs.Entities ?? Enumerable.Empty<ChangedEntity>())
				dto.Entities.Add(ToDto(e));

			return dto;
		}

		private static ChangedEntityDto ToDto(ChangedEntity e) {
			if(e == null) return null;

			var dto = new ChangedEntityDto {
				ChangeTime = e.ChangeTime,
				Operation = e.Operation.ToString(),
				EntityClassName = e.EntityClassName,
				EntityId = e.EntityId,
				EntityTitle = e.EntityTitle
			};

			var changes = e.Changes;
			foreach(var fc in changes)
				dto.Changes.Add(ToDto(fc));

			return dto;
		}

		private static FieldChangeDto ToDto(FieldChange fc) {
			if(fc == null) return null;

			var dto = new FieldChangeDto {
				Path = fc.Path,
				OldValue = fc.OldValue,
				NewValue = fc.NewValue,
				OldId = fc.OldId,
				NewId = fc.NewId,
				Type = fc.Type.ToString()
			};

			return dto;
		}

		#endregion
	}
}
