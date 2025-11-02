using FluentAssertions;
using MottuProjeto.Infrastructure;
using Xunit;

public class PaginationHelperTests
{
    [Theory]
    [InlineData(-1, 0, 1, 10)]
    [InlineData(0, 1, 1, 1)]
    [InlineData(1, 500, 1, 100)]
    [InlineData(2, 20, 2, 20)]
    [InlineData(10, -5, 10, 10)]
    public void Clamp_Deve_Normalizar_Page_Size(int inPage, int inSize, int expPage, int expSize)
    {
        // Arrange / Act
        var (page, size) = PaginationHelper.Clamp(inPage, inSize);

        // Assert
        page.Should().Be(expPage);
        size.Should().Be(expSize);
    }
}
