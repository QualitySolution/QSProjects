using MySqlConnector;
using QS.SqlLog.Domain;
using QS.SqlLog.Interfaces;
using System;
using System.Threading.Tasks;

namespace QS.SqlLog
{
    public class ChangeSetPersister : IChangeSetPersister {
		/// <summary>
		/// Используется для записи журнала изменений.
		/// </summary>
		private readonly string connectionString;
		//На случай, если изменений много, а размер передаваемого пакета данных не велик
		private const int _maxChangedEntitiesSaveInOneBatch = 10000;


		public ChangeSetPersister(string connectionString) {
			this.connectionString = connectionString;
			if(!connectionString.Contains("Allow User Variables")) {
				if(!this.connectionString.EndsWith(";"))
					this.connectionString += ";";
				this.connectionString += "Allow User Variables=true;";
			}
		}

		public void Save(ChangeSetDto changeSet) {
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				var transaction = connection.BeginTransaction();

				if(changeSet.Entities.Count < _maxChangedEntitiesSaveInOneBatch) {
					ExecuteSingleBatch(changeSet, connection, transaction);
				}
				else {
					ExecuteMultipleBatches(changeSet, connection, transaction);
				}

				transaction.Commit();
			}
		}


		public async Task SaveAsync(ChangeSetDto changeSet) {
			using(var connection = new MySqlConnection(connectionString)) {
				await connection.OpenAsync();
				var transaction = await connection.BeginTransactionAsync();

				if(changeSet.Entities.Count < _maxChangedEntitiesSaveInOneBatch) {
					ExecuteSingleBatch(changeSet, connection, transaction);
				}
				else {
					ExecuteMultipleBatches(changeSet, connection, transaction);
				}

				await transaction.CommitAsync();
			}
		}

		private void ExecuteMultipleBatches(ChangeSetDto changeSet, MySqlConnection connection, MySqlTransaction transaction) {

			var repeatCount = Math.Ceiling((decimal)changeSet.Entities.Count / _maxChangedEntitiesSaveInOneBatch);
			var entitiesIndex = 0;

			for(var i = 0; i < repeatCount; i++) {
				using(var batch = new MySqlBatch(connection, transaction)) {
					if(i == 0) {
						batch.BatchCommands.Add(CreateInsertChangesSetCommand(changeSet));
						batch.BatchCommands.Add(CreateSetChangeSetIdParameterCommand());
					}

					do {
						batch.BatchCommands.Add(CreateInsertChangedEntityCommand(changeSet.Entities[entitiesIndex]));
						batch.BatchCommands.Add(CreateSetChangedEntityIdParameterCommand());

						foreach(var change in changeSet.Entities[entitiesIndex].Changes) {
							batch.BatchCommands.Add(CreateInsertEntityChangesCommand(change)
							);
						}

						entitiesIndex++;
					} while(entitiesIndex < changeSet.Entities.Count && entitiesIndex % _maxChangedEntitiesSaveInOneBatch != 0);
					batch.ExecuteNonQuery();
				}
			}
		}

		private void ExecuteSingleBatch(ChangeSetDto changeSet, MySqlConnection connection, MySqlTransaction transaction) {
			using(var batch = new MySqlBatch(connection, transaction)) {
				batch.BatchCommands.Add(CreateInsertChangesSetCommand(changeSet));
				batch.BatchCommands.Add(CreateSetChangeSetIdParameterCommand());

				foreach(var entity in changeSet.Entities) {
					batch.BatchCommands.Add(CreateInsertChangedEntityCommand(entity));
					batch.BatchCommands.Add(CreateSetChangedEntityIdParameterCommand());

					foreach(var change in entity.Changes) {
						batch.BatchCommands.Add(CreateInsertEntityChangesCommand(change)
						);
					}
				}
				batch.ExecuteNonQuery();
			}
		}

		private MySqlBatchCommand CreateSetChangeSetIdParameterCommand() {
			return new MySqlBatchCommand("SET @ChangeSetId = LAST_INSERT_ID();");
		}

		private MySqlBatchCommand CreateSetChangedEntityIdParameterCommand() {
			return new MySqlBatchCommand("SET @ChangedEntityId = LAST_INSERT_ID();");
		}

		private MySqlBatchCommand CreateInsertChangesSetCommand(ChangeSetDto changeSet) {
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

		private MySqlBatchCommand CreateInsertEntityChangesCommand(FieldChangeDto change) {
			var sqlInsertChange =
				"INSERT INTO history_changes (type, field_name, old_value, old_id, new_value, new_id, changed_entity_id) " +
				"VALUES (@TypeOfChange, @Path, @OldValue, @OldId, @NewValue, @NewId, @ChangedEntityId);";

			return new MySqlBatchCommand(sqlInsertChange) {
				Parameters = {
					new MySqlParameter("@TypeOfChange", change.Type),
					new MySqlParameter("@Path", change.Path),
					new MySqlParameter("@OldValue", change.OldValue),
					new MySqlParameter("@OldId", change.OldId),
					new MySqlParameter("@NewValue", change.NewValue),
					new MySqlParameter("@NewId", change.NewId),
				}
			};
		}

		private MySqlBatchCommand CreateInsertChangedEntityCommand(ChangedEntityDto entity) {
			var sqlInsertEntity =
				"INSERT INTO history_changed_entities (datetime, operation, entity_class, entity_id, entity_title, changeset_id) " +
				"VALUES (@ChangeTime, @OperationDbName, @EntityClassName, @EntityId, @EntityTitle, @ChangeSetId);";

			return new MySqlBatchCommand(sqlInsertEntity) {
				Parameters = {
					new MySqlParameter("@ChangeTime", entity.ChangeTime),
					new MySqlParameter("@OperationDbName", entity.Operation),
					new MySqlParameter("@EntityClassName", entity.EntityClassName),
					new MySqlParameter("@EntityId", entity.EntityId),
					new MySqlParameter("@EntityTitle", entity.EntityTitle),
				}
			};
		}
	}
}
