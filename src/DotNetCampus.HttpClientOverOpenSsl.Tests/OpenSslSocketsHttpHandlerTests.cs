namespace DotNetCampus.HttpClientOverOpenSsl.Tests;

[TestClass]
public sealed class OpenSslSocketsHttpHandlerTests
{
    [TestMethod]
    public void Constructor_WithDefaultParameter_ShouldNotThrow()
    {
        // Arrange & Act
        var handler = new OpenSslSocketsHttpHandler();

        // Assert
        Assert.IsNotNull(handler);
        handler.Dispose();
    }

    [TestMethod]
    public void Dispose_ShouldDisposeWithoutException()
    {
        // Arrange
        var handler = new OpenSslSocketsHttpHandler();

        // Act
        handler.Dispose();

        // Assert — passes if no exception thrown
    }

    [TestMethod]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var handler = new OpenSslSocketsHttpHandler();

        // Act
        handler.Dispose();
        handler.Dispose();

        // Assert — passes if no exception thrown
    }
}