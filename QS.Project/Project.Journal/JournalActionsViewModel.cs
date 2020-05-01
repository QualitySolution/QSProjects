using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.ViewModels;

namespace QS.Project.Journal
{
	public class JournalActionsViewModel : ViewModelBase
	{
		private object[] selectedObjs;
		public object[] SelectedObjs {
			get => selectedObjs;
			set => SetField(ref selectedObjs, value);
		}

		private bool addButtonVisibility = true;
		public bool AddButtonVisibility {
			get => addButtonVisibility;
			set => SetField(ref addButtonVisibility, value);
		}

		private bool addButtonSensitivity = true;
		public bool AddButtonSensitivity {
			get => addButtonSensitivity;
			set => SetField(ref addButtonSensitivity, value);
		}

		private bool editButtonVisibility = true;
		public bool EditButtonVisibility {
			get => editButtonVisibility;
			set => SetField(ref editButtonVisibility, value);
		}

		private bool editButtonSensitivity;
		public bool EditButtonSensitivity {
			get => editButtonSensitivity;
			set => SetField(ref editButtonSensitivity, value);
		}

		private bool deleteButtonVisibility = true;
		public bool DeleteButtonVisibility {
			get => deleteButtonVisibility;
			set => SetField(ref deleteButtonVisibility, value);
		}

		private bool deleteButtonSensitivity;
		public bool DeleteButtonSensitivity {
			get => deleteButtonSensitivity;
			set => SetField(ref deleteButtonSensitivity, value);
		}

		private bool selectButtonVisibility;
		public bool SelectButtonVisibility {
			get => selectButtonVisibility;
			set => SetField(ref selectButtonVisibility, value);
		}

		private bool selectButtonSensitivity;
		public bool SelectButtonSensitivity {
			get => selectButtonSensitivity;
			set => SetField(ref selectButtonSensitivity, value);
		}

		public Action DefaultAddAction;
		public Action<object[]> DefaultSelectAction;
		public Action<object[]> DefaultEditAction;
		public Action<object[]> DefaultDeleteAction;

		public JournalActionsViewModel()
		{
			CreateCommands();
		}

		private void CreateCommands()
		{
			CreateAddCommand();
			CreateEditCommand();
			CreateDeleteCommand();
			CreateSelectCommand();
		}

		public DelegateCommand AddCommand { get; private set; }
		private void CreateAddCommand()
		{
			AddCommand = new DelegateCommand(
				() => DefaultAddAction?.Invoke(),
				() => true
			);
		}

		public DelegateCommand EditCommand { get; private set; }
		private void CreateEditCommand()
		{
			EditCommand = new DelegateCommand(
				() => DefaultEditAction?.Invoke(SelectedObjs),
				() => SelectedObjs.Any()
			);
		}

		public DelegateCommand DeleteCommand { get; private set; }
		private void CreateDeleteCommand()
		{
			DeleteCommand = new DelegateCommand(
				() => DefaultDeleteAction?.Invoke(SelectedObjs),
				() => SelectedObjs.Any()
			);
		}

		public DelegateCommand SelectCommand { get; private set; }
		private void CreateSelectCommand()
		{
			SelectCommand = new DelegateCommand(
				() => DefaultSelectAction?.Invoke(SelectedObjs),
				() => SelectedObjs.Any()
			);
		}
	}

	public class JournalActionsModel<TEntity, TNode>
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TNode : JournalEntityNodeBase
	{
		public Dictionary<Type, JournalEntityConfig<TNode>> EntityConfigs { get; private set; }

		protected JournalEntityConfigurator<TEntity, TNode> RegisterEntity()
		{
			var configurator = new JournalEntityConfigurator<TEntity, TNode>();
			configurator.OnConfigurationFinished += (sender, e) => {
				var config = e.Config;
				if(EntityConfigs.ContainsKey(config.EntityType)) {
					throw new InvalidOperationException($"Конфигурация для сущности ({config.EntityType.Name}) уже была добавлена.");
				}
				EntityConfigs.Add(config.EntityType, config);
			};
			return configurator;
		}

		public JournalActionsModel()
		{
			EntityConfigs = new Dictionary<Type, JournalEntityConfig<TNode>>();
		}
	}

	/*
	public class JournalActionsViewModel : ViewModelBase
	{

	}
	*/
}
