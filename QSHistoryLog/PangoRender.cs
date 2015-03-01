using System;
using System.Text;
using DiffPlex.DiffBuilder.Model;

namespace QSHistoryLog
{
	public static class PangoRender
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public const string PangoInsertFormat = "<span background=\"#A6F3A6\">{0}</span>";
		public const string PangoDeleteFormat = "<span background=\"#F8CBCB\">{0}</span>";
		public const string PangoChangeFormat = "<span background=\"#F0DB88\">{0}</span>";

		public static string RenderDiffLines(DiffPaneModel diffModel)
		{
			StringBuilder result = new StringBuilder ();
			foreach (var line in diffModel.Lines)
			{
				if (line.Type == ChangeType.Deleted)
					result.AppendLine (String.Format (PangoDeleteFormat, line.Text));
				else if (line.Type == ChangeType.Inserted)
					result.AppendLine (String.Format ( PangoInsertFormat, line.Text));
				else if (line.Type == ChangeType.Unchanged)
					result.AppendLine (line.Text);
				else if (line.Type == ChangeType.Modified)
					result.AppendLine (RenderDiffWords(line));
				else if (line.Type == ChangeType.Imaginary)
				{
					//fillColor = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
					//AddImaginaryLine(textBox, lineNumber);
					logger.Debug ("Imaginary line {0}", line.Text);
				}
			}
			return result.ToString ().TrimEnd ('\n');
		}

		private static string RenderDiffWords(DiffPiece line)
		{
			StringBuilder result = new StringBuilder ();
			foreach (var word in line.SubPieces)
			{
				if (word.Type == ChangeType.Deleted)
					result.AppendFormat (PangoDeleteFormat, word.Text);
				else if (word.Type == ChangeType.Inserted)
					result.AppendFormat (PangoInsertFormat, word.Text);
				else if (word.Type == ChangeType.Modified)
					result.Append (RenderDiffCharacter(word));
				else if (word.Type == ChangeType.Imaginary)
					continue;
				else
					result.Append (word.Text);
			}
			return result.ToString ();
		}

		private static string RenderDiffCharacter(DiffPiece word)
		{
			StringBuilder result = new StringBuilder ();
			foreach (var characters in word.SubPieces)
			{
				if (characters.Type == ChangeType.Deleted)
					result.AppendFormat (PangoDeleteFormat, characters.Text);
				else if (characters.Type == ChangeType.Inserted)
					result.AppendFormat (PangoInsertFormat, characters.Text);
				else if (characters.Type == ChangeType.Imaginary)
					continue;
				else
					result.Append (characters.Text);
			}
			return result.ToString ();
		}

	}
}

