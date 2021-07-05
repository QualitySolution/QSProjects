using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Tdi;
using QS.Utilities.Text;
using System.ComponentModel;

namespace QS.Project.Journal
{
	public sealed class JournalEntityConfigurator<TEntity>
		where TEntity : class, INotifyPropertyChanged, IDomainObject, new()
		//where TNode : JournalEntityNodeBase
	{
		private List<JournalEntityDocumentsConfig> documentConfigs;

		internal event EventHandler<JournalEntityConfigEventArgs> OnConfigurationFinished;

		public JournalEntityConfigurator()
		{
			documentConfigs = new List<JournalEntityDocumentsConfig>();
			entityTitleName = GetEntityTitleName();
		}

		private readonly string entityTitleName;
		private string GetEntityTitleName()
		{
			var names = DomainHelper.GetSubjectNames(typeof(TEntity));
			if(names == null || string.IsNullOrWhiteSpace(names.Nominative)) {
				throw new ApplicationException($"Для типа {typeof(TEntity)} не проставлен аттрибут AppellativeAttribute, или в аттрибуте не проставлено имя Nominative, из-за чего невозможно разрешить правильное имя документа для отображения в журнале с конфигурацией документов по умолчанию.");
			}
			return names.Nominative.StringToTitleCase();
		}

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, идентификатором документа по умолчанию по типу самого документа, и с именем взятым из описания сущности
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <typeparam name="TEntityTabDialog">Тип диалога для конфигурируемого документа</typeparam>
		public JournalEntityConfigurator<TEntity> AddDocumentConfiguration<TEntityTabDialog>(JournalParametersForDocument journalParameters = null)
			where TEntityTabDialog : class, ITdiTab
		{
			return AddDocumentConfiguration<TEntityTabDialog>(entityTitleName, journalParameters);
		}

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, идентификатором документа по умолчанию по типу самого документа, и с определенным именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <typeparam name="TEntityTabDialog">Тип диалога для конфигурируемого документа</typeparam>
		public JournalEntityConfigurator<TEntity> AddDocumentConfiguration<TEntityTabDialog>(
			string createActionTitle,
			JournalParametersForDocument journalParameters = null
		)
			where TEntityTabDialog : class, ITdiTab
		{
			Func<JournalEntityNodeBase, bool> docIdentificationFunc = node => node.EntityType == typeof(TEntity);

			return AddDocumentConfiguration<TEntityTabDialog>(createActionTitle, docIdentificationFunc, journalParameters);
		}

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, с определенным идентификатором
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <typeparam name="TEntityTabDialog">Тип диалога для конфигурируемого документа</typeparam>
		public JournalEntityConfigurator<TEntity> AddDocumentConfiguration<TEntityTabDialog>(
			Func<JournalEntityNodeBase, bool> docIdentificationFunc,
			JournalParametersForDocument journalParameters = null
		)
			where TEntityTabDialog : class, ITdiTab
		{
			return AddDocumentConfiguration<TEntityTabDialog>(entityTitleName, docIdentificationFunc, journalParameters);
		}

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, с определенным идентификатором и именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <typeparam name="TEntityTabDialog">Тип диалога для конфигурируемого документа</typeparam>
		public JournalEntityConfigurator<TEntity> AddDocumentConfiguration<TEntityTabDialog>(
			string createActionTitle, 
			Func<JournalEntityNodeBase, bool> docIdentificationFunc,
			JournalParametersForDocument journalParameters = null
		)
			where TEntityTabDialog : class, ITdiTab
		{
			Type dlgType = typeof(TEntityTabDialog);
			CheckDialogRestrictions(dlgType);

			var dlgCtorForCreateNewEntity = dlgType.GetConstructor(Type.EmptyTypes);
			var dlgCtorForOpenEntity = dlgType.GetConstructor(new[] { typeof(int) });

			Func<TEntityTabDialog> createDlgFunc = () => (TEntityTabDialog)dlgCtorForCreateNewEntity.Invoke(Type.EmptyTypes);
			Func<JournalEntityNodeBase, TEntityTabDialog> openDlgFunc = node => (TEntityTabDialog)dlgCtorForOpenEntity.Invoke(new object[] { node.Id });

			return AddDocumentConfiguration<TEntityTabDialog>(createActionTitle, createDlgFunc, openDlgFunc, docIdentificationFunc, journalParameters);
		}

		/// <summary>
		/// Добавление конфигурации документа с не стандартным опредлением диалогов, с определенным идентификатором и именем взятым из описания сущности
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createDlgFunc">Функция вызова диалога создания нового документа</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TEntityTabDialog">Тип диалога для конфигурируемого документа</typeparam>
		public JournalEntityConfigurator<TEntity> AddDocumentConfiguration<TEntityTabDialog>(
			Func<TEntityTabDialog> createDlgFunc, 
			Func<JournalEntityNodeBase, TEntityTabDialog> openDlgFunc, 
			Func<JournalEntityNodeBase, bool> docIdentificationFunc,
			JournalParametersForDocument journalParameters = null
		)
			where TEntityTabDialog : class, ITdiTab
		{
			return AddDocumentConfiguration<TEntityTabDialog>(entityTitleName, createDlgFunc, openDlgFunc, docIdentificationFunc, journalParameters);
		}

		/// <summary>
		/// Добавление функций открытия диалогов для документа с определенным идентификатором и именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <param name="createDlgFunc">Функция вызова диалога создания нового документа</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TEntityTabDialog">Тип диалога для конфигурируемого документа</typeparam>
		public JournalEntityConfigurator<TEntity> AddDocumentConfiguration<TEntityTabDialog>(
			string createActionTitle, 
			Func<TEntityTabDialog> createDlgFunc, 
			Func<JournalEntityNodeBase, TEntityTabDialog> openDlgFunc, 
			Func<JournalEntityNodeBase, bool> docIdentificationFunc,
			JournalParametersForDocument journalParameters = null
		)
			where TEntityTabDialog : class, ITdiTab
		{
			var dlgInfo = new JournalEntityDocumentsConfig(createActionTitle, createDlgFunc, openDlgFunc, docIdentificationFunc, journalParameters);
			documentConfigs.Add(dlgInfo);
			return this;
		}

		/// <summary>
		/// Добавление функций открытия диалогов для документа с определенным идентификатором без возможности создания документа
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TEntityTabDialog">Тип диалога для конфигурируемого документа</typeparam>
		public JournalEntityConfigurator<TEntity> AddDocumentConfigurationWithoutCreation<TEntityTabDialog>(
			Func<JournalEntityNodeBase, TEntityTabDialog> openDlgFunc, 
			Func<JournalEntityNodeBase, bool> docIdentificationFunc,
			JournalParametersForDocument journalParameters = null
		)
			where TEntityTabDialog : class, ITdiTab
		{
			var dlgInfo = new JournalEntityDocumentsConfig(openDlgFunc, docIdentificationFunc, journalParameters);
			documentConfigs.Add(dlgInfo);
			return this;
		}

		/// <summary>
		/// Добавление конфигурации документа с не стандартным опредлением диалогов, с определенным идентификатором и именем взятым из описания сущности
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createDlgFunc">Функция вызова диалога создания нового документа</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TEntityTabDialog">Тип диалога для конфигурируемого документа</typeparam>
		public JournalEntityConfigurator<TEntity> AddDocumentConfiguration(
			Func<ITdiTab> createDlgFunc, 
			Func<JournalEntityNodeBase, ITdiTab> openDlgFunc, 
			Func<JournalEntityNodeBase, bool> docIdentificationFunc, 
			string title = "",
			JournalParametersForDocument journalParameters = null
		)
		{
			var dlgInfo = new JournalEntityDocumentsConfig(string.IsNullOrWhiteSpace(title) ? entityTitleName : title, createDlgFunc, openDlgFunc, docIdentificationFunc, journalParameters);
			documentConfigs.Add(dlgInfo);
			return this;
		}

		/// <summary>
		/// Завершение конфигурации документа, проверка конфликтов и запись конфиругации в модель
		/// </summary>
		public void FinishConfiguration()
		{
			if(!documentConfigs.Any()) {
				throw new InvalidOperationException($"Для класса \"{typeof(TEntity)}\" должна быть определена как минимум одна конфигурация диалогов. Для ее определения необходимо вызвать метод \"{nameof(AddDocumentConfiguration)}\"");
			}
			JournalEntityConfig config = new JournalEntityConfig(typeof(TEntity), documentConfigs);
			OnConfigurationFinished?.Invoke(this, new JournalEntityConfigEventArgs(config));
		}

		private void CheckDialogRestrictions(Type dialogType)
		{
			var dlgCtorForCreateNewEntity = dialogType.GetConstructor(Type.EmptyTypes);
			if(dlgCtorForCreateNewEntity == null) {
				throw new InvalidOperationException("Диалог должен содержать конструктор без параметров для создания новой сущности");
			}

			var dlgCtorForOpenEntity = dialogType.GetConstructor(new[] { typeof(int) });
			if(dlgCtorForOpenEntity == null) {
				throw new InvalidOperationException("Диалог должен содержать конструктор принимающий id сущности для ее открытия");
			}
		}
	}

	internal class JournalEntityConfigEventArgs : EventArgs
	{
		public JournalEntityConfig Config { get; }

		public JournalEntityConfigEventArgs(JournalEntityConfig config)
		{
			Config = config ?? throw new ArgumentNullException(nameof(config));
		}

	}
}
