using System;
using System.Linq;
using QSSupportLib.Serial;

namespace QSSupportLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public class SerialNumberEntry : Gtk.Entry
	{
		private int? SetPos;

		public SerialNumberEntry()
		{
		}

		protected override void OnTextInserted(string text, ref int position)
		{
			base.OnTextInserted(text, ref position);

			Console.WriteLine("Insert = {1}, Position = {0}", position, text);
			if(SetPos.HasValue)
			{
				position = SetPos.Value;
				Console.WriteLine("Set Position = {0}", position);
				SetPos = null;
			}
		}

		protected override void OnTextDeleted(int start_pos, int end_pos)
		{
			Console.WriteLine("Delete from = {0} - {1}, Position = {2}",start_pos, end_pos,  Position);
			base.OnTextDeleted(start_pos, end_pos);

			if(SetPos.HasValue)
			{
				Position = SetPos.Value;
				Console.WriteLine("Set Position = {0}", Position);
				SetPos = null;
			}
		}

		protected override void OnChanged()
		{
			string digits = Text.Replace("-", "");
			string newStr = SerialCommon.AddHyphens(digits);
			int oldHyphens = (Position > Text.Length ? Text : Text.Substring(0,  Position))
				.Count(c => c == '-');
			int PosForNewstr = Position + (newStr.Length - Text.Length);
			int newHyphens = ( PosForNewstr > newStr.Length ? newStr : newStr.Substring(0, PosForNewstr))
				.Count(c => c == '-');
			if (Text != newStr)
			{
				int oldPos = Position;
				Text = newStr;
				int newPos = oldPos + newHyphens - oldHyphens;

				if (newPos < newStr.Length && (newStr[newPos - 1] == '-' || newStr[newPos] == '-'))
					newPos+=1;
				SetPos = newPos;
			}

			base.OnChanged();
		}
	}
}

