﻿using System;
using QS.Navigation;
using QS.ViewModels.Extension;

namespace QS.Tdi
{
	public interface ITdiTabParent
	{
        /// <summary>
        /// Добавить подчинённую вкладку.
        /// </summary>
        /// <param name="masterTab">Вкладка, для которой надо открыть подчинённую.</param>
        /// <param name="slaveTab">Подчинённая вкладка.</param>
        void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab, IDialogDocumentation documentation = null);

        /// <summary>
        /// Добавить вкладку.
        /// </summary>
        /// <param name="tab">Вкладка, которую надо открыть.</param>
        /// <param name="afterTab">После какой вкладки добавлять. Обычно после выбранной - в этом случае аргумент "this".</param>
        /// <param name="canSlided">Может ли вкладка открыться рядом с журналом. "True" и открывается из журнала - появляется справа, "false" - открывается в отдельной вкладке.</param>
		void AddTab(ITdiTab tab, ITdiTab afterTab, bool canSlided = true, IDialogDocumentation documentation = null);

		/// <summary>
		/// Выполняем проверку есть ли у вкладки подчиненные, если есть выводится сообщение пользователю с переключение на незакрытую подчинённую вкладку.
		/// </summary>
		/// <returns><c>true</c>, если подчиненных вкладки есть, если все готово для закрытия текущей <c>false</c>.</returns>
		/// <param name="tab">Вкладка для проверки.</param>
		bool CheckClosingSlaveTabs(ITdiTab tab);

		ITdiTab FindTab(string hashName, string masterHashName = null);

		void SwitchOnTab(ITdiTab tab);

		/// <summary>
		/// Попросить закрыть вкладку. Вкладка может не закрыться, если на ней есть изменения и пользователь отменит закрытие.
		/// </summary>
		/// <returns><c>true</c>, если вкладка была закрыта, <c>false</c> если нет.</returns>
		/// <param name="tab">Вкладка которую нужно закрыть</param>
		bool AskToCloseTab(ITdiTab tab, CloseSource source = CloseSource.External);

		/// <summary>
		/// Принудительно закрыть вкладку без вопросов.
		/// </summary>
		/// <param name="tab">Вкладка</param>
		void ForceCloseTab(ITdiTab tab, CloseSource source = CloseSource.External);

		ITdiTab OpenTab(Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, Type[] argTypes = null, object[] args = null);

		/// <summary>
		/// Добавить вкладку.
		/// </summary>
		/// <param name="tab">Вкладка, которую надо открыть.</param>
		/// <param name="afterTab">После какой вкладки добавлять. Обычно после выбранной - в этом случае аргумент "this".</param>
		/// <param name="canSlided">Может ли вкладка открыться рядом с журналом. "True" и открывается из журнала - появляется справа, "false" - открывается в отдельной вкладке.</param>
		ITdiTab OpenTab(string hashName, Func<ITdiTab> newTabFunc, ITdiTab afterTab = null, bool canSlided = true);

		/// <summary>
		/// Открываем вкладку с автоматическим вызовом конструктора вкладки с переданными функции аргументами.
		/// </summary>
		ITdiTab OpenTab<TTab>(ITdiTab afterTab = null) where TTab : ITdiTab;
		ITdiTab OpenTab<TTab, TArg1>(TArg1 arg1, ITdiTab afterTab = null) where TTab : ITdiTab;
		ITdiTab OpenTab<TTab, TArg1, TArg2>(TArg1 arg1, TArg2 arg2, ITdiTab afterTab = null) where TTab : ITdiTab;
		ITdiTab OpenTab<TTab, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, ITdiTab afterTab = null) where TTab : ITdiTab;
	}
}
