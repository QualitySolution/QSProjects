using System;
using System.Collections.Generic;
using System.Data.Common;

namespace QS.Deletion.Configuration
{
	public class DeleteInfo : IDeleteInfo
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public Type ObjectClass { get; set;}
		public string ObjectsName { get; set;}

		public string TableName { get; set;}

		/// <summary>
		/// Запрос Select для отображения удаляемых записей, в запросе в строке FROM
		/// можно не указывать напрямую имя таблицы, а использовать @tablename, что 
		/// в случае использования ORM, позволяет переименовывать таблицу для класса
		/// без последствий для удаления. 
		/// </summary>
		public string SqlSelect;
		public string DisplayString;
		public List<DeleteDependenceInfo> DeleteItems { get; set;}
		public List<ClearDependenceInfo> ClearItems { get; set;}
		public List<RemoveFromDependenceInfo> RemoveFromItems { get; set;}
		public List<UpdateDependenceInfo> UpdateItems { get; set;}

		public bool HasDependences
		{
			get
			{
				return DeleteItems.Count > 0 || ClearItems.Count > 0 || RemoveFromItems.Count > 0;
			}
		}

		public string PreparedSqlSelect{
			get { //Заменяем название таблицы и добавляем пробел, если его нет.
				return SqlSelect.Replace ("@tablename", String.Format("`{0}`", TableName)).TrimEnd (' ') + " ";
			}
		}


		public DeleteInfo()
		{
			DeleteItems = new List<DeleteDependenceInfo>();
			ClearItems = new List<ClearDependenceInfo>();
			RemoveFromItems = new List<RemoveFromDependenceInfo>();
			UpdateItems = new List<UpdateDependenceInfo>();
		}

		IList<EntityDTO> IDeleteInfo.GetDependEntities(IDeleteCore core, DeleteDependenceInfo depend, EntityDTO masterEntity)
		{
			return GetEntitiesList(core.UoW.Session.Connection, depend.WhereStatment, masterEntity.Id);
		}

		IList<EntityDTO> IDeleteInfo.GetDependEntities(IDeleteCore core, ClearDependenceInfo depend, EntityDTO masterEntity)
		{
			return GetEntitiesList(core.UoW.Session.Connection, depend.WhereStatment, masterEntity.Id);
		}

		IList<EntityDTO> IDeleteInfo.GetDependEntities(IDeleteCore core, RemoveFromDependenceInfo depend, EntityDTO masterEntity)
		{
			throw new NotImplementedException ();
		}

		EntityDTO IDeleteInfo.GetSelfEntity(IDeleteCore core, uint id)
		{
			return GetEntitiesList(core.UoW.Session.Connection, String.Format("WHERE {0}.id = @id", TableName), id)[0];
		}

		private IList<EntityDTO> GetEntitiesList(DbConnection connection, string whereStatment, uint forId)
		{
			string sql = PreparedSqlSelect + whereStatment;
			DbCommand cmd = connection.CreateCommand ();
			var resultList = new List<EntityDTO> ();
			cmd.CommandText = sql;
			logger.Debug ("Запрос объектов по SQL={0}", cmd.CommandText);
			InternalHelper.AddParameterWithId (cmd, forId);

			using (DbDataReader rdr = cmd.ExecuteReader ()) {
				int IndexOfIdParam = rdr.GetOrdinal ("id");
				while (rdr.Read ()) {
					object[] fields = new object[rdr.FieldCount];
					rdr.GetValues (fields);

					resultList.Add (new EntityDTO{
						Id = (uint)fields[IndexOfIdParam],
						ClassType = ObjectClass,
						Title = String.Format (DisplayString, fields)
					});
				}
			}
			return resultList;
		}

		public Operation CreateDeleteOperation(EntityDTO masterEntity, DeleteDependenceInfo depend, IList<EntityDTO> dependEntities)
		{
			return new SQLDeleteOperation {
				ItemId = masterEntity.Id,
				TableName = TableName,
				WhereStatment = depend.WhereStatment
			};
		}

		public Operation CreateDeleteOperation(EntityDTO entity)
		{
			return new SQLDeleteOperation {
				ItemId = entity.Id,
				TableName = TableName,
				WhereStatment = "WHERE id = @id"
			};
		}

		public Operation CreateClearOperation(EntityDTO masterEntity, ClearDependenceInfo depend, IList<EntityDTO> dependEntities)
		{
			return new SQLCleanOperation () {
				ItemId = masterEntity.Id,
				TableName = TableName,
				CleanField = depend.ClearField,
				WhereStatment = depend.WhereStatment
			};
		}

		public Operation CreateRemoveFromOperation(EntityDTO masterEntity, RemoveFromDependenceInfo depend, IList<EntityDTO> dependEntities)
		{
			throw new NotSupportedException ();
		}

		public Operation CreateUpdateOperation(EntityDTO entityDTO, UpdateDependenceInfo updateDependenceInfo) {
			throw new NotImplementedException();
		}
	}
}

