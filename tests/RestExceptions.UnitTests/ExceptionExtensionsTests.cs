namespace RestExceptions.UnitTests;

public class ExceptionExtensionsTests
{
    [Fact]
    public void MapToRestException_Uses_First_Mapper_That_Maps()
    {
        // Arrange
        IRestExceptionMapper[] mappers =
        [
            new StubMapper(_ => null),
            new StubMapper(_ => new ConflictRestException()),
            new StubMapper(_ => new GoneRestException())
        ];

        // Act
        var result = new InvalidOperationException().MapToRestException(mappers);

        // Assert
        result.ShouldBeOfType<ConflictRestException>();
    }

    [Fact]
    public void MapToRestException_Falls_Back_To_Built_In_Mapping()
    {
        // Arrange
        IRestExceptionMapper[] mappers = [new StubMapper(_ => null)];

        // Act
        var result = new KeyNotFoundException().MapToRestException(mappers);

        // Assert
        result.ShouldBeOfType<NotFoundRestException>();
    }

    [Fact]
    public void MapToRestException_Does_Not_Pass_RestException_To_Mappers()
    {
        // Arrange
        var mapper = new StubMapper(_ => new ConflictRestException());
        var exception = new GoneRestException();

        // Act
        var result = exception.MapToRestException([mapper]);

        // Assert
        result.ShouldBeSameAs(exception);
        mapper.Calls.ShouldBe(0);
    }
}
