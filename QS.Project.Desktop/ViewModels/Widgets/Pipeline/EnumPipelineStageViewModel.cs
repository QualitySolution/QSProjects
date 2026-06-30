using Gamma.Utilities;
using System;

namespace QS.ViewModels.Widgets.Pipeline {
	public class EnumPipelineStageViewModel : PipelineStageViewModel {
		public EnumPipelineStageViewModel(Enum content) {
			Content = content ?? throw new ArgumentNullException(nameof(content));

			Id = content.ToString();
			Name = content.GetEnumTitle();
		}

		public Enum Content { get; }
	}
}
