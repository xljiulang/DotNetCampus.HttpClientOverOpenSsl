namespace DotNetCampus.HttpClientOverOpenSsl.Tests;

[TestClass]
public sealed class OpenSslSocketsHttpHandlerTests
{
    [TestMethod]
    public void Constructor_WithNullParameter_ShouldNotThrow()
    {
        // Arrange & Act
        var handler = new OpenSslSocketsHttpHandler(null);

        // Assert
        Assert.IsNotNull(handler);
        handler.Dispose();
    }

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
    public void Constructor_WithFreshSocketsHttpHandler_ShouldNotThrow()
    {
        // Arrange
        using var innerHandler = new SocketsHttpHandler();

        // Act
        var handler = new OpenSslSocketsHttpHandler(innerHandler);

        // Assert
        Assert.IsNotNull(handler);
        handler.Dispose();
    }

    [TestMethod]
    public void Constructor_WithProvidedHandler_SetsConnectCallback()
    {
        // Arrange
        using var innerHandler = new SocketsHttpHandler();

        // Act
        var handler = new OpenSslSocketsHttpHandler(innerHandler);

        // Assert
        Assert.IsNotNull(innerHandler.ConnectCallback, "ConnectCallback should be set on provided handler.");

        handler.Dispose();
    }

    [TestMethod]
    public void Constructor_WithConnectCallbackAlreadySet_ShouldThrowArgumentException()
    {
        // Arrange
        using var innerHandler = new SocketsHttpHandler();
        innerHandler.ConnectCallback = static (context, cancellationToken) =>
            ValueTask.FromResult<Stream>(Stream.Null);

        // Act
        void Act() => new OpenSslSocketsHttpHandler(innerHandler);

        // Assert
        var exception = Assert.Throws<ArgumentException>(Act);
        Assert.AreEqual("socketsHttpHandler", exception.ParamName);
        StringAssert.Contains(exception.Message, "ConnectCallback");
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