using System;
using System.Collections.Generic;
using System.Data.Common;
using QSProjectsLib;

namespace QSOrmProject.Deletion
{
	abstract class Operation
	{
		public uint ItemId;
		public List<Operation> ChildOperations = new List<Operation> ();

		public abstract void Execute (DbTransaction trans);
	}

	class SQLDeleteOperation : Operation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public string TableName;
		public string WhereStatment;

		public override void Execute (DbTransaction trans)
		{
			ChildOperations.ForEach (o => o.Execute (trans));

			DbCommand cmd = QSMain.ConnectionDB.CreateCommand ();
			cmd.Transaction = trans;
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

		public override void Execute (DbTransaction trans)
		{
			var sql = new DBWorks.SQLHelper ("UPDATE {0} SET ", TableName);
			sql.StartNewList ("", ", ");
			foreach (string FieldName in CleanFields) {
				sql.AddAsList ("{0} = NULL ", args: FieldName);
			}
			sql.Add (WhereStatment);

			DbCommand cmd = QSMain.ConnectionDB.CreateCommand ();
			cmd.Transaction = trans;
			cmd.CommandText = sql.Text;
			DeleteCore.AddParameterWithId (cmd, ItemId);
			logger.Debug ("Выполнение очистки ссылок SQL={0}", cmd.CommandText);
			cmd.ExecuteNonQuery ();
		}
	}
}

