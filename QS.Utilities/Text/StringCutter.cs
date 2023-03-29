using System;

namespace QS.Utilities.Text {

	public static class StringCutter {
		/// <summary>
		/// Обрезка с конца
		/// </summary>
		/// <param name="lenght"></param> Максимально ожидаемая длинна результата (от 4)
		/// <returns></returns>
		public static string CutEnd(string sourse, int lenght = 100) {
			if(sourse == null) 
				return null;
			if(lenght < 4)
				return "...";
			if(sourse.Length > lenght )
				return sourse.Substring(0, lenght - 3) + "...";
			return sourse;
		}
  
		/// <summary>
		/// Обрезка середины
		/// </summary>
		/// <param name="lenght"></param> Максимально ожидаемая длинна результата (от 10)
		/// <returns></returns>
		public static string CutMiddle(string sourse, int lenght = 100) {
			if(sourse == null) 
				return null;
			
			if(lenght < 9)
				return "...";

			if(sourse.Length > lenght) 
				return sourse.Substring(0, lenght / 2) + " ... " + sourse.Substring(sourse.Length - lenght / 2 - 5);
				
			return sourse;
		}
	}
}
