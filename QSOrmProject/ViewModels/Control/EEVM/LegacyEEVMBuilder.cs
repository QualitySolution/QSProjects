using System;
using System.ComponentModel;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.Entity;

namespace QS.ViewModels.Control.EEVM
{
	public class LegacyEEVMBuilder<TBindedEntity, TEntity> : CommonEEVMBuilder<TBindedEntity, TEntity>
		where TBindedEntity : class, INotifyPropertyChanged
		where TEntity : class, IDomainObject
	{
		readonly LegacyEEVMBuilderFactory<TBindedEntity> legacyFactory;

		public LegacyEEVMBuilder(LegacyEEVMBuilderFactory<TBindedEntity> builderFactory, Expression<Func<TBindedEntity, TEntity>> bindedProperty) : base(builderFactory, bindedProperty)
		{
			this.legacyFactory = builderFactory;
			factory = builderFactory;
		}

		#region Fluent Config

		public LegacyEEVMBuilder<TBindedEntity, TEntity> UseOrmReferenceJournal()
		{
			EntitySelector = new OrmReferenceSelector(legacyFactory.DialogTab, factory.UnitOfWork, typeof(TEntity));
			return this;
		}

		public LegacyEEVMBuilder<TBindedEntity, TEntity> UseOrmReferenceJournal(QueryOver itemsQuery)
		{
			EntitySelector = new OrmReferenceSelector(legacyFactory.DialogTab, factory.UnitOfWork, itemsQuery);
			return this;
		}

		public LegacyEEVMBuilder<TBindedEntity, TEntity> UseOrmReferenceJournal(ICriteria itemsCriteria)
		{
			EntitySelector = new OrmReferenceSelector(legacyFactory.DialogTab, factory.UnitOfWork, itemsCriteria);
			return this;
		}

		public LegacyEEVMBuilder<TBindedEntity, TEntity> UseOrmReferenceJournalAndAutocompleter()
		{
			EntitySelector = new OrmReferenceSelector(legacyFactory.DialogTab, factory.UnitOfWork, typeof(TEntity));
			EntityAutocompleteSelector = new OrmReferenceAutocompleteSelector<TEntity>(factory.UnitOfWork);
			return this;
		}

		public LegacyEEVMBuilder<TBindedEntity, TEntity> UseOrmReferenceJournalAndAutocompleter(QueryOver itemsQuery)
		{
			EntitySelector = new OrmReferenceSelector(legacyFactory.DialogTab, factory.UnitOfWork, itemsQuery);
			EntityAutocompleteSelector = new OrmReferenceAutocompleteSelector<TEntity>(factory.UnitOfWork, itemsQuery);
			return this;
		}

		public LegacyEEVMBuilder<TBindedEntity, TEntity> UseOrmReferenceJournalAndAutocompleter(ICriteria itemsCriteria)
		{
			EntitySelector = new OrmReferenceSelector(legacyFactory.DialogTab, factory.UnitOfWork, itemsCriteria);
			EntityAutocompleteSelector = new OrmReferenceAutocompleteSelector<TEntity>(factory.UnitOfWork, itemsCriteria);
			return this;
		}

		public LegacyEEVMBuilder<TBindedEntity, TEntity> UseTdiEntityDialog()
		{
			EntityDlgOpener = new OrmObjectDialogOpener<TEntity>(legacyFactory.DialogTab);
			return this;
		}

		#endregion
	}
}
