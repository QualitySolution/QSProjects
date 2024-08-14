using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Utilities.Extensions {
	public static class IListExtensions {
		/// <summary>
		/// Сортирует список методом слияния
		/// </summary>
		/// <param name="source">Сортируемый список</param>
		/// <param name="comparison">Функция сравнения двух значений</param>
		public static void MergeSort<T>(this IList<T> source, Comparison<T> comparison) {
			MergeSort(source, 0, source.Count - 1, comparison);
		}
		
		private static void MergeSort<T>(IList<T> source, int left, int right, Comparison<T> comparison) {
			if(left >= right) {
				return;
			}

			var middle = left + (right - left) / 2;

			MergeSort(source, left, middle, comparison);
			MergeSort(source, middle + 1, right, comparison);

			Merge(source, left, middle, right, comparison);
		}
		
		private static void Merge<T>(IList<T> source, int left, int middle, int right, Comparison<T> comparison) {
			// Создаем временные массивы
			var sourceArray = source.ToArray();
			var leftArrayLength = middle - left + 1;
			var rightArrayLength = right - middle;

			var leftArray = new T[leftArrayLength];
			var rightArray = new T[rightArrayLength];

			// Копируем данные во временные массивы
			Array.Copy(sourceArray, left, leftArray, 0, leftArrayLength);
			Array.Copy(sourceArray, middle + 1, rightArray, 0, rightArrayLength);

			// Сливаем временные массивы обратно в основной массив
			var i = 0; // Начальный индекс первого подмассива
			var j = 0; // Начальный индекс второго подмассива
			var k = left; // Начальный индекс сливаемого массива

			while(i < leftArrayLength && j < rightArrayLength) {
				var compare = comparison(leftArray[i], rightArray[j]);
				
				if(compare <= 0)
				{
					source[k] = leftArray[i];
					i++;
				}
				else
				{
					source[k] = rightArray[j];
					j++;
				}
				k++;
			}

			// Копируем оставшиеся элементы, если они есть
			while(i < leftArrayLength)
			{
				source[k] = leftArray[i];
				i++;
				k++;
			}

			while(j < rightArrayLength)
			{
				source[k] = rightArray[j];
				j++;
				k++;
			}
		}
	}
}
