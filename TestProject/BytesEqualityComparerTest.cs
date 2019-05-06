using System;
using Xunit;
using myClippit.ApplicationCore.Shared.Comparers;

namespace TestProject.ApplicationCore.Shared
{
	public class BytesEqualityComparerTest
	{
		private BytesEqualityComparer comparer;

		public BytesEqualityComparerTest()
		{
			comparer = new BytesEqualityComparer();
		}

		[Fact]
		public void References_Null()
		{
			// Arrange
			byte[] x = null;
			byte[] y = null;

			// Act & assert - success case
			Assert.True(comparer.Equals(x, y));
		}

		[Fact]
		public void References_Same()
		{
			// Arrange
			var x = new byte[] { 1, 2, 3 };
			var y = x;

			// Act & assert - success case
			Assert.True(comparer.Equals(x, y));

			// Arrange
			x = y;

			// Act & assert - success case
			Assert.True(comparer.Equals(x, y));
		}

		[Fact]
		public void Values_Same()
		{
			// Arrange
			var x = new byte[] { 1, 2, 3 };
			var y = new byte[] { 1, 2, 3 };

			// Act & assert - success case
			Assert.True(comparer.Equals(x, y));
		}

		[Fact]
		public void Values_Different()
		{
			// Arrange
			var x = new byte[] { 1, 2, 3 };
			var y = new byte[] { 1, 2 };

			// Act & assert - failure case
			Assert.False(comparer.Equals(x, y));
		}

	}
}
