using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using QSProjectsLib;

namespace QSOrmProject.Deletion
{
	public abstract class Operation
	{
		public uint ItemId;
		public List<Operation> ChildOperations = new List<Operation> ();

		public abstract void Execute (DeleteCore core);
	}

	class SQLDeleteOperation : Operation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public string TableName;
		public string WhereStatment;

		public override void Execute (DeleteCore core)
		{
			ChildOperations.ForEach (o => o.Execute (core));

			DbCommand cmd = QSMain.ConnectionDB.CreateCommand ();
			cmd.Transaction = core.SqlTransaction;
			cmd.CommandText = String.Format ("DELETE FROM {0} {1}", TableName, WhereStatment);
			DeleteCore.AddParameterWithId (cmd, ItemId);
			logger.Debug ("Выполение удаления SQL={0}", cmd.CommandText);
			cmd.ExecuteNonQuery ();
		}
	}

	class SQLCleanOperation : Operation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public string TableName;
		public string WhereStatment;
		public string[] CleanFields;

		public override void Execute (DeleteCore core)
		{
			var sql = new DBWorks.SQLHelper ("UPDATE {0} SET ", TableName);
			sql.StartNewList ("", ", ");
			foreach (string FieldName in CleanFields) {
				sql.AddAsList ("{0} = NULL ", args: FieldName);
			}
			sql.Add (WhereStatment);

			DbCommand cmd = QSMain.ConnectionDB.CreateCommand ();
			cmd.Transaction = core.SqlTransaction;
			cmd.CommandText = sql.Text;
			DeleteCore.AddParameterWithId (cmd, ItemId);
			logger.Debug ("Выполнение очистки ссылок SQL={0}", cmd.CommandText);
			cmd.ExecuteNonQuery ();
		}
	}

	class HibernateDeleteOperation : Operation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public IList<EntityDTO> DeletingItems { get; set;}

		public override void Execute (DeleteCore core)
		{
			foreach(var item in DeletingItems)
			{
				logger.Debug ("Удаляем {0}...", item.Title);
				core.UoW.TryDelete (item.Entity);
			}
		}
	}

	class HibernateCleanOperation : Operation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public Type EntityType { get; set;}
		public IList<EntityDTO> ClearingItems { get; set;}
		public string PropertyName { get; set;}

		public override void Execute (DeleteCore core)
		{
			var propertyCache = EntityType.GetProperty (PropertyName);
			foreach(var item in ClearingItems)
			{
				logger.Debug ("Очищаем свойство {0} в {1}...", PropertyName, item.Title);
				propertyCache.SetValue (item.Entity, null, null);
				core.UoW.TrySave (item.Entity);
			}
		}
	}
}

