using System;
using System.Linq;
using Gamma.Utilities;

namespace QSOrmProject.Permissions
{
	public class PermissionMatrix<TPermissionEnum, TPerEntity> : IPermissionMatrix
		where TPerEntity : IDomainObject
	{
		public int PermissionCount { get; private set; }
		public int ColumnCount { get; private set; }

		public PermissionMatrix(string title)
		{
			Title = title;
		}

		public string Title { get; private set; }

		string[] permissionNames;

		public string[] PermissionNames {
			get {
				if(permissionNames == null)
					Init();
				return permissionNames;
			}
		}

		string[] columnNames;

		public string[] ColumnNames {
			get {
				if(columnNames == null) {
					Init();
				}

				return columnNames;
			}
		}

		TPerEntity[] Entities;
		bool[,] allowed;

		public void Init()
		{
			permissionNames = Enum.GetValues(typeof(TPermissionEnum)).Cast<Enum>()
	   			.Select(x => AttributeUtil.GetEnumTitle(x))
	   			.ToArray();
			PermissionCount = PermissionNames.Length;

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var entities = uow.GetAll<TPerEntity>();
				columnNames = entities.Select(x => DomainHelper.GetObjectTilte(x)).ToArray();
				Entities = entities.ToArray();
			}
			ColumnCount = Entities.Length;

			allowed = new bool[PermissionCount, ColumnCount];

		}

		#region Индексаторы

		public bool this[int permissionIx, int columnIx] {
			get{
				return allowed[permissionIx, columnIx];
			}
			set{
				allowed[permissionIx, columnIx] = value;
			}
		}

  		#endregion
	}
}
