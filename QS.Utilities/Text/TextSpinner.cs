using System;
namespace QS.Utilities.Text
{
	public interface ISpinnerTemplate {
		uint Interval { get; }
		string[] Frames { get; }
	}

	/// <summary>
	/// Не доступен на используемом по умолчанию в GTK 2 на Windows шрифте.
	/// </summary>
	public class SpinnerTemplateClock : ISpinnerTemplate
	{
		public uint Interval => 100;
		public string[] Frames => new string[] {"🕛", "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙","🕚"};
	}

	/// <summary>
	/// Не доступен на используемом по умолчанию в GTK 2 на Windows шрифте.
	/// </summary>
	public class SpinnerTemplateEarth : ISpinnerTemplate
	{
		public uint Interval => 180;
		public string[] Frames => new string[] { "🌍", "🌎","🌏" };
	}

	/// <summary>
	/// Не доступен на используемом по умолчанию в GTK 2 на Windows шрифте.
	/// </summary>
	public class SpinnerTemplateMoon : ISpinnerTemplate
	{
		public uint Interval => 80;
		public string[] Frames => new string[] { "🌑", "🌒", "🌓", "🌔", "🌕", "🌖", "🌗", "🌘" };
	}

	/// <summary>
	/// Не доступен на используемом по умолчанию в GTK 2 на Windows шрифте.
	/// </summary>
	public class SpinnerTemplateDots : ISpinnerTemplate
	{
		public uint Interval => 80;
		public string[] Frames => new string[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
	}

	public class SpinnerTemplateLine : ISpinnerTemplate
	{
		public uint Interval => 130;
		public string[] Frames => new string[] { "-", "\\", "|", "/" };
	}

	public class SpinnerTemplateArrow : ISpinnerTemplate
	{
		public uint Interval => 120;
		public string[] Frames => new string[] { "▹▹▹▹▹", "▸▹▹▹▹", "▹▸▹▹▹", "▹▹▸▹▹", "▹▹▹▸▹", "▹▹▹▹▸" };
	}

	public class SpinnerTemplateBouncingBar : ISpinnerTemplate
	{
		public uint Interval => 80;
		public string[] Frames => new string[] { "[    ]", "[=   ]", "[==  ]", "[=== ]", "[ ===]", "[  ==]", "[   =]", "[    ]", "[   =]", "[  ==]", "[ ===]", "[====]", "[=== ]", "[==  ]", "[=   ]" };
	}

	public class SpinnerTemplateBouncingBall : ISpinnerTemplate
	{
		public uint Interval => 80;
		public string[] Frames => new string[] { 
		    "( ●    )",
			"(  ●   )",
			"(   ●  )",
			"(    ● )",
			"(     ●)",
			"(    ● )",
			"(   ●  )",
			"(  ●   )",
			"( ●    )",
			"(●     )" 
		};
	}

	public class SpinnerTemplateDotsScrolling : ISpinnerTemplate
	{
		public uint Interval => 200;
		public string[] Frames => new string[] { ".  ", ".. ", "...", " ..", "  .", "   " };
	}

	public class SpinnerTemplateCircleQuarters : ISpinnerTemplate
	{
		public uint Interval => 120;
		public string[] Frames => new string[] { "◴", "◷", "◶", "◵" };
	}

	public class SpinnerTemplateToggle : ISpinnerTemplate
	{
		public uint Interval => 120;
		public string[] Frames => new string[] {"☗","☖"};
	}

	public class SpinnerTemplateAesthetic : ISpinnerTemplate
	{
		public uint Interval => 80;
		public string[] Frames => new string[] {
			"▰▱▱▱▱▱▱",
			"▰▰▱▱▱▱▱",
			"▰▰▰▱▱▱▱",
			"▰▰▰▰▱▱▱",
			"▰▰▰▰▰▱▱",
			"▰▰▰▰▰▰▱",
			"▰▰▰▰▰▰▰",
			"▰▱▱▱▱▱▱"
		};
	}

	public class SpinnerTemplateAestheticScrolling : ISpinnerTemplate
	{
		public uint Interval => 80;
		public string[] Frames => new string[] {
			"▰▱▱▱▱▱▱",
			"▰▰▱▱▱▱▱",
			"▰▰▰▱▱▱▱",
			"▱▰▰▰▱▱▱",
			"▱▱▰▰▰▱▱",
			"▱▱▱▰▰▰▱",
			"▱▱▱▱▰▰▰",
			"▱▱▱▱▱▰▰",
			"▱▱▱▱▱▱▰",
			"▱▱▱▱▱▱▱",
		};
	}

	public class TextSpinner
	{
		ISpinnerTemplate template;
		uint index = 0;

		public uint RecommendedInterval => template.Interval;

		public TextSpinner(ISpinnerTemplate template)
		{
			this.template = template;
		}

		public string GetFrame()
		{
			var frame = template.Frames[index];
			index++;
			if(index >= template.Frames.Length)
				index = 0;
			return frame;
		}
	}
}
