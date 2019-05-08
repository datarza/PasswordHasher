using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using Microsoft.Extensions.Options;

using myClippit.ApplicationCore.Shared.Comparers;

namespace myClippit.ApplicationCore.Shared.Hashers
{
	public class PasswordHasher : IPasswordHasher
	{
		private readonly PasswordHasherOptions options;

		private readonly IEqualityComparer<byte[]> comparer;

		/// <summary>
		/// Creates a new instance of <see cref="PasswordHasher"/>.
		/// </summary>
		/// <param name="optionsAccessor">The options for this instance.</param>
		/// <param name="bytesComparer">The comparer of byte[] for this instance.</param>
		public PasswordHasher(IOptions<PasswordHasherOptions> optionsAccessor = null, IEqualityComparer<byte[]> bytesComparer = null)
		{
			options = optionsAccessor?.Value ?? new PasswordHasherOptions();

			if (options.SaltSize < 8)
				throw new ArgumentOutOfRangeException(nameof(options.SaltSize));

			if (options.Iterations < 1)
				throw new ArgumentOutOfRangeException(nameof(options.Iterations));

			comparer = bytesComparer ?? new BytesEqualityComparer();
		}

		public string HashPassword(string password)
		{
			byte[] saltBuffer;
			byte[] hashBuffer;

			using (var keyDerivation = new Rfc2898DeriveBytes(password, options.SaltSize, options.Iterations, options.HashAlgorithmName))
			{
				saltBuffer = keyDerivation.Salt;
				hashBuffer = keyDerivation.GetBytes(options.HashSize);
			}

			byte[] result = new byte[options.HashSize + options.SaltSize];
			Buffer.BlockCopy(hashBuffer, 0, result, 0, options.HashSize);
			Buffer.BlockCopy(saltBuffer, 0, result, options.HashSize, options.SaltSize);
			return Convert.ToBase64String(result);
		}

		public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
		{
			byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);
			if (hashedPasswordBytes.Length != options.HashSize + options.SaltSize)
			{
				return false;
			}

			byte[] hashBytes = new byte[options.HashSize];
			Buffer.BlockCopy(hashedPasswordBytes, 0, hashBytes, 0, options.HashSize);
			byte[] saltBytes = new byte[options.SaltSize];
			Buffer.BlockCopy(hashedPasswordBytes, options.HashSize, saltBytes, 0, options.SaltSize);

			byte[] providedHashBytes;
			using (var keyDerivation = new Rfc2898DeriveBytes(providedPassword, saltBytes, options.Iterations, options.HashAlgorithmName))
			{
				providedHashBytes = keyDerivation.GetBytes(options.HashSize);
			}

			return comparer.Equals(hashBytes, providedHashBytes);
		}
		
	}
}
