using System;

using Microsoft.Extensions.Options;

using myClippit.ApplicationCore.Shared.Hashers;

using Xunit;

namespace TestProject.ApplicationCore.Shared
{
	public class PasswordHasherTest
	{
		[Theory]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(7)]
		public void Ctor_InvalidSaltSize_Throws(int saltSize)
		{
			// Act & assert
			var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				new PasswordHasher(BuildOptions(saltSize: saltSize));
			});
			Assert.Equal("Specified argument was out of the range of valid values.\r\nParameter name: SaltSize", ex.Message);
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(0)]
		public void Ctor_InvalidIterations_Throws(int iterations)
		{
			// Act & assert
			var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				new PasswordHasher(BuildOptions(iterations: iterations));
			});
			Assert.Equal("Specified argument was out of the range of valid values.\r\nParameter name: Iterations", ex.Message);
		}

		[Theory]
		[InlineData(PasswordHasherAlgorithms.SHA1)]
		[InlineData(PasswordHasherAlgorithms.SHA256)]
		[InlineData(PasswordHasherAlgorithms.SHA384)]
		[InlineData(PasswordHasherAlgorithms.SHA512)]
		public void HashRoundTrip(PasswordHasherAlgorithms algorithm)
		{
			// Arrange
			var hasher = new PasswordHasher(BuildOptions(algorithm));

			// Act & assert - success case
			var hashedPassword = hasher.HashPassword("password 1");
			var successResult = hasher.VerifyHashedPassword(hashedPassword, "password 1");
			Assert.True(successResult);

			// Act & assert - failure case
			var failedResult = hasher.VerifyHashedPassword(hashedPassword, "password 2");
			Assert.False(failedResult);
		}
		
		[Theory]
		[InlineData(PasswordHasherAlgorithms.SHA1, 8, 40)]
		[InlineData(PasswordHasherAlgorithms.SHA1, 9, 40)]
		[InlineData(PasswordHasherAlgorithms.SHA1, 10, 40)]
		[InlineData(PasswordHasherAlgorithms.SHA1, 16, 48)]
		[InlineData(PasswordHasherAlgorithms.SHA256, 16, 64)]
		[InlineData(PasswordHasherAlgorithms.SHA384, 24, 96)]
		[InlineData(PasswordHasherAlgorithms.SHA512, 32, 128)]
		public void LengthRoundTrip(PasswordHasherAlgorithms algorithm, int saltSize, int expectedLength)
		{
			// Arrange
			var hasher = new PasswordHasher(BuildOptions(algorithm, saltSize));

			// Act & assert - success case
			var hashedPassword = hasher.HashPassword("password 1");
			Assert.Equal(expectedLength, hashedPassword.Length);
		}

		[Theory]
		[InlineData(PasswordHasherAlgorithms.SHA1, 20)]
		[InlineData(PasswordHasherAlgorithms.SHA256, 32)]
		[InlineData(PasswordHasherAlgorithms.SHA384, 48)]
		[InlineData(PasswordHasherAlgorithms.SHA512, 64)]
		public void OptionsRoundTrip(PasswordHasherAlgorithms algorithm, int expectedSize)
		{
			// Arrange
			var options = new PasswordHasherOptions(algorithm);

			// Act & assert - success case
			Assert.Equal(expectedSize, options.HashSize);

			// Arrange
			options.HashAlgorithm = PasswordHasherAlgorithms.SHA1;
			options.SaltSize = expectedSize;
			options.HashAlgorithm = algorithm;
			
			// Act & assert - failure case
			Assert.Equal(expectedSize, options.HashSize);
		}

		public static IOptions<PasswordHasherOptions> BuildOptions(int? saltSize = null, int? iterations = null)
		{
			var options = new PasswordHasherOptions();

			if (saltSize.HasValue)
				options.SaltSize = saltSize.Value;

			if (iterations.HasValue)
				options.Iterations = iterations.Value;

			return Options.Create(options);
		}

		public static IOptions<PasswordHasherOptions> BuildOptions(PasswordHasherAlgorithms algorithm, int? saltSize = null, int? iterations = null)
		{
			var options = new PasswordHasherOptions(algorithm, saltSize, iterations);
			return Options.Create(options);
		}

	}
}
