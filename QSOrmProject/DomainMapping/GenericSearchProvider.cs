using System;
using System.Collections.Generic;
using System.Linq;

namespace QSOrmProject.DomainMapping
{
	public class GenericSearchProvider<TEntity> : ISearchProvider
	{
		private readonly List<Func<TEntity, string>> searchBy = new List<Func<TEntity, string>>();

		#region ISearchProvider implementation

		public bool Match(object entity, string searchText)
		{
			if (String.IsNullOrWhiteSpace(searchText))
				return true;
			
			return Match((TEntity)entity, searchText);
		}

		public bool Match(TEntity entity, string searchText)
		{
			foreach(var field in searchBy)
			{
				var str = field(entity);
				if (String.IsNullOrEmpty(str))
					continue;
				if (str.IndexOf (searchText, StringComparison.CurrentCultureIgnoreCase) > -1) 
					return true;
			}
			return false;
		}
			
		public System.Collections.IList FilterList(System.Collections.IList sourcelist, string searchText)
		{
			if (String.IsNullOrWhiteSpace(searchText))
				return sourcelist;

			var genericList = sourcelist as IList<TEntity>;
			if(genericList != null)
			{
				return genericList.Where(e => Match(e, searchText)).ToList();
			}
			else
			{
				return sourcelist.Cast<TEntity>().Where(e => Match(e, searchText)).ToList();
			}
		}

		#endregion

		public GenericSearchProvider()
		{
		}

		public void AddSearchByFunc(Func<TEntity, string> func)
		{
			searchBy.Add(func);
		}
	}
}

