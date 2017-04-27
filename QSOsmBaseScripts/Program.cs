using System;
using System.Diagnostics;

namespace OsmBaseScripts
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			const string fixBaseScript = "./PerlScripts/fix_base.pl";
			const string deleteWrongScript = "./PerlScripts/delete_wrong.pl";
			ProcessStartInfo fixBase = new ProcessStartInfo ("perl", fixBaseScript);
			ProcessStartInfo deleteWrong = new ProcessStartInfo ("perl", deleteWrongScript);
			Process proc;

			while (true) {
				Console.Clear ();
				Console.WriteLine ("Выберите действие из списка:");
				Console.WriteLine ("\t1. Наполнить исходную базу необходимыми данными. (Проставить районы, регионы, улицы).");
				Console.WriteLine ("\t2. Заполнение наших таблиц osm данными.");
				Console.WriteLine ("\t3. Выход.");
				Console.WriteLine ("Введите цифру, соответствующую вашему выбору.");
				string choice = Console.ReadLine ();
				Console.Clear ();
				switch (choice) {
				case "1":
					Console.WriteLine ("Происходит заполнение базы, пожалуйста подождите...");
					proc = Process.Start (fixBase);
					proc.WaitForExit ();
					Console.WriteLine ("Происходит очистка базы. Процесс может занять до часа времени. Пожалуйста подождите...");
					proc = Process.Start (deleteWrong);
					proc.WaitForExit ();
					break;
				case "2":
					Console.WriteLine ("Происходит перенос osm данных в наши таблицы, пожалуйста подождите...");
					CitiesWorker.FillCities ();
					StreetsWorker.FillStreets ();
					break;
				case "3":
					Environment.Exit (0);
					break;
				default:
					Console.WriteLine ("Действие с таким номером отсутствует.");
					break;
				}
				Console.WriteLine ("Нажмите любую клавишу для продолжения.");
				Console.ReadKey ();
			}
		}
	}
}
