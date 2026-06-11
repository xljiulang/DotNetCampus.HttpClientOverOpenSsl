using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace DotNetCampus.HttpClientOverOpenSsl.Tests;

[TestClass]
[SuppressMessage("Design", "CA2022:Avoid inexact read with Stream.Read", Justification = "Tests intentionally call Read/ReadAsync to verify validation logic, not to consume stream data.")]
public sealed class OpenSslAsyncStreamTests : IDisposable
{
    private Socket? _socket;

    [TestInitialize]
    public void TestInitialize()
    {
        _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _socket?.Dispose();
        _socket = null;
    }

    public void Dispose()
    {
        _socket?.Dispose();
    }

    #region Constructor Tests

    [TestMethod]
    public void Constructor_NullSocket_ThrowsArgumentNullException()
    {
        // Act & Assert
        try
        {
            _ = new OpenSslAsyncStream(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown.");
        }
        catch (ArgumentNullException)
        {
            // Expected exception.
        }
    }

    [TestMethod]
    public void Constructor_ValidSocket_DefaultOwnsSocket_DoesNotThrow()
    {
        // Act
        using var stream = new OpenSslAsyncStream(_socket!);

        // Assert - no exception thrown
        Assert.IsNotNull(stream);
    }

    [TestMethod]
    public void Constructor_ValidSocket_OwnsSocketTrue_DoesNotThrow()
    {
        // Act
        using var stream = new OpenSslAsyncStream(_socket!, ownsSocket: true);

        // Assert - no exception thrown
        Assert.IsNotNull(stream);
    }

    [TestMethod]
    public void Constructor_ValidSocket_OwnsSocketFalse_DoesNotThrow()
    {
        // Act
        using var stream = new OpenSslAsyncStream(_socket!, ownsSocket: false);

        // Assert - no exception thrown
        Assert.IsNotNull(stream);
    }

    #endregion

    #region Property Tests

    [TestMethod]
    public void CanRead_AlwaysReturnsTrue()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);

