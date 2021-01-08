using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace MasterClassSagaPattern.Common
{
    public static class EnumExtensions
    {
        public static string ToFriendlyName<TEnum>(this TEnum enumValue)
            where TEnum : Enum
        {
            return Enum.GetName(typeof(TEnum), enumValue);
        }

        public static TEnum ToEnumValue<TEnum>(this string stringValue)
            where TEnum : Enum
        {
            return (TEnum)Enum.Parse(typeof(TEnum), stringValue);
        }

        public static string GetEnumMemberValue<T>(this T value)
            where T : struct, IConvertible
        {
            return typeof(T)
                .GetTypeInfo()
                .DeclaredMembers
                .FirstOrDefault(x => x.Name.Equals(value.ToString(), StringComparison.OrdinalIgnoreCase))
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;
        }

        public static T ToEnum<T>(this string str)
        {
            var enumType = typeof(T);
            foreach (var name in Enum.GetNames(enumType))
            {
                var enumMemberAttribute = (EnumMemberAttribute)enumType.GetField(name).GetCustomAttribute(typeof(EnumMemberAttribute), true);
                if (enumMemberAttribute.Value == str)
                {
                    return (T)Enum.Parse(enumType, name);
                }
            }

            throw new NotSupportedException();
        }
    }
}
