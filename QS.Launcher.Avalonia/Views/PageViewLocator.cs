using System;
using System.Collections.Generic;
using Avalonia.Controls;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using QS.Launcher.Views.Pages;
using QS.Launcher.Views.Pages.DataBase;

namespace QS.Launcher.Views;

/// <summary>
/// Сопоставляет VM страницы её View,
/// чтобы добавить новый тип страницы, достаточно зарегистрировать здесь пару (VM, View)
/// </summary>
public class PageViewLocator {
	private readonly Dictionary<Type, Func<CarouselPageVM, UserControl>> factories;

	public PageViewLocator() {
		factories = new Dictionary<Type, Func<CarouselPageVM, UserControl>> {
			[typeof(LoginVM)] = vm => new LoginView((LoginVM)vm),
			[typeof(DataBasesVM)] = vm => new DataBasesView((DataBasesVM)vm),
			[typeof(UserManagementVM)] = vm => new UserManagementView((UserManagementVM)vm),
			[typeof(CreateDbSettingsVM)] = vm => new CreateDbSettingsView((CreateDbSettingsVM)vm),
			[typeof(ImportDbSettingsVM)] = vm => new ImportDbSettingsView((ImportDbSettingsVM)vm),
			[typeof(BackupDbSettingsVM)] = vm => new BackupDbSettingsView((BackupDbSettingsVM)vm),
			[typeof(CreateDataBaseProgressVM)] = vm => new CreateDataBaseProgressView((CreateDataBaseProgressVM)vm),
		};
	}

	public UserControl Resolve(CarouselPageVM page) {
		if(page == null)
			throw new ArgumentNullException(nameof(page));
		if(factories.TryGetValue(page.GetType(), out var factory))
			return factory(page);
		throw new InvalidOperationException($"Не зарегистрирован View для страницы {page.GetType().Name}.");
	}
}
