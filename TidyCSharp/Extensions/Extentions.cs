using System.Linq;

namespace System
{
    public static class Extensions
    {
        public static bool HasValue(this string text) => !string.IsNullOrEmpty(text);

        public static string Or(this string text, string ifEmpty) => text.HasValue() ? text : ifEmpty;

        public static string TrimStart(this string text, string textToTrim, bool ignoreCase = false)
        {
            if (text != null && text.StartsWith(textToTrim, ignoreCase, Globalization.CultureInfo.InvariantCulture))
            {
                return text.Substring(textToTrim.Length);
            }
            else
            {
                return text;
            }
        }

        public static string TrimEnd(this string text, string textToTrim)
        {
            if (text != null && text.EndsWith(textToTrim))
            {
                return text.TrimEnd(textToTrim.Length);
            }
            else
            {
                return text;
            }
        }

        public static string TrimEnd(this string text, int numberOfCharacters)
        {
            if (numberOfCharacters < 0)
                throw new ArgumentException("numberOfCharacters must be greater than 0.");

            if (numberOfCharacters == 0) return text;

            if (text.IsEmpty() || text.Length <= numberOfCharacters)
                return string.Empty;

            return text.Substring(0, text.Length - numberOfCharacters);
        }

        public static bool IsEmpty(this string text) => string.IsNullOrEmpty(text);

        public static string Remove(this string text, params string[] substringsToExclude)
        {
            if (text.IsEmpty()) return text;

            var result = text;

            if (substringsToExclude != null)
                foreach (var sub in substringsToExclude)
                    result = result.Replace(sub, "");

            return result;
        }

        public static bool EndsWithAny(this string input, params string[] listOfEndings)
        {
            if (listOfEndings != null)
                foreach (var option in listOfEndings)
                    if (input.EndsWith(option)) return true;

            return false;
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        public static K Get<T, K>(this T item, Func<T, K> selector)
        {
            if (object.ReferenceEquals(item, null))
                return default(K);
            return (selector != null) ? selector(item) : default(K);
            //else
            //{
            //    try
            //    {
            //        return selector(item);
            //    }
            //    catch (NullReferenceException)
            //    {
            //        return default(K);
            //    }
            //}
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        public static Guid? Get<T>(this T item, Func<T, Guid> selector) where T : class
        {
            if (item == null) return null;
            return (selector != null) ? selector(item) : null as Guid?;
            //try
            //{
            //    return selector(item);
            //}
            //catch (NullReferenceException)
            //{
            //    return null;
            //}
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        public static int? Get<T>(this T item, Func<T, int> selector) where T : class
        {
            if (item == null) return null;
            return (selector != null) ? selector(item) : null as int?;
            //try
            //{
            //    return selector(item);
            //}
            //catch (NullReferenceException)
            //{
            //    return null;
            //}
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        public static double? Get<T>(this T item, Func<T, double> selector) where T : class
        {
            if (item == null) return null;
            return (selector != null) ? selector(item) : null as double?;
            //try
            //{
            //    return selector(item);
            //}
            //catch (NullReferenceException)
            //{
            //    return null;
            //}
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        public static decimal? Get<T>(this T item, Func<T, decimal> selector) where T : class
        {
            if (item == null) return null;
            return (selector != null) ? selector(item) : null as decimal?;
            //try
            //{
            //    return selector(item);
            //}
            //catch (NullReferenceException)
            //{
            //    return null;
            //}
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        public static bool? Get<T>(this T item, Func<T, bool> selector) where T : class
        {
            if (item == null) return null;
            return (selector != null) ? selector(item) : null as bool?;
            //try
            //{
            //    return selector(item);
            //}
            //catch (NullReferenceException)
            //{
            //    return null;
            //}
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        public static string Get(this DateTime? item, Func<DateTime?, string> selector)
        {
            if (item == null) return null;
            return (selector != null) ? selector(item) : null;
            //try
            //{
            //    return selector(item);
            //}
            //catch (NullReferenceException)
            //{
            //    return null;
            //}
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        public static byte? Get<T>(this T item, Func<T, byte> selector) where T : class
        {
            if (item == null) return null;
            return (selector != null) ? selector(item) : null as byte?;
            //    try
            //{
            //    return selector(item);
            //}
            //catch (NullReferenceException)
            //{
            //    return null;
            //}
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        public static DateTime? Get<T>(this T item, Func<T, DateTime> selector) where T : class
        {
            if (item == null) return null;

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        public static DateTime? Get<T>(this T item, Func<T, DateTime?> selector) where T : class
        {
            if (item == null) return null;

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        public static T Get<T>(this DateTime? item, Func<DateTime?, T> selector) where T : struct
        {
            if (item == null) return default(T);

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return default(T);
            }
        }

        public static string Replace(this string str, string oldValue, string newValue, bool caseSensitive)
        {
            if (caseSensitive)
                return str.Replace(oldValue, newValue);

            var prevPos = 0;
            var retval = str;
            var pos = retval.IndexOf(oldValue, StringComparison.InvariantCultureIgnoreCase);

            while (pos > -1)
            {
                retval = str.Remove(pos, oldValue.Length);
                retval = retval.Insert(pos, newValue);
                prevPos = pos + newValue.Length;
                pos = retval.IndexOf(oldValue, prevPos, StringComparison.InvariantCultureIgnoreCase);
            }

            return retval;
        }

        const string SingleQuote = "'", DoubleQuote = "\"";

        public static string StripQuotation(this string str)
        {
            if (str.StartsWith(SingleQuote))
                return str
                    .TrimStart(SingleQuote)
                    .TrimEnd(SingleQuote);

            if (str.StartsWith(DoubleQuote))
                return str
                    .TrimStart(DoubleQuote)
                    .TrimEnd(DoubleQuote);

            return str;
        }
    }
}