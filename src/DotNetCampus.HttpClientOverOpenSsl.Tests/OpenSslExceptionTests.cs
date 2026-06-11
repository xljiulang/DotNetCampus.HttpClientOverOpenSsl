namespace DotNetCampus.HttpClientOverOpenSsl.Tests;

[TestClass]
public sealed class OpenSslExceptionTests
{
    [TestMethod]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        const string message = "SSL handshake failed";
        const int sslErrorCode = 5;
        const ulong openSslErrorCode = 0x14090086;

        // Act
        var exception = new OpenSslException(message, sslErrorCode, openSslErrorCode);

        // Assert
        Assert.AreEqual(message, exception.Message);
        Assert.AreEqual(sslErrorCode, exception.SslErrorCode);
        Assert.AreEqual(openSslErrorCode, exception.OpenSslErrorCode);
    }

    [TestMethod]
    public void Constructor_WithoutOpenSslErrorCode_DefaultsToZero()
    {
        // Arrange
        const string message = "SSL read error";
        const int sslErrorCode = 2;

        // Act
        var exception = new OpenSslException(message, sslErrorCode);

        // Assert
        Assert.AreEqual(message, exception.Message);
        Assert.AreEqual(sslErrorCode, exception.SslErrorCode);
        Assert.AreEqual(0UL, exception.OpenSslErrorCode);
    }

    [TestMethod]
    public void Constructor_IsIOException()
    {
        // Arrange & Act
        var exception = new OpenSslException("test", 1);

        // Assert
        Assert.IsInstanceOfType<IOException>(exception);
    }

    [TestMethod]
    public void Constructor_WithNullMessage_DoesNotThrowAndPreservesErrorCodes()
    {
        // Arrange & Act
        var exception = new OpenSslException(null!, 3, 42);

        // Assert — Exception.Message returns a system-supplied default when message is null
        Assert.IsNotNull(exception.Message);
        Assert.AreEqual(3, exception.SslErrorCode);
        Assert.AreEqual(42UL, exception.OpenSslErrorCode);
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_SetsMessage()
    {
        // Arrange & Act
        var exception = new OpenSslException(string.Empty, 0);

        // Assert
        Assert.AreEqual(string.Empty, exception.Message);
        Assert.AreEqual(0, exception.SslErrorCode);
    }

    [TestMethod]
    public void Constructor_WithNegativeSslErrorCode_PreservesValue()
    {
        // Arrange
        const int sslErrorCode = -1;

        // Act
        var exception = new OpenSslException("error", sslErrorCode);

        // Assert
        Assert.AreEqual(sslErrorCode, exception.SslErrorCode);
    }

    [TestMethod]
    public void Constructor_WithMaxValues_PreservesValues()
    {
        // Arrange
        const int sslErrorCode = int.MaxValue;
        const ulong openSslErrorCode = ulong.MaxValue;

        // Act
        var exception = new OpenSslException("max", sslErrorCode, openSslErrorCode);

        // Assert
        Assert.AreEqual(sslErrorCode, exception.SslErrorCode);
        Assert.AreEqual(openSslErrorCode, exception.OpenSslErrorCode);
    }
}
