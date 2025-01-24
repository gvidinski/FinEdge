using FinEdgeAnalytics.DTOs;
using FinEdgeAnalytics.Transformers;

namespace Tests
{
	public class CsvTransformerTests
	{
		[Fact]
		public void ConvertData_ShouldConvertCorrectly()
		{
			// Arrange
			var transactions = new List<CsvTransactionDto>
				{
					new (Guid.CreateVersion7(), "2025-01-23T10:15:30+02:00", 10.5m, "Test123456789012345" ),
					new (Guid.CreateVersion7(), "2025-01-20T23:47:09+02:00", 20m, "AnotherTest" )
				};

			const int descriptionLength = 10;

			// Act
			var result = CsvTransformer.ConvertData(transactions, descriptionLength);

			// Assert
			Assert.Equal(2, result.Count);
			Assert.NotEqual(result[0].TransactionId, result[1].TransactionId);

			Assert.Equal(DateTime.Parse("2025-01-23T10:15:30+02:00").ToUniversalTime(), result[0].Date);
			Assert.Equal(10.5m, result[0].Amount);
			Assert.True(result[0].Description.Length <= descriptionLength);

			//Assert.Equal(2, result[1].TransactionId);
			Assert.Equal(DateTime.Parse("2025-01-20T23:47:09+02:00").ToUniversalTime(), result[1].Date);
			Assert.Equal(20m, result[1].Amount);
			Assert.True(result[1].Description.Length <= descriptionLength);
		}
	}
}
