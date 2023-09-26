using System;

namespace QS.Deletion.Configuration {
	public class UpdateDependenceInfo {
		public UpdateDependenceInfo(
			Type objectClass,
			string refDependencePropertyName,
			string refUpdatablePropertyName,
			object value) {
			ObjectClass = objectClass;
			DependencePropertyName = refDependencePropertyName;
			UpdatablePropertyName = refUpdatablePropertyName;
			Value = value;
		}

		public Type ObjectClass { get; set; }
		public string DependencePropertyName { get; }
		public string UpdatablePropertyName { get; }
		public object Value { get; }

		public static UpdateDependenceInfo Create<TEntity>(string refDependencePropertyName, string refUpdatablePropertyName, object value) {
			return new UpdateDependenceInfo(typeof(TEntity), refDependencePropertyName, refUpdatablePropertyName, value);
		}
	}
}

