using System;
using System.Globalization;
using System.Reflection;

namespace Photizer.Domain.Helpers
{
    public class PropertyTypeConverter
    {
        public static object GetConvertedValueForProperty(PropertyInfo info, string input)
        {
            var typeCode = Type.GetTypeCode(info.PropertyType);

            switch (typeCode)
            {
                case TypeCode.Int32:
                    return Convert.ToInt32(input);

                case TypeCode.Double:
                    return Convert.ToDouble(input, CultureInfo.InvariantCulture);

                case TypeCode.DateTime:
                    return Convert.ToDateTime(input);

                case TypeCode.String:
                    return input;

                default:
                    return input;
            }
        }
    }
}