using System;

namespace myClippit.ApplicationCore.Shared.Hashers
{
	public interface IPasswordHasher
	{		
		string HashPassword(string password);
		bool VerifyHashedPassword(string hashedPassword, string providedPassword);
	}
}
