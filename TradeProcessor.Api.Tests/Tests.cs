using FluentAssertions;
using TradeProcessor.Domain;

namespace TradeProcessor.Api.Tests
{
	public class Tests
	{
		[Theory]
		[InlineData(37.1425, 10, 30)]
		[InlineData(31.1425, 10, 30)]
		[InlineData(0.0125, 0.01, 0.01)]

		public void testRounding(decimal quantity, decimal amountToRoundBy, decimal expectedQuantity)
		{
			var actualQuantity = quantity.RoundDownToMultiple(amountToRoundBy);

			actualQuantity.Should().Be(expectedQuantity);
		}
	}
}
