//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Globalization;
//using System.Linq;
//using System.Net.Mail;
//using System.Reflection;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace System
//{
//	/// <summary>
//	/// String extension methods.
//	/// </summary>
//	public static partial class StringExtensions
//	{
//		/// <summary>
//		/// Null or Empty
//		/// </summary>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		public static bool IsNullOrEmpty(this string value)
//		{
//			return string.IsNullOrEmpty(value);
//		}

//		/// <summary>
//		/// Null or Empty or "0"
//		/// </summary>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		public static bool IsNullOrEmptyOrZero(this string value)
//		{
//			return string.IsNullOrEmpty(value) || value.Trim() == "0";
//		}

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		public static bool IsEmailAddress(this string value)
//		{
//			try
//			{
//				MailAddress mailAddress = new(value);
//				return true;
//			}
//			catch (Exception)
//			{
//				return false;
//			}
//		}

//		/// <summary>
//		/// Not Null or Empty
//		/// </summary>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		public static bool IsNotNullOrEmpty(this string value)
//		{
//			return !string.IsNullOrEmpty(value);
//		}

//		/// <summary>
//		/// Slugify string.
//		/// </summary>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		public static string Slugify(this string value)
//		{
//			var s = value.ToLower();
//			s = Regex.Replace(s, @"[^a-z0-9\s-]", "");                      // remove invalid characters
//			s = Regex.Replace(s, @"\s+", " ").Trim();                       // single space
//			s = s.Substring(0, s.Length <= 45 ? s.Length : 45).Trim();      // cut and trim
//			s = Regex.Replace(s, @"\s", "-");                               // insert hyphens
//			return s.ToLower();
//		}

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		public static Tuple<bool, DateTimeOffset> IsDateTime(this string value)
//		{
//			var isDateTime = DateTimeOffset.TryParse(value, out DateTimeOffset date);
//			return new Tuple<bool, DateTimeOffset>(isDateTime, date);
//		}

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		public static decimal ToDecimal(this string value)
//		{
//			return value.IsNullOrEmpty() ? 0m : Convert.ToDecimal(value);
//		}

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		public static DateTime ToDateTime(this string value)
//		{
//			DateTime result = DateTime.MaxValue;
//			CultureInfo cultureinfo = new("en-gb");
//			DateTime.TryParse(value, cultureinfo, DateTimeStyles.None, out result);
//			return result;
//		}

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <param name="type"></param>
//		/// <returns></returns>
//		public static bool IsNumericType(this Type type)
//		{
//			return NumericTypes.Contains(type) ||
//						 NumericTypes.Contains(Nullable.GetUnderlyingType(type));
//		}

//		/// <summary>
//		/// 
//		/// </summary>
//		private static readonly HashSet<Type> NumericTypes = new()
//				{
//						typeof(int),
//						typeof(uint),
//						typeof(double),
//						typeof(decimal)
//				};

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		public static string RemoveIllegalChars(this string value)
//		{
//			return value.Replace(",", "");
//		}

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <param name="type"></param>
//		/// <returns></returns>
//		public static bool IsDateTime(this Type type)
//		{
//			return type.Name == "DateTime";
//		}

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <typeparam name="T"></typeparam>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		public static T ToEnum<T>(this string value)
//		{
//			return (T)Enum.Parse(typeof(T), value);
//		}

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <param name="value"></param>
//		/// <returns></returns>
//		public static string ToDisplayName(this Enum value)
//		{
//			DisplayAttribute displayAttribute = value
//					.GetType()
//					.GetMember(value.ToString())
//					.First()
//					.GetCustomAttribute<DisplayAttribute>();

//			string displayName = displayAttribute?.GetName();

//			return displayName ?? value.ToString();
//		}

//		public static string ToName(this Enum value)
//		{
//			return Enum.GetName(value.GetType(), value);
//		}

//		public static string Right(this string value, int numberOfCharacters)
//		{
//			return value.Substring(value.Length - numberOfCharacters, numberOfCharacters);
//		}

//		public static string Left(this string value, int numberOfCharacters)
//		{
//			return value.Substring(0, numberOfCharacters);
//		}
//	}
//}
