using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using QS.HistoryLog.Domain;

namespace QS.HistoryLog
{
    public class ChangeSetWriter {
		/// <summary>
		/// Используется для записи журнала изменений.
		/// </summary>
		private readonly string connectionString;
		//На случай, если изменений много, а размер передаваемого пакета данных не велик
		private const int _maxChangedEntitiesSaveInOneBatch = 10000;
		private readonly int _batchSize;

		public ChangeSetWriter(string connectionString, int? batchSize = null) {
			this.connectionString = connectionString;
			_batchSize = batchSize ?? _maxChangedEntitiesSaveInOneBatch;
			if(!connectionString.Contains("Allow User Variables")) {
				if(!this.connectionString.EndsWith(";"))
					this.connectionString += ";";
				this.connectionString += "Allow User Variables=true;";
			}
		}

		public void Save(IChangeSetToSave changeSet) {
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				var transaction = connection.BeginTransaction();
				
				foreach(var batch in CreateBatches(changeSet, connection, transaction)) {
					batch.ExecuteNonQuery();
					batch.Dispose();
				}
				
				transaction.Commit();
			}
		}

		public async Task SaveAsync(IChangeSetToSave changeSet) {
			using(var connection = new MySqlConnection(connectionString)) {
				await connection.OpenAsync();
				var transaction = await connection.BeginTransactionAsync();
				
				foreach(var batch in CreateBatches(changeSet, connection, transaction)) {
					await batch.ExecuteNonQueryAsync();
					batch.Dispose();
				}
				
				await transaction.CommitAsync();
			}
		}

		private IEnumerable<MySqlBatch> CreateBatches(IChangeSetToSave changeSet, MySqlConnection connection, MySqlTransaction transaction) {
			MySqlBatch batch = new MySqlBatch(connection, transaction);
			batch.BatchCommands.Add(CreateInsertChangesSetCommand(changeSet));
			batch.BatchCommands.Add(CreateSetChangeSetIdParameterCommand());

			var entityCountInBatch = 0;
			foreach(var entity in changeSet.Entities) {
				// Добавляем сущность в батч
				batch.BatchCommands.Add(CreateInsertChangedEntityCommand(entity));
				batch.BatchCommands.Add(CreateSetChangedEntityIdParameterCommand());

				foreach(var change in entity.Changes) {
					batch.BatchCommands.Add(CreateInsertEntityChangesCommand(change));
				}

				entityCountInBatch++;

				// Если достигли лимита - возвращаем батч
				if(entityCountInBatch >= _batchSize) {
					yield return batch;
					batch = new MySqlBatch(connection, transaction);
					entityCountInBatch = 0;
				}
			}

			// Возвращаем оставшийся батч
			if(entityCountInBatch > 0) 
				yield return batch;
		}

		private MySqlBatchCommand CreateSetChangeSetIdParameterCommand() {
			return new MySqlBatchCommand("SET @ChangeSetId = LAST_INSERT_ID();");
		}

		private MySqlBatchCommand CreateSetChangedEntityIdParameterCommand() {
			return new MySqlBatchCommand("SET @ChangedEntityId = LAST_INSERT_ID();");
		}

		private MySqlBatchCommand CreateInsertChangesSetCommand(IChangeSetToSave changeSet) {
			var sqlInsertChangeSet =
				"INSERT INTO history_changeset (user_login, action_name, user_id) " +
				"VALUES (@UserLogin, @ActionName, @UserId);";

			return new MySqlBatchCommand(sqlInsertChangeSet) {
				Parameters = {
					new MySqlParameter("@UserLogin", changeSet.UserLogin),
					new MySqlParameter("@ActionName", changeSet.ActionName),
					new MySqlParameter("@UserId", changeSet.UserId),
				}
			};
		}

		private MySqlBatchCommand CreateInsertEntityChangesCommand(IFieldChangeToSave change) {
			var sqlInsertChange =
				"INSERT INTO history_changes (type, field_name, old_value, old_id, new_value, new_id, changed_entity_id) " +
				"VALUES (@TypeOfChange, @Path, @OldValue, @OldId, @NewValue, @NewId, @ChangedEntityId);";

			return new MySqlBatchCommand(sqlInsertChange) {
				Parameters = {
					new MySqlParameter("@TypeOfChange", change.Type.ToString()),
					new MySqlParameter("@Path", change.Path),
					new MySqlParameter("@OldValue", change.OldValue),
					new MySqlParameter("@OldId", change.OldId),
					new MySqlParameter("@NewValue", change.NewValue),
					new MySqlParameter("@NewId", change.NewId),
				}
			};
		}

		private MySqlBatchCommand CreateInsertChangedEntityCommand(IChangedEntityToSave entity) {
			var sqlInsertEntity =
				"INSERT INTO history_changed_entities (datetime, operation, entity_class, entity_id, entity_title, changeset_id) " +
				"VALUES (@ChangeTime, @OperationDbName, @EntityClassName, @EntityId, @EntityTitle, @ChangeSetId);";

			return new MySqlBatchCommand(sqlInsertEntity) {
				Parameters = {
					new MySqlParameter("@ChangeTime", entity.ChangeTime),
					new MySqlParameter("@OperationDbName", entity.Operation.ToString()),
					new MySqlParameter("@EntityClassName", entity.EntityClassName),
					new MySqlParameter("@EntityId", entity.EntityId),
					new MySqlParameter("@EntityTitle", entity.EntityTitle),
				}
			};
		}
	}
}
