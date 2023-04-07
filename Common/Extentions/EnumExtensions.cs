using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace BookingCare.Common.Extentions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            try
            {
                if (enumValue == null)
                {
                    return string.Empty;
                }
                var configName = enumValue.GetType()
                    .GetMember(enumValue.ToString())
                    .First()?
                    .GetCustomAttribute<DisplayAttribute>()?
                    .GetName();
                if (string.IsNullOrEmpty(configName))
                {
                    return enumValue.ToString();
                }
                return configName;
            }
            catch
            {
                return enumValue.ToString();
            }

        }

        // case gồm 2 status đổ lên
        public static string GetDisplayNames(this Enum enumValue)
        {
            var enumType = enumValue.GetType();
            var names = new List<string>();
            foreach (var e in Enum.GetValues(enumType))
            {
                var flag = (Enum)e;
                if (enumValue.HasFlag(flag))
                {
                    names.Add(GetDisplayName(flag));
                }
            }
            if (names.Count <= 0) throw new ArgumentException();
            if (names.Count == 1) return names.First();
            return string.Join(", ", names);
        }

        public static int GetOrder(this Enum enumValue)
        {
            var orderConfig = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()?
                .GetCustomAttribute<DisplayAttribute>()?
                .GetOrder().GetValueOrDefault();
            return orderConfig.GetValueOrDefault(0);
        }

        public static string ToTextByEnum<T>(this long value)
        {
            return Enum
                .GetValues(typeof(T))
                .Cast<T>()
                .FirstOrDefault(v => (int)(object)v == value)
                .ToString();
        }

        public static string ToTextByEnum<T>(this int value)
        {
            return Enum
                .GetValues(typeof(T))
                .Cast<T>()
                .FirstOrDefault(v => (int)(object)v == value)
                .ToString();
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            var result = enumerable == null;

            if (!result)
            {
                result = !enumerable.Any() ||
                         (typeof(T) == typeof(string)
                             ? enumerable.Select(e => e == null || string.IsNullOrWhiteSpace(e.ToString())).All(b => b)
                             : enumerable.All(e => e == null));
            }

            return result;
        }

        public static string GetShortName(this Enum enumValue)
        {
            try
            {
                if (enumValue == null)
                {
                    return string.Empty;
                }
                var configName = enumValue.GetType()
                    .GetMember(enumValue.ToString())
                    .First()?
                    .GetCustomAttribute<DisplayAttribute>()?
                    .GetShortName();
                if (string.IsNullOrEmpty(configName))
                {
                    return enumValue.ToString();
                }
                return configName;
            }
            catch
            {
                return enumValue.ToString();
            }

        }
    }
}
