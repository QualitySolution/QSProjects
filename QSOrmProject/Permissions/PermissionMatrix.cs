using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Newtonsoft.Json;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QSOrmProject.Permissions
{
	public class PermissionMatrix<TPermissionEnum, TPerEntity> : IPermissionMatrix
		where TPerEntity : IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public int PermissionCount { get; private set; }
		public int ColumnCount { get; private set; }

		public PermissionMatrix()
		{

		}

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
		TPermissionEnum[] Permissions;
		bool[,] allowed;

		public void Init()
		{
			Permissions = Enum.GetValues(typeof(TPermissionEnum)).Cast<TPermissionEnum>().ToArray();
			permissionNames = Enum.GetValues(typeof(TPermissionEnum)).Cast<Enum>()
	   			.Select(x => AttributeUtil.GetEnumTitle(x))
	   			.ToArray();
			PermissionCount = PermissionNames.Length;

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var entities = uow.GetAll<TPerEntity>();
				columnNames = entities.Select(x => DomainHelper.GetTitle(x)).ToArray();
				Entities = entities.ToArray();
			}
			ColumnCount = Entities.Length;

			allowed = new bool[PermissionCount, ColumnCount];

		}

		public void ParseJson(string json)
		{
			//Clear
			for(uint row = 0; row < PermissionCount; row++)
				for(uint col = 0; col < ColumnCount; col++)
					allowed[row, col] = false;

			if(String.IsNullOrWhiteSpace(json))
				return;
			
			//Set
			var dict = JsonConvert.DeserializeObject<Dictionary<TPermissionEnum, List<int>>>(json);
			foreach(var pair in dict) {
				foreach(var id in pair.Value) {
					var ix = IndexOfColumnById(id);
					if(ix == -1)
						logger.Error("Объект типа {0} c id={1} не найден. Хотя был указан в правах пользователя. Права на этот объект пропущены.", typeof(TPerEntity), id);
					else
						this[pair.Key, id] = true;
				}
			}
		}

		public string GetJson()
		{
			var dict = new Dictionary<TPermissionEnum, List<int>>();

			for(uint row = 0; row < PermissionCount; row++) {
				var ids = new List<int>();
				for(uint col = 0; col < ColumnCount; col++)
					if(allowed[row, col])
						ids.Add(Entities[col].Id);

				if(ids.Count > 0)
					dict.Add(Permissions[row], ids);
			}

			return JsonConvert.SerializeObject(dict);
		}

		#region Помощники

		int IndexOf(TPermissionEnum permission)
		{
			return Array.FindIndex(Permissions, x => x.Equals(permission));
		}

		int IndexOfColumnById(int id)
		{
			return Array.FindIndex(Entities, x => x.Id == id);
		}

		int IndexOf(TPerEntity entity)
		{
			return Array.FindIndex(Entities, x => x.Id == entity.Id);
		}

		#endregion

		#region Индексаторы

		public bool this[int permissionIx, int columnIx] {
			get {
				return allowed[permissionIx, columnIx];
			}
			set {
				allowed[permissionIx, columnIx] = value;
			}
		}

		public bool this[TPermissionEnum permission, int id] {
			get {
				return allowed[IndexOf(permission), IndexOfColumnById(id)];
			}
			set {
				allowed[IndexOf(permission), IndexOfColumnById(id)] = value;
			}
		}

		public bool this[TPermissionEnum permission, TPerEntity entity] {
			get {
				return allowed[IndexOf(permission), IndexOf(entity)];
			}
			set {
				allowed[IndexOf(permission), IndexOf(entity)] = value;
			}
		}

		#endregion

		#region Списки

		public IEnumerable<TPerEntity> Allowed(TPermissionEnum permission)
		{
			var row = IndexOf(permission);
			for(uint col = 0; col < ColumnCount; col++)
			{
				if(allowed[row, col])
					yield return Entities[col];
			}
		}

		/// <summary>
		/// Получаем все позволенные пользователю разрешения, в виде пар значений.
		/// </summary>
		/// <returns>KeyValuePair где ключ это права, а значение это объект.</returns>
		public IEnumerable<KeyValuePair<TPermissionEnum, TPerEntity>> Allowed()
		{
			for(uint row = 0; row < PermissionCount; row++) {
				for(uint col = 0; col < ColumnCount; col++) {
					if(allowed[row, col])
						yield return new KeyValuePair<TPermissionEnum, TPerEntity>(Permissions[row], Entities[col]);
				}
			}
		}

		/// <summary>
		/// Получаем все объекты для которых есть хотя бы одно из разрешений.
		/// </summary>
		public IEnumerable<TPerEntity> AnyPermissions()
		{
			for(uint col = 0; col < ColumnCount; col++) {
				for(uint row = 0; row < PermissionCount; row++) {
					if(allowed[row, col])
					{
						yield return Entities[col];
						break;
					}
				}
			}
		}

		/// <summary>
		/// Получаем все разрешения которые указаны хотя бы для одного объекта.
		/// </summary>
		public IEnumerable<TPermissionEnum> AnyEntities()
		{
			for(uint row = 0; row < PermissionCount; row++) {
				for(uint col = 0; col < ColumnCount; col++) {
					if(allowed[row, col]) {
						yield return Permissions[row];
						break;
					}
				}
			}
		}

  		#endregion
	}
}
