using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Project.Journal.Search {
	/// <summary>
	/// Класс предназначен для формирования критериев поиска для журналов на чистом SQL.
	/// </summary>
	public class SqlSearchCriterion {
		protected readonly IJournalSearch journalSearch;
		protected readonly List<(string Name, LikeMatchMode Mode)> SearchColumns = new List<(string Name, LikeMatchMode Mode)>();

		public SqlSearchCriterion(IJournalSearch journalSearch) {
			this.journalSearch = journalSearch ?? throw new ArgumentNullException(nameof(journalSearch));
		}
		
		protected LikeMatchMode LikeMatchMode { get; set; } = LikeMatchMode.StringAnywhere;

		#region Fluent
		public SqlSearchCriterion By(params string[] columns) {
			foreach(var column in columns) {
				SearchColumns.Add((column, LikeMatchMode));			
			}
			return this;
		}
		public SqlSearchCriterion By(params (string Name, LikeMatchMode Mode)[] columns) {
			foreach(var column in columns) {
				SearchColumns.Add(column);			
			}
			return this;
		}
		
		public SqlSearchCriterion WithLikeMode(LikeMatchMode matchMode) {
			LikeMatchMode = matchMode;
			return this;
		}
		#endregion

		#region Формирование запроса
		public string Finish() {
			if(journalSearch.SearchValues == null || !journalSearch.SearchValues.Any()) {
				return String.Empty;
			}

			var listConjunction = new List<string>();
			foreach(var sv in journalSearch.SearchValues) {
				if(string.IsNullOrWhiteSpace(sv)) {
					continue;
				}

				var listDisjunction = new List<string>();
				foreach(var column in SearchColumns) {
					var columnConduction = GetConductionByMode(column, sv);
					if(columnConduction != null) 
						listDisjunction.Add(columnConduction);
				}
				listConjunction.Add($"({String.Join(" OR ", listDisjunction)})");
			}
			
			return $"{String.Join(" AND ", listConjunction)}";
		}
		
		string GetConductionByMode((string Name, LikeMatchMode Mode) column, string searchValue) {
			switch(column.Mode) {
				case LikeMatchMode.StringAnywhere:
					return $"{column.Name} LIKE '%{searchValue}%'";
				case LikeMatchMode.UnsignedNumberEqual:
					if(ulong.TryParse(searchValue, out ulong ulongValue)) {
						return $"{column.Name} = {ulongValue}";
					}
					break;
				default:
					throw new NotImplementedException();
			}

			return null;
		}
		#endregion
	}

	public enum LikeMatchMode {
		StringAnywhere,
		UnsignedNumberEqual,
	}
}
