using System.IO;
using System.Reflection;

namespace QS.Utilities.Extensions {
	public static class AssemblyExtension {
		/// <summary>
		/// Получает встроенный ресурс в виде массива байт
		/// </summary>
		/// <returns></returns>
		public static byte[] GetResourceByteArray(this Assembly assembly, string resourceName) {
			using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
				using (MemoryStream ms = new MemoryStream()) {
					stream.CopyTo(ms);
					return ms.ToArray();
				}
			}
		}
	}
}