        // Act
        var result = stream.CanRead;

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void CanSeek_AlwaysReturnsFalse()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);

        // Act
        var result = stream.CanSeek;

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CanWrite_AlwaysReturnsTrue()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);

        // Act
        var result = stream.CanWrite;

        // Assert
        Assert.IsTrue(result);
    }

    #endregion

    #region AuthenticateAsClientAsync Validation Tests

    [TestMethod]
    public async Task AuthenticateAsClientAsync_Disposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var stream = new OpenSslAsyncStream(_socket!);
        stream.Dispose();
        var options = new OpenSslClientAuthenticationOptions { TargetHost = "example.com" };

        // Act & Assert
        try
        {
            await stream.AuthenticateAsClientAsync(options);
            Assert.Fail("Expected ObjectDisposedException was not thrown.");
        }
        catch (ObjectDisposedException)
        {
            // Expected exception.
        }
    }

    [TestMethod]
    public async Task AuthenticateAsClientAsync_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);

        // Act & Assert
        try
        {
            await stream.AuthenticateAsClientAsync(null!);
            Assert.Fail("Expected ArgumentNullException was not thrown.");
        }
        catch (ArgumentNullException)
        {
            // Expected exception.
        }
    }

    [TestMethod]
    public async Task AuthenticateAsClientAsync_NullTargetHost_ThrowsArgumentException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);
        var options = new OpenSslClientAuthenticationOptions { TargetHost = null! };

        // Act & Assert
        try
        {
            await stream.AuthenticateAsClientAsync(options);
            Assert.Fail("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException ex)
        {
            StringAssert.Contains(ex.Message, "目标主机名不能为空");
        }
    }

    [TestMethod]
    public async Task AuthenticateAsClientAsync_EmptyTargetHost_ThrowsArgumentException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);
        var options = new OpenSslClientAuthenticationOptions { TargetHost = string.Empty };

        // Act & Assert
        try
        {
            await stream.AuthenticateAsClientAsync(options);
            Assert.Fail("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException ex)
        {
            StringAssert.Contains(ex.Message, "目标主机名不能为空");
        }
    }

    [TestMethod]
    public async Task AuthenticateAsClientAsync_WhitespaceTargetHost_ThrowsArgumentException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);
        var options = new OpenSslClientAuthenticationOptions { TargetHost = "   " };

        // Act & Assert
        try
        {
            await stream.AuthenticateAsClientAsync(options);
            Assert.Fail("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException ex)
        {
            StringAssert.Contains(ex.Message, "目标主机名不能为空");
        }
    }

    #endregion

    #region Length Property Tests

    [TestMethod]
    public void Length_Get_ThrowsNotSupportedException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);

        // Act & Assert
        try
        {
            _ = stream.Length;
            Assert.Fail("Expected NotSupportedException was not thrown.");
        }
        catch (NotSupportedException)
        {
            // Expected exception.
        }
    }

    #endregion

    #region Position Property Tests

    [TestMethod]
    public void Position_Get_ThrowsNotSupportedException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);

        // Act & Assert
        try
        {
            _ = stream.Position;
            Assert.Fail("Expected NotSupportedException was not thrown.");
        }
        catch (NotSupportedException)
        {
            // Expected exception.
        }
    }

    [TestMethod]
    public void Position_Set_ThrowsNotSupportedException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);

        // Act & Assert
        try
        {
            stream.Position = 42;
            Assert.Fail("Expected NotSupportedException was not thrown.");
        }
        catch (NotSupportedException)
        {
            // Expected exception.
        }
    }

    #endregion

    #region Read Validation Tests

    [TestMethod]
    public void Read_Disposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var stream = new OpenSslAsyncStream(_socket!);
        stream.Dispose();
        var buffer = new byte[16];

        // Act & Assert
        try
        {
            stream.Read(buffer, 0, buffer.Length);
            Assert.Fail("Expected ObjectDisposedException was not thrown.");
        }
        catch (ObjectDisposedException)
        {
            // Expected exception.
        }
    }

    [TestMethod]
    public void Read_NotAuthenticated_ThrowsInvalidOperationException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);
        var buffer = new byte[16];

        // Act & Assert
        try
        {
            stream.Read(buffer, 0, buffer.Length);
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
            // Expected exception.
        }
    }

    #endregion

    #region ReadAsync Validation Tests

    [TestMethod]
    public async Task ReadAsync_Disposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var stream = new OpenSslAsyncStream(_socket!);
        stream.Dispose();
        var buffer = new byte[16];

        // Act & Assert
        try
        {
            await stream.ReadAsync(buffer, 0, buffer.Length);
            Assert.Fail("Expected ObjectDisposedException was not thrown.");
        }
        catch (ObjectDisposedException)
        {
            // Expected exception.
        }
    }

    [TestMethod]
    public async Task ReadAsync_NotAuthenticated_ThrowsInvalidOperationException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);
        var buffer = new byte[16];

        // Act & Assert
        try
        {
            await stream.ReadAsync(buffer, 0, buffer.Length);
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
            // Expected exception.
        }
    }

    #endregion

    #region Write Validation Tests

    [TestMethod]
    public void Write_Disposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var stream = new OpenSslAsyncStream(_socket!);
        stream.Dispose();
        var buffer = new byte[16];

        // Act & Assert
        try
        {
            stream.Write(buffer, 0, buffer.Length);
            Assert.Fail("Expected ObjectDisposedException was not thrown.");
        }
        catch (ObjectDisposedException)
        {
            // Expected exception.
        }
    }

    [TestMethod]
    public void Write_NotAuthenticated_ThrowsInvalidOperationException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);
        var buffer = new byte[16];

        // Act & Assert
        try
        {
            stream.Write(buffer, 0, buffer.Length);
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
            // Expected exception.
        }
    }

    #endregion

    #region WriteAsync Validation Tests

    [TestMethod]
    public async Task WriteAsync_Disposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var stream = new OpenSslAsyncStream(_socket!);
        stream.Dispose();
        var buffer = new byte[16];

        // Act & Assert
        try
        {
            await stream.WriteAsync(buffer, 0, buffer.Length);
            Assert.Fail("Expected ObjectDisposedException was not thrown.");
        }
        catch (ObjectDisposedException)
        {
            // Expected exception.
        }
    }

    [TestMethod]
    public async Task WriteAsync_NotAuthenticated_ThrowsInvalidOperationException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);
        var buffer = new byte[16];

        // Act & Assert
        try
        {
            await stream.WriteAsync(buffer, 0, buffer.Length);
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
            // Expected exception.
        }
    }

    #endregion

    #region Flush Tests

    [TestMethod]
    public void Flush_DoesNotThrow()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);

        // Act - Flush is a no-op
        stream.Flush();

        // Assert - no exception thrown
    }

    [TestMethod]
    public void Flush_AfterDispose_DoesNotThrow()
    {
        // Arrange
        var stream = new OpenSslAsyncStream(_socket!);
        stream.Dispose();

        // Act - Flush is a no-op even after dispose
        stream.Flush();

        // Assert - no exception thrown
    }

    #endregion

    #region FlushAsync Tests

    [TestMethod]
    public async Task FlushAsync_ReturnsCompletedTask()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);

        // Act
        var task = stream.FlushAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.AreSame(Task.CompletedTask, task);
    }

    [TestMethod]
    public async Task FlushAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        try
        {
            await stream.FlushAsync(cts.Token);
            Assert.Fail("Expected OperationCanceledException was not thrown.");
        }
        catch (OperationCanceledException)
        {
            // Expected exception.
        }
    }

    #endregion

    #region Seek Tests

    [TestMethod]
    public void Seek_Always_ThrowsNotSupportedException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);

        // Act & Assert
        try
        {
            stream.Seek(0, SeekOrigin.Begin);
            Assert.Fail("Expected NotSupportedException was not thrown.");
        }
        catch (NotSupportedException)
        {
            // Expected exception.
        }
    }

    #endregion

    #region SetLength Tests

    [TestMethod]
    public void SetLength_Always_ThrowsNotSupportedException()
    {
        // Arrange
        using var stream = new OpenSslAsyncStream(_socket!);

        // Act & Assert
        try
        {
            stream.SetLength(0);
            Assert.Fail("Expected NotSupportedException was not thrown.");
        }
        catch (NotSupportedException)
        {
            // Expected exception.
        }
    }

    #endregion
}
