using System;
namespace QS.Utilities.Text
{
	public interface ISpinnerTemplate {
		uint Interval { get; }
		string[] Frames { get; }
	}

	public class SpinnerTemplateClock : ISpinnerTemplate
	{
		public uint Interval => 100;
		public string[] Frames => new string[] {"🕛", "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙","🕚"};
	}

	public class SpinnerTemplateEarth : ISpinnerTemplate
	{
		public uint Interval => 180;
		public string[] Frames => new string[] { "🌍", "🌎","🌏" };
	}

	public class SpinnerTemplateMoon : ISpinnerTemplate
	{
		public uint Interval => 80;
		public string[] Frames => new string[] { "🌑", "🌒", "🌓", "🌔", "🌕", "🌖", "🌗", "🌘" };
	}

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
