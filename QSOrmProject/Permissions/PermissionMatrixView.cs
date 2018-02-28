using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QSProjectsLib.Permissions;

namespace QSOrmProject.Permissions
{
	
	public partial class PermissionMatrixView : Gtk.Bin, IPermissionsView
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public string ViewName { get; private set; }

		public string DBFieldName { get; private set; }

		public string DBFieldValue {
			get { return permissionMatrix.GetJson(); }
			set {
				permissionMatrix.ParseJson(value);
				ReloadValues();
			}
		}

		IPermissionMatrix permissionMatrix;

		CheckButton[,] checkButtons;

		bool internalSets = false;

		//Вложенные боксы, нужны только для того чтобы галочка была посередине ячейки. При этом можно было вычислить размеры каждой ячейки, по виджету, который занимает все доступное пространство.
		HBox[,] cells;

		public PermissionMatrixView(IPermissionMatrix permissionMatrix, string title, string dbfield)
		{
			this.Build();
			this.permissionMatrix = permissionMatrix;
			ViewName = title;
			DBFieldName = dbfield;

			tableAccsessMatrix.NColumns = 2 + (uint)permissionMatrix.ColumnNames.Length;
			tableAccsessMatrix.NRows = 2 + (uint)permissionMatrix.PermissionNames.Length;

			//Рисуем строки
			var labelAll = new Label("Все права");
			labelAll.SetAlignment(1, 0.5f);
			tableAccsessMatrix.Attach(labelAll, 0, 1, 1, 2);
			uint row = 2;
			foreach(var permiss in permissionMatrix.PermissionNames)
			{
				var labelPerm = new Label(permiss);
				labelPerm.SetAlignment(1, 0.5f);
				tableAccsessMatrix.Attach(labelPerm, 0, 1, row, row + 1);
				row++;
			}

			//Рисуем колонки
			labelAll = new Label("Все");
			labelAll.Angle = 90;
			labelAll.SetAlignment(0.5f, 1);
			tableAccsessMatrix.Attach(labelAll, 1, 2, 0, 1);
			uint col = 2;
			foreach(var column in permissionMatrix.ColumnNames) {
				var labelColumn = new Label(column);
				labelColumn.Angle = 90;
				labelColumn.SetAlignment(0.5f, 1);
				tableAccsessMatrix.Attach(labelColumn, col, col + 1, 0, 1);
				col++;
			}

			//Заполняем главную таблицу
			checkButtons = new CheckButton[permissionMatrix.PermissionNames.Length + 1, permissionMatrix.ColumnNames.Length + 1];
			cells = new HBox[permissionMatrix.PermissionNames.Length + 1, permissionMatrix.ColumnNames.Length + 1];

			for(uint crow = 0; crow <= permissionMatrix.PermissionCount; crow++)
				for(uint ccol = 0; ccol <= permissionMatrix.ColumnCount; ccol++) 
					PackCell(crow, ccol);

			tableAccsessMatrix.ExposeEvent += tableAccsessMatrix_ExposeEvent;
			tableAccsessMatrix.ShowAll();
		}

		private yCheckButton PackCell(uint row, uint col)
		{
			Contract.Ensures(Contract.Result<yCheckButton>() != null);
			var check = new yCheckButton();
			//Используем Gdk.Point просто чтобы не создавать новой структуры. Нужны поля X и Y;
			check.Tag = new Gdk.Point((int)col, (int)row);
			check.Active = GetCellValue(row, col);
			check.SetAlignment(0.5f, 0.5f);
			check.Toggled += OnCheckCellToggled;
			checkButtons[row, col] = check;
			var box = new HBox();
			cells[row, col] = box;
			box.PackStart(check, true, false, 0);
			tableAccsessMatrix.Attach(box, col + 1, col + 2, row + 1, row + 2);
			return check;
		}

		bool GetCellValue(uint row, uint col)
		{
			if(row == 0 && col == 0)
				return Enumerable.Range(0, permissionMatrix.PermissionCount).All(y => Enumerable.Range(0, permissionMatrix.ColumnCount).All(x => permissionMatrix[y, x]));
			if(row == 0)
				return Enumerable.Range(0, permissionMatrix.PermissionCount).All(i => permissionMatrix[i, (int)col - 1]);
			if(col == 0)
				return Enumerable.Range(0, permissionMatrix.ColumnCount).All(i => permissionMatrix[(int)row - 1, i]);

			return permissionMatrix[(int)row - 1, (int)col - 1];
		}

		void SetCellValue(uint row, uint col, bool value)
		{
			if(row == 0 && col == 0)
				Enumerable.Range(0, permissionMatrix.PermissionCount).ToList().ForEach(y => Enumerable.Range(0, permissionMatrix.ColumnCount).ToList().ForEach(x => permissionMatrix[y, x] = value));
			else if(row == 0)
				Enumerable.Range(0, permissionMatrix.PermissionCount).ToList().ForEach(i => permissionMatrix[i, (int)col - 1] = value);
			else if(col == 0)
				Enumerable.Range(0, permissionMatrix.ColumnCount).ToList().ForEach(i => permissionMatrix[(int)row - 1, i] = value);
			else
			{
				permissionMatrix[(int)row - 1, (int)col - 1] = value;
				return;
			}
			ReloadValues();
		}

		void ReloadValues()
		{
			internalSets = true;
			for(uint crow = 0; crow <= permissionMatrix.PermissionCount; crow++)
				for(uint ccol = 0; ccol <= permissionMatrix.ColumnCount; ccol++)
					checkButtons[crow, ccol].Active = GetCellValue(crow, ccol);

			internalSets = false;
		}

		void OnCheckCellToggled(object sender, EventArgs e)
		{
			if(internalSets)
				return;
			var check = sender as yCheckButton;
			var point = (Gdk.Point)check.Tag;
			SetCellValue((uint)point.Y, (uint)point.X, check.Active);
		}

		void tableAccsessMatrix_ExposeEvent(object o, Gtk.ExposeEventArgs args)
		{
			int x, y, w, h, x0, y0;
			h = tableAccsessMatrix.Allocation.Height;
			w = tableAccsessMatrix.Allocation.Width;
			x0 = tableAccsessMatrix.Allocation.X;
			y0 = tableAccsessMatrix.Allocation.Y;

			for(int col = 0; col < checkButtons.GetLength(1); col++) {
				if(checkButtons[1, col] == null)
					continue;

				var gc = this.Style.ForegroundGC(this.State);
				gc.SetLineAttributes(col != 0 ? 1 : 2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
				x = cells[1, col].Allocation.X - 3;
				//checkButtons[1, col].TranslateCoordinates(tableAccsessMatrix, -1, 0, out x, out y);
				tableAccsessMatrix.GdkWindow.DrawLine(gc, x, y0, x, y0 + h);
			}
			for(int row = 0; row < checkButtons.GetLength(0); row++) {
				if(checkButtons[row, 1] == null)
					continue;
				checkButtons[row, 1].TranslateCoordinates(tableAccsessMatrix, 0, -3, out x, out y);

				var gc = this.Style.ForegroundGC(this.State);
				gc.SetLineAttributes(row != 0 ? 1 : 2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

				tableAccsessMatrix.GdkWindow.DrawLine(gc, x0, y0 + y, x0 + w, y0 + y);
			}
		}
	}
}
