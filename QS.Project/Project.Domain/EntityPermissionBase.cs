﻿using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace QS.Project.Domain
{
	public abstract class EntityPermissionBase : PropertyChangedBase
	{
		private TypeOfEntity typeOfEntity;
		[Display(Name = "Тип сущности")]
		public virtual TypeOfEntity TypeOfEntity {
			get => typeOfEntity;
			set => SetField(ref typeOfEntity, value, () => TypeOfEntity);
		}

		private bool canCreate;
		[Display(Name = "Может создавать")]
		public virtual bool CanCreate {
			get => canCreate;
			set => SetField(ref canCreate, value, () => CanCreate);
		}

		private bool canRead;
		[Display(Name = "Может просматривать")]
		public virtual bool CanRead {
			get => canRead;
			set => SetField(ref canRead, value, () => CanRead);
		}

		private bool canUpdate;
		[Display(Name = "Может редактировать")]
		public virtual bool CanUpdate {
			get => canUpdate;
			set => SetField(ref canUpdate, value, () => CanUpdate);
		}

		private bool canDelete;
		[Display(Name = "Может удалять")]
		public virtual bool CanDelete {
			get => canDelete;
			set => SetField(ref canDelete, value, () => CanDelete);
		}
	}
}
