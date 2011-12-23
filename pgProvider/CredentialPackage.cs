using System;

namespace pgProvider
{
	public class CredentialPackage
	{
		public string PasswordSalt { get; set; }
		public byte[] PasswordHash { get; set; }
		public string AnswerSalt { get; set; }
		public byte[] AnswerHash { get; set; }
		public DateTime LockedOutAsOf { get; set; }
	}
}
