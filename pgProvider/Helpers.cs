using System;
using System.Security.Cryptography;
using System.Text;

namespace pgProvider
{
	public static class Helpers
	{
		public static string ToBase64(this byte[] target)
		{
			return Convert.ToBase64String(target);
		}

		public static string ToCharacterString(this byte[] target)
		{
			return Encoding.UTF8.GetString(target).TrimEnd(new char[] { '\0' });
		}

		public static byte[] ToByteArray(this string target)
		{
			return Encoding.UTF8.GetBytes(target);
		}

		public static int ToInt(this string target, int defaultValue)
		{
			int toChange;
			if (int.TryParse(target, out toChange))
			{
				return toChange;
			}
			return defaultValue;
		}

		public static int ToInt(this string target)
		{
			return target.ToInt(0);
		}
	}
}
