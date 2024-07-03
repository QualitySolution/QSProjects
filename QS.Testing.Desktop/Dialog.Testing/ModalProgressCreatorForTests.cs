using QS.Navigation;

namespace QS.Dialog.Testing {
	public class ModalProgressCreatorForTests : ModalProgressCreator {
		public ModalProgressCreatorForTests() : base()
		{
		}

		public override bool IsStarted => isStarted;
		private bool isStarted;

		public override void Start(double maxValue = 1, double minValue = 0, string text = null, double startValue = 0) {
			isStarted = true;
		}

		public override void Add(double addValue = 1, string text = null) {
			
		}

		public override void Update(string curText) {
			
		}

		public override void Update(double curValue) {
		}
		
		public override void Close() {
			isStarted = false;
		}
	}
}
