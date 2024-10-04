using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.Dialog;
using QS.Launcher.Views;
using QS.Launcher.Views.Pages;
using QS.Project.Avalonia;
using QS.Project.Avalonia.Interactive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace QS.Launcher;
public static partial class DependencyInjection {
	public static IServiceCollection AddPages(this IServiceCollection services) {
		return services
			.AddSingleton<MainWindow>()
			.AddSingleton<UserControl, LoginView>()
			.AddSingleton<UserControl, DataBasesView>()
			.AddSingleton<UserControl, UserManagementView>()
			.AddSingleton<UserControl, BaseManagementView>();
	}

	public static IServiceCollection AddConnections(this IServiceCollection services, IEnumerable<ConnectionInfo> connectionInfos, string? oldConfigName = null) {
		List<Dictionary<string, string>> connectionDefinitions = [];
		try {
			using(var stream = File.Open("connections.json", FileMode.Open)) {
				try {
					connectionDefinitions = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(stream);
				}
				catch(JsonException jsonEx) {
					DialogWindow.Error(jsonEx.Message, "Файл с подключениями некорректен");
				}
			}
		}
		catch(FileNotFoundException fileEx) {
			// Trying to find old ini-config and update it to new json-config
			if(!string.IsNullOrEmpty(oldConfigName)) {

				string fullOldConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), oldConfigName);

				if(File.Exists(fullOldConfigPath)) {
					connectionDefinitions = Configuration.ConfigTransition.FromOldIniConfig(fullOldConfigPath);
					DialogWindow.Info("Найден файл с подключениями старой версии, будет создан новый на его основе", "Конфигурация обновлена");
				}
				else
					DialogWindow.Info("Будет создан новый", "Файл с подключениями не найден");

			}
		}

		foreach(var parameters in connectionDefinitions)
			services.AddSingleton(
				connectionInfos.First(ci => ci.Title == parameters["Title"]).CreateConnection(parameters));

		return services;
	}

	public static IServiceCollection AddInteractive(this IServiceCollection services) {
		return services
			.AddSingleton<IInteractiveMessage, AvaloniaInteractiveMessage>();
	}
}
