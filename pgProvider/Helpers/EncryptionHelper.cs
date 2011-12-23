using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace pgProvider
{
	public static class EncryptionHelper
	{
		private const string SaltChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-";
		private const string SpecialChars = @"()*&^%$#@!`~/?;:.>,<\|=+-_";
		private static int SaltCharLength = SaltChars.Length;
		private static int SpecialCharLength = SpecialChars.Length;
		private static RNGCryptoServiceProvider crypt = new RNGCryptoServiceProvider();

		public static string GenerateSalt(int minChars, int maxChars)
		{
			if (minChars <= 0) throw new ArgumentOutOfRangeException("minChars");
			if (maxChars < minChars) throw new ArgumentOutOfRangeException("maxChars");

			var saltLength = GenerateTrueRandomNumber(minChars, maxChars);
			var salt = string.Empty;
			for (int i = 0; i < saltLength; i++)
			{
				salt += SaltChars.Substring(GenerateTrueRandomNumber(0, SaltCharLength), 1);
			}
			return salt;
		}

		public static int GenerateTrueRandomNumber(int min, int max)
		{
			if (min == max) return min;
			int range = max - min;
			byte[] buffer = BitConverter.GetBytes(range);
			crypt.GetBytes(buffer);
			var rndValue = (decimal)Math.Abs(BitConverter.ToInt32(buffer, 0));
			var integral = (decimal)int.MaxValue / (decimal)range;
			return ((int)(rndValue / integral) + min);
		}

		public static byte[] GenerateHash(string toHash, string salt)
		{
			if (string.IsNullOrEmpty(toHash)) throw new ArgumentException("toHash");
			if (string.IsNullOrEmpty(salt)) throw new ArgumentException("salt");

			var bytes = Encoding.UTF8.GetBytes(toHash + salt);
			using (var algorithm = new SHA384Managed())
			{
				return algorithm.ComputeHash(bytes);
			}
		}

		public static byte[] EncryptString(byte[] toEncrypt, byte[] encryptionKey)
		{
			if (encryptionKey == null || encryptionKey.Length == 0) throw new ArgumentException("encryptionKey");
			using (var provider = new AesCryptoServiceProvider())
			{
				provider.Key = encryptionKey;
				provider.Mode = CipherMode.CBC;
				provider.Padding = PaddingMode.PKCS7;
				using (var encryptor = provider.CreateEncryptor(provider.Key, provider.IV))
				{
					using (var ms = new MemoryStream())
					{
						using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
						{
							cs.Write(toEncrypt, 0, toEncrypt.Length);
							cs.FlushFinalBlock();
							var retVal = new byte[16 + ms.Length];
							provider.IV.CopyTo(retVal, 0);
							ms.ToArray().CopyTo(retVal, 16);
							return retVal;
						}
					}
				}
			}
		}

		public static byte[] EncryptString(string toEncrypt, byte[] encryptionKey)
		{
			if (string.IsNullOrEmpty(toEncrypt)) throw new ArgumentException("toEncrypt");
			return EncryptString(Encoding.UTF8.GetBytes(toEncrypt), encryptionKey);
		}

		public static byte[] DecryptString(byte[] encryptedString, byte[] encryptionKey)
		{
			if (encryptedString == null || encryptedString.Length == 0) throw new ArgumentException("encryptedString");
			if (encryptionKey == null || encryptionKey.Length == 0) throw new ArgumentException("encryptionKey");
			using (var provider = new AesCryptoServiceProvider())
			{
				provider.Key = encryptionKey;
				provider.Mode = CipherMode.CBC;
				provider.Padding = PaddingMode.PKCS7;
				provider.IV = encryptedString.Take(16).ToArray();
				using (var ms = new MemoryStream(encryptedString, 16, encryptedString.Length - 16))
				{
					using (var decryptor = provider.CreateDecryptor(provider.Key, provider.IV))
					{
						using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
						{
							byte[] decrypted = new byte[encryptedString.Length];
							var byteCount = cs.Read(decrypted, 0, encryptedString.Length);
							//return Encoding.UTF8.GetString(decrypted, 0, byteCount);
							return decrypted;
						}
					}
				}
			}
		}

		public static string ToBase64(this byte[] target)
		{
			return Convert.ToBase64String(target);
		}

		public static string ToCharacterString(this byte[] target)
		{
			return Encoding.UTF8.GetString(target).TrimEnd(new char[]{'\0'});
		}

		public static byte[] GenerateAESKey()
		{
			using (var provider = new AesManaged())
			{
				provider.KeySize = 256;
				provider.GenerateKey();
				return provider.Key;
			}
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
