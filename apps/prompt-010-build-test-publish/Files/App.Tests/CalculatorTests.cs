using Xunit;

public class CalculatorTests
{
    [Fact]
    public void AddsTwoValues()
        => Assert.Equal(5, App.Calculator.Add(2, 3));
}
