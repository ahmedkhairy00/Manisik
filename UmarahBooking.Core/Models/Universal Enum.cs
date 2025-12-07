// UniversalEnumHelper.cs
using System;
using System.Collections.Generic;

namespace UmarahBooking.Core.Helpers
{
    /// <summary>
    /// Helper to parse enums universally from string inputs,
    /// and retrieve enum names/values dynamically.
    /// Generic and usable in any service (e.g., ChatBotService).
    /// </summary>
    public static class UniversalEnumHelper
    {
        #region Parsing

        /// <summary>
        /// Try parse a string into an enum value (non-nullable).
        /// </summary>
        public static bool TryParse<TEnum>(string value, out TEnum result, bool ignoreCase = true)
            where TEnum : struct, Enum
        {
            result = default;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var type = typeof(TEnum);
            var names = Enum.GetNames(type);

            // direct parse
            if (Enum.TryParse<TEnum>(value, ignoreCase, out result)) return true;

            // try alternative matches (basic normalization)
            var valLower = value.Trim().ToLowerInvariant();
            foreach (var name in names)
            {
                if (name.ToLowerInvariant() == valLower)
                {
                    result = (TEnum)Enum.Parse(type, name);
                    return true;
                }
            }

            // try numeric parsing
            if (int.TryParse(value, out var intVal))
            {
                if (Enum.IsDefined(type, intVal))
                {
                    result = (TEnum)Enum.ToObject(type, intVal);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Try parse a string into a nullable enum value.
        /// </summary>
        public static bool TryParseNullable<TEnum>(string value, out TEnum? result, bool ignoreCase = true)
            where TEnum : struct, Enum
        {
            result = null;
            if (string.IsNullOrWhiteSpace(value)) return false;

            if (TryParse<TEnum>(value, out var res, ignoreCase))
            {
                result = res;
                return true;
            }

            return false;
        }

        #endregion

        #region Enum info

        /// <summary>
        /// Get all names of an enum as strings.
        /// </summary>
        public static string[] GetNames<TEnum>() where TEnum : struct, Enum
        {
            return Enum.GetNames(typeof(TEnum));
        }

        /// <summary>
        /// Get all values of an enum as their typed values.
        /// </summary>
        public static TEnum[] GetValues<TEnum>() where TEnum : struct, Enum
        {
            return (TEnum[])Enum.GetValues(typeof(TEnum));
        }

        /// <summary>
        /// Get dictionary of enum names and their underlying integer values.
        /// </summary>
        public static Dictionary<string, int> GetNamesAndValues<TEnum>() where TEnum : struct, Enum
        {
            var type = typeof(TEnum);
            var dict = new Dictionary<string, int>();
            foreach (var val in Enum.GetValues(type))
            {
                dict[val.ToString()] = (int)val;
            }
            return dict;
        }

        #endregion
    }
}
