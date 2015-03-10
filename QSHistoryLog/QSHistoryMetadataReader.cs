using System;
using System.Collections.Generic;
using System.Reflection;
using SIT.Components.ObjectComparer;
using QSProjectsLib;

namespace QSHistoryLog 
{
    public class QSHistoryMetadataReader : MetadataReader {

        public QSHistoryMetadataReader( IContext context ):base(context) {}

        public override ClassDescription GetClassDescription(object value) {
            throw new NotImplementedException();
        }

        public override MemberDescription GetPropertyDescription(object value, MemberInfo pi) {
            throw new NotImplementedException();
        }

        public override ItemDescription GetDescription(object item, MemberInfo pi) {
            throw new NotImplementedException();
        }

        public override List<MemberInfo> GetMembers(object value) {
            var retval = new List<MemberInfo>();
            if (value == null)
                return retval;

            retval.AddRange(value.GetType().GetProperties(_context.Configuration.GetMemberBindingFlags));
            retval.AddRange(value.GetType().GetFields(_context.Configuration.GetMemberBindingFlags));
            return retval;

        }

        public override void UpdateSnapshotData(SnapshotData data, object value, MemberInfo mi) {
            if (data == null)
                throw new ArgumentNullException("data");

            CompareAttribute ca = null;
            if (mi != null) {
                if (mi is PropertyInfo)
                    data.TypeName = (mi as PropertyInfo).PropertyType.FullName;
                else
                    data.TypeName = (mi as FieldInfo).FieldType.FullName;

                ca = Attribute.GetCustomAttribute(mi, typeof(ComparePropertyAttribute)) as CompareAttribute;
                if (ca != null)
                    data.Name = string.IsNullOrEmpty(ca.DisplayName) ? mi.Name : ca.DisplayName;
                else
                    data.Name = mi.Name;

                if (ca is CompareClassAttribute)
                    data.IdPropertyName = (ca as CompareClassAttribute).IdPropertyName;
            }
            if (value == null) {
                return;
            }

			if(value is KeyValuePair<string, object>)
			{
				//FIXME быстрый фикс сохранения коллекций.
				data.Name = ((KeyValuePair<string, object>)value).Key;
				data.Value = ((KeyValuePair<string, object>)value).Value;
				data.TypeName = value.GetType().FullName;
				return;
			}

            if (string.IsNullOrEmpty(data.TypeName))
                data.TypeName = value.GetType().FullName;

            if (string.IsNullOrEmpty(data.Name)) {
                
                ca = GetCompareMetadata(_context, value);
                if (ca != null) {
                    data.Name = string.IsNullOrEmpty(ca.DisplayName) ? value.GetType().Name : ca.DisplayName;
                    if (ca is CompareClassAttribute)
                        data.IdPropertyName = (ca as CompareClassAttribute).IdPropertyName;

                }
            }
            if (string.IsNullOrEmpty(data.IdPropertyName)) {
                ca = GetCompareMetadata(_context, value);
                if (ca != null)
                    data.IdPropertyName = (ca as CompareClassAttribute).IdPropertyName;

            }
            if (string.IsNullOrEmpty(data.Name))
                data.Name = value.GetType().Name;

            data.Value = value;

			if(value is IFileTrace)
			{
				var file = value as IFileTrace;
				data.Value = file.IsChanged ? "*": "" 
					+ StringWorks.BytesToIECUnitsString ((ulong)file.Size);
			}

			//Для классов бизнес модели
			if(value.GetType ().IsClass)
			{
				if (String.IsNullOrEmpty (data.IdPropertyName)) {
					var prop = value.GetType ().GetProperty ("Id");
					if (prop != null)
						data.IdPropertyName = "Id";
				}
				if(mi != null)
				{
					var prop = value.GetType ().GetProperty ("Title");
					if (prop != null) {
						data.Value = String.Format ("[{0}]", prop.GetValue (value, null));
						return;
					}

					prop = value.GetType ().GetProperty ("Name");
					if (prop != null) {
						data.Value = String.Format ("[{0}]", prop.GetValue (value, null));
						return;
					}

				}
			}

            if (value.GetType().IsValueType | value is string)
                return;

            data.Value = null;       
     
        }

		internal static CompareAttribute GetCompareMetadata( IContext context, object value ) {
			var configuration = context.Configuration;
			if( configuration.MetadataRetrievalOptions== MetadataRetrievalOptions.ReflectCompareAttributes ) {
				CompareAttribute ca = GetAttribute(
					value,
					typeof( CompareAttribute )
				) as CompareAttribute;
				return ca;

			}
			if( configuration.MetadataRetrievalOptions== MetadataRetrievalOptions.ReflectDescriptions ) {
				if (value is MemberInfo) {

				} else {

				}

				throw new NotImplementedException();

			}
			if( configuration.MetadataRetrievalOptions== MetadataRetrievalOptions.EmitPropertyDescription ) {
				throw new NotImplementedException();

			}
			throw new NotImplementedException();

		}

		internal static object GetAttribute( object value, System.Type attribute ) {
			object[] customAttributes;
			object currentAttribute;

			// Check if the given value is of type object or if it is already a PropertyInfo
			if( value is PropertyInfo )
				customAttributes = ( value as PropertyInfo ).GetCustomAttributes( attribute, true );
			else
				customAttributes = value.GetType().GetCustomAttributes( attribute, true );

			// Iterate through the found attributes
			for( int i = 0; i < customAttributes.Length; i++ ) {
				currentAttribute = customAttributes[ i ];

				// Check if the attribute its self is the correct type or if it is derived
				if( currentAttribute.GetType() == attribute || currentAttribute.GetType().BaseType == attribute )
					return currentAttribute;

			}
			return null;

		}

    }
}
