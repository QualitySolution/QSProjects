using System;
namespace QS.DomainModel.UoW
{
	public class UnitOfWorkTitle
	{
        //НЕЛЬЗЯ иметь ссылку на UoW так как этот класс сохраняется глобально и будет держать UoW от удаления гарбич коллектором.
        public string CallerMemberName { get; private set; }
        public string CallerFilePath { get; private set; }
		public int CallerLineNumber { get; private set; }
        public string UserActionTitle { get; set; }

        public DateTime CreateTime { get; private set; }

        public UnitOfWorkTitle(string userActionTitle, string callerMemberName, string callerFilePath, int callerLineNumber)
		{
            UserActionTitle = userActionTitle;
            CallerMemberName = callerMemberName;
            CallerFilePath = callerFilePath;
            CallerLineNumber = callerLineNumber;
            CreateTime = DateTime.Now;
		}
	}
}
