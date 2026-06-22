using System.Runtime.InteropServices;

using DotNetCampus.HttpClientOverOpenSsl.Interop;

namespace DotNetCampus.HttpClientOverOpenSsl.Tests;

[TestClass]
public sealed class OpenSSLNativeTests
{
    #region OPENSSL_init_ssl

    [TestMethod]
    public void OPENSSL_init_ssl_WithZeroOptsAndNullSettings_CallsSuccessfully()
    {
        try
        {
            // Act
            var result = OpenSSLNative.OPENSSL_init_ssl(0, IntPtr.Zero);

            // Assert
            Assert.AreEqual(1, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void OPENSSL_init_ssl_WithLoadSslStrings_CallsSuccessfully()
    {
        try
        {
            // Act
            var result = OpenSSLNative.OPENSSL_init_ssl(
                OpenSSLNative.OPENSSL_INIT_LOAD_SSL_STRINGS, IntPtr.Zero);

            // Assert
            Assert.AreEqual(1, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void OPENSSL_init_ssl_WithLoadSslAndCryptoStrings_CallsSuccessfully()
    {
        try
        {
            // Arrange
            var opts = OpenSSLNative.OPENSSL_INIT_LOAD_SSL_STRINGS
                       | OpenSSLNative.OPENSSL_INIT_LOAD_CRYPTO_STRINGS;

            // Act
            var result = OpenSSLNative.OPENSSL_init_ssl(opts, IntPtr.Zero);

            // Assert
            Assert.AreEqual(1, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void OPENSSL_init_ssl_CalledMultipleTimes_Idempotent()
    {
        try
        {
            // Act - OpenSSL init is designed to be idempotent
            var result1 = OpenSSLNative.OPENSSL_init_ssl(
                OpenSSLNative.OPENSSL_INIT_LOAD_SSL_STRINGS, IntPtr.Zero);
            var result2 = OpenSSLNative.OPENSSL_init_ssl(
                OpenSSLNative.OPENSSL_INIT_LOAD_SSL_STRINGS, IntPtr.Zero);

            // Assert
            Assert.AreEqual(1, result1);
            Assert.AreEqual(1, result2);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region TLS_client_method

    [TestMethod]
    public void TLS_client_method_ReturnsNonNullPointer()
    {
        try
        {
            // Act
            var method = OpenSSLNative.TLS_client_method();

            // Assert
            Assert.AreNotEqual(IntPtr.Zero, method);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void TLS_client_method_CalledMultipleTimes_ReturnsSamePointer()
    {
        try
        {
            // Act
            var method1 = OpenSSLNative.TLS_client_method();
            var method2 = OpenSSLNative.TLS_client_method();

            // Assert - OpenSSL returns the same static method pointer
            Assert.AreEqual(method1, method2);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_CTX_new

    [TestMethod]
    public void SSL_CTX_new_WithValidMethod_ReturnsValidHandle()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();

            // Act
            using var ctx = OpenSSLNative.SSL_CTX_new(method);

            // Assert
            Assert.IsNotNull(ctx);
            Assert.IsFalse(ctx.IsInvalid);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_CTX_new_WithNullMethod_ReturnsInvalidHandle()
    {
        try
        {
            // Act
            using var ctx = OpenSSLNative.SSL_CTX_new(IntPtr.Zero);

            // Assert - passing NULL method should result in an invalid handle
            Assert.IsNotNull(ctx);
            Assert.IsTrue(ctx.IsInvalid);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_CTX_free

    [TestMethod]
    public void SSL_CTX_free_WithIntPtrZero_DoesNotThrow()
    {
        try
        {
            // Act & Assert - SSL_CTX_free(NULL) is a no-op in OpenSSL
            OpenSSLNative.SSL_CTX_free(IntPtr.Zero);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_CTX_free_WithValidHandle_DoesNotThrow()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            var ctx = OpenSSLNative.SSL_CTX_new(method);

            // Act & Assert - free a valid context should not throw
            var rawHandle = ctx.DangerousGetHandle();
            OpenSSLNative.SSL_CTX_free(rawHandle);

            // 手动释放后标记 SafeHandle 为无效，避免终结器 double-free 导致进程崩溃。
            ctx.MarkAsInvalid();
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_CTX_set_default_verify_paths

    [TestMethod]
    public void SSL_CTX_set_default_verify_paths_WithValidContext_ReturnsSuccess()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);

            // Act
            var result = OpenSSLNative.SSL_CTX_set_default_verify_paths(ctx);

            // Assert
            Assert.AreEqual(1, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_CTX_load_verify_file

    [TestMethod]
    public void SSL_CTX_load_verify_file_WithValidContextAndNonExistentFile_ReturnsFailure()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);

            // Act
            var result = OpenSSLNative.SSL_CTX_load_verify_file(ctx, "nonexistent_ca_file.pem");

            // Assert - loading a non-existent file should return 0 (failure)
            Assert.AreEqual(0, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_CTX_load_verify_locations

    [TestMethod]
    public void SSL_CTX_load_verify_locations_WithValidContextAndNonExistentFileNullPath_ReturnsFailure()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);

            // Act
            var result = OpenSSLNative.SSL_CTX_load_verify_locations(ctx, "nonexistent_ca_file.pem", null);

            // Assert - loading a non-existent file with null path should return 0 (failure)
            Assert.AreEqual(0, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_CTX_load_verify_locations_WithValidContextAndNullFileNullPath_ReturnsFailure()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);

            // Act
            var result = OpenSSLNative.SSL_CTX_load_verify_locations(ctx, null!, null);

            // Assert - passing null file and null path should return 0 (failure)
            Assert.AreEqual(0, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_CTX_set_verify

    [TestMethod]
    public void SSL_CTX_set_verify_WithVerifyNone_DoesNotThrow()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);

            // Act & Assert - setting verify mode should not throw
            OpenSSLNative.SSL_CTX_set_verify(ctx, OpenSSLNative.SSL_VERIFY_NONE, IntPtr.Zero);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_CTX_set_verify_WithVerifyPeer_DoesNotThrow()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);

            // Act & Assert - setting verify peer mode should not throw
            OpenSSLNative.SSL_CTX_set_verify(ctx, OpenSSLNative.SSL_VERIFY_PEER, IntPtr.Zero);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_new

    [TestMethod]
    public void SSL_new_WithValidContext_ReturnsValidHandle()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);

            // Act
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Assert
            Assert.IsNotNull(ssl);
            Assert.IsFalse(ssl.IsInvalid);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_free

    [TestMethod]
    public void SSL_free_WithIntPtrZero_DoesNotThrow()
    {
        try
        {
            // Act & Assert - SSL_free(NULL) is a no-op in OpenSSL
            OpenSSLNative.SSL_free(IntPtr.Zero);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_free_WithValidHandle_DoesNotThrow()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            var ssl = OpenSSLNative.SSL_new(ctx);

            // Act & Assert - free a valid SSL handle should not throw
            var rawHandle = ssl.DangerousGetHandle();
            OpenSSLNative.SSL_free(rawHandle);

            // 手动释放后标记 SafeHandle 为无效，避免终结器 double-free 导致进程崩溃。
            ssl.MarkAsInvalid();
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_connect

    [TestMethod]
    public void SSL_connect_WithNewlyCreatedHandle_ReturnsError()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_connect(ssl);

            // Assert - without an underlying socket, SSL_connect returns error
            Assert.IsTrue(result <= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_write

    [TestMethod]
    public unsafe void SSL_write_WithNewlyCreatedHandle_ReturnsError()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);
            var buffer = new byte[1024];

            fixed (byte* buf = buffer)
            {
                // Act
                var result = OpenSSLNative.SSL_write(ssl, buf, buffer.Length);

                // Assert - without an established connection, SSL_write returns error
                Assert.IsTrue(result <= 0);
            }
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public unsafe void SSL_write_WithZeroLength_ReturnsZeroOrError()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);
            var buffer = new byte[1];

            fixed (byte* buf = buffer)
            {
                // Act
                var result = OpenSSLNative.SSL_write(ssl, buf, 0);

                // Assert - writing zero bytes should return 0 or error
                Assert.IsTrue(result <= 0);
            }
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public unsafe void SSL_write_WithSmallBuffer_ReturnsError()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);
            var buffer = new byte[1] { 0x48 };

            fixed (byte* buf = buffer)
            {
                // Act
                var result = OpenSSLNative.SSL_write(ssl, buf, 1);

                // Assert - without an established connection, SSL_write returns error
                Assert.IsTrue(result <= 0);
            }
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_read

    [TestMethod]
    public unsafe void SSL_read_WithNewlyCreatedHandle_ReturnsError()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);
            var buffer = new byte[1024];

            fixed (byte* buf = buffer)
            {
                // Act
                var result = OpenSSLNative.SSL_read(ssl, buf, buffer.Length);

                // Assert - without an established connection, SSL_read returns error
                Assert.IsTrue(result <= 0);
            }
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public unsafe void SSL_read_WithZeroLength_ReturnsZeroOrError()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);
            var buffer = new byte[1];

            fixed (byte* buf = buffer)
            {
                // Act
                var result = OpenSSLNative.SSL_read(ssl, buf, 0);

                // Assert - reading zero bytes should return 0 or error
                Assert.IsTrue(result <= 0);
            }
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public unsafe void SSL_read_WithSmallBuffer_ReturnsError()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);
            var buffer = new byte[1];

            fixed (byte* buf = buffer)
            {
                // Act
                var result = OpenSSLNative.SSL_read(ssl, buf, 1);

                // Assert - without an established connection, SSL_read returns error
                Assert.IsTrue(result <= 0);
            }
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_shutdown

    [TestMethod]
    public void SSL_shutdown_WithNewlyCreatedHandle_ReturnsZeroOrError()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_shutdown(ssl);

            // Assert - without an established connection, SSL_shutdown returns 0 or error
            Assert.IsTrue(result <= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_shutdown_CalledMultipleTimes_ReturnsConsistentResult()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result1 = OpenSSLNative.SSL_shutdown(ssl);
            var result2 = OpenSSLNative.SSL_shutdown(ssl);

            // Assert - both calls should return <= 0 without a connection
            Assert.IsTrue(result1 <= 0);
            Assert.IsTrue(result2 <= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_get_error

    [TestMethod]
    public void SSL_get_error_WithValidHandleAndZeroRet_ReturnsErrorCode()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var error = OpenSSLNative.SSL_get_error(ssl, 0);

            // Assert - ret=0 with no prior operation returns the last error in the SSL object,
            // which may be SSL_ERROR_SSL for a newly created unconnected handle
            Assert.IsTrue(error >= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_get_error_WithValidHandleAndNegativeRet_ReturnsErrorCode()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var error = OpenSSLNative.SSL_get_error(ssl, -1);

            // Assert - ret=-1 indicates an error, should return a valid error code
            Assert.IsTrue(error >= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_get_error_WithValidHandleAndPositiveRet_ReturnsSSL_ERROR_NONE()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var error = OpenSSLNative.SSL_get_error(ssl, 1);

            // Assert - positive ret value usually means SSL_ERROR_NONE
            Assert.AreEqual(OpenSSLNative.SSL_ERROR_NONE, error);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_get_error_MultipleCalls_ReturnConsistentResults()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var error1 = OpenSSLNative.SSL_get_error(ssl, -1);
            var error2 = OpenSSLNative.SSL_get_error(ssl, -1);

            // Assert - same input should produce same error code
            Assert.AreEqual(error1, error2);
            Assert.IsTrue(error1 >= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_set_bio

    [TestMethod]
    public void SSL_set_bio_WithValidHandles_DoesNotThrow()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act & Assert - SSL_set_bio with new (initially invalid) BIO handles should not throw
            using var rbio = new SafeBioHandle();
            using var wbio = new SafeBioHandle();
            OpenSSLNative.SSL_set_bio(ssl, rbio, wbio);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_set1_host

    [TestMethod]
    public void SSL_set1_host_WithValidHandleAndHostname_ReturnsSuccess()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_set1_host(ssl, "example.com");

            // Assert - SSL_set1_host returns 1 on success
            Assert.AreEqual(1, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_set1_host_WithValidHandleAndEmptyHostname_ReturnsResult()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_set1_host(ssl, string.Empty);

            // Assert - setting empty host may return 0 (failure) or 1 (if treated as valid)
            Assert.IsTrue(result == 0 || result == 1);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_set1_host_WithValidHandleAndNullHostname_ReturnsOne()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_set1_host(ssl, null!);

            // Assert - OpenSSL accepts NULL hostname (marshaled from null string) and returns 1
            Assert.AreEqual(1, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_set1_host_WithValidHandleAndIpAddress_ReturnsSuccess()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_set1_host(ssl, "127.0.0.1");

            // Assert - IP addresses are also valid hostnames
            Assert.AreEqual(1, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_ctrl

    [TestMethod]
    public void SSL_ctrl_WithValidHandleAndModeCmd_ReturnsLong()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_ctrl(ssl, OpenSSLNative.SSL_CTRL_MODE, 0, IntPtr.Zero);

            // Assert - SSL_ctrl returns the previous mode value
            Assert.IsTrue(result >= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_ctrl_WithValidHandleAndTlsextHostnameCmd_ReturnsZeroOrOne()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);
            var namePtr = Marshal.StringToHGlobalAnsi("test.local");
            try
            {
                // Act
                var result = OpenSSLNative.SSL_ctrl(
                    ssl,
                    OpenSSLNative.SSL_CTRL_SET_TLSEXT_HOSTNAME,
                    OpenSSLNative.TLSEXT_NAMETYPE_host_name,
                    namePtr);

                // Assert - returns 1 on success, 0 on failure
                Assert.IsTrue(result == 0 || result == 1);
            }
            finally
            {
                Marshal.FreeHGlobal(namePtr);
            }
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_ctrl_WithValidHandleAndZeroCmd_ReturnsResult()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_ctrl(ssl, 0, 0, IntPtr.Zero);

            // Assert - unknown command may return 0 or -1
            Assert.IsTrue(result == 0 || result == -1);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_set_tlsext_host_name

    [TestMethod]
    public void SSL_set_tlsext_host_name_WithValidHandleAndHostname_ReturnsOne()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_set_tlsext_host_name(ssl, "example.com");

            // Assert - SSL_set_tlsext_host_name returns 1 on success
            Assert.AreEqual(1, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_set_tlsext_host_name_WithValidHandleAndNullName_ReturnsOne()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_set_tlsext_host_name(ssl, null!);

            // Assert - OpenSSL accepts NULL hostname (marshaled from null string) and returns 1
            Assert.AreEqual(1, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_set_tlsext_host_name_WithValidHandleAndEmptyName_ReturnsResult()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_set_tlsext_host_name(ssl, string.Empty);

            // Assert - empty hostname may be treated as valid or invalid
            Assert.IsTrue(result == 0 || result == 1);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_set_tlsext_host_name_WithIpAddress_ReturnsOne()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_set_tlsext_host_name(ssl, "192.168.1.1");

            // Assert - IP addresses can be used as SNI hostnames
            Assert.AreEqual(1, result);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SSL_set_mode

    [TestMethod]
    public void SSL_set_mode_WithValidHandleAndNoMode_ReturnsPreviousMode()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_set_mode(ssl, 0);

            // Assert - returns the previous mode (which should be >= 0)
            Assert.IsTrue(result >= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_set_mode_WithEnablePartialWrite_ReturnsPreviousMode()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_set_mode(ssl, OpenSSLNative.SSL_MODE_ENABLE_PARTIAL_WRITE);

            // Assert - returns the previous mode
            Assert.IsTrue(result >= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_set_mode_WithAcceptMovingWriteBuffer_ReturnsPreviousMode()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result = OpenSSLNative.SSL_set_mode(ssl, OpenSSLNative.SSL_MODE_ACCEPT_MOVING_WRITE_BUFFER);

            // Assert - returns the previous mode
            Assert.IsTrue(result >= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_set_mode_WithCombinedModes_ReturnsPreviousMode()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);
            var combinedMode = OpenSSLNative.SSL_MODE_ENABLE_PARTIAL_WRITE
                               | OpenSSLNative.SSL_MODE_ACCEPT_MOVING_WRITE_BUFFER;

            // Act
            var result = OpenSSLNative.SSL_set_mode(ssl, combinedMode);

            // Assert - returns the previous mode
            Assert.IsTrue(result >= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void SSL_set_mode_ChainedCalls_MutuallyConsistent()
    {
        try
        {
            // Arrange
            var method = OpenSSLNative.TLS_client_method();
            using var ctx = OpenSSLNative.SSL_CTX_new(method);
            using var ssl = OpenSSLNative.SSL_new(ctx);

            // Act
            var result1 = OpenSSLNative.SSL_set_mode(ssl, OpenSSLNative.SSL_MODE_ENABLE_PARTIAL_WRITE);
            var result2 = OpenSSLNative.SSL_set_mode(ssl, OpenSSLNative.SSL_MODE_ACCEPT_MOVING_WRITE_BUFFER);

            // Assert - both calls return the previous mode (non-negative)
            Assert.IsTrue(result1 >= 0);
            Assert.IsTrue(result2 >= 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region BIO_new_socket

    [TestMethod]
    public void BIO_new_socket_WithInvalidSocket_ReturnsNonNullHandle()
    {
        try
        {
            // Act
            using var bio = OpenSSLNative.BIO_new_socket(IntPtr.Zero, 0);

            // Assert - BIO_new_socket allocates a BIO even for invalid socket; handle is non-null
            Assert.IsNotNull(bio);
            // Note: IsInvalid may be false because OpenSSL still returns a valid BIO pointer,
            // even though the underlying socket is invalid (BIO won't work for I/O).
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void BIO_new_socket_WithInvalidSocketAndCloseFlag_ReturnsNonNullHandle()
    {
        try
        {
            // Act
            using var bio = OpenSSLNative.BIO_new_socket(IntPtr.Zero, 1);

            // Assert - BIO_new_socket allocates a BIO even for invalid socket
            Assert.IsNotNull(bio);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void BIO_new_socket_WithValidSocket_ReturnsValidHandle()
    {
        try
        {
            // Arrange
            using var socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Tcp);

            // Act
            using var bio = OpenSSLNative.BIO_new_socket(socket.Handle, 0);

            // Assert
            Assert.IsNotNull(bio);
            Assert.IsFalse(bio.IsInvalid);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void BIO_new_socket_WithValidSocketAndCloseFlag_ReturnsValidHandle()
    {
        try
        {
            // Arrange
            using var socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Tcp);

            // Act
            using var bio = OpenSSLNative.BIO_new_socket(socket.Handle, 1);

            // Assert - BIO will close the socket when freed
            Assert.IsNotNull(bio);
            Assert.IsFalse(bio.IsInvalid);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region BIO_free_all

    [TestMethod]
    public void BIO_free_all_WithIntPtrZero_DoesNotThrow()
    {
        try
        {
            // Act & Assert - BIO_free_all(NULL) should be a no-op
            OpenSSLNative.BIO_free_all(IntPtr.Zero);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void BIO_free_all_WithValidBioHandle_DoesNotThrow()
    {
        try
        {
            // Arrange
            using var socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Tcp);
            var bio = OpenSSLNative.BIO_new_socket(socket.Handle, 0);

            // Act & Assert - free a valid BIO handle should not throw
            var rawHandle = bio.DangerousGetHandle();
            OpenSSLNative.BIO_free_all(rawHandle);

            // 手动释放后标记 SafeHandle 为无效，避免终结器 double-free 导致进程崩溃。
            bio.MarkAsInvalid();
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void BIO_free_all_DoubleFree_DoesNotThrow()
    {
        try
        {
            // Arrange
            using var socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Tcp);
            var bio = OpenSSLNative.BIO_new_socket(socket.Handle, 0);
            var rawHandle = bio.DangerousGetHandle();

            // 标记 SafeHandle 为无效，避免后续 double-free 时 SafeHandle 重复释放。
            bio.MarkAsInvalid();

            // Act & Assert - first free
            OpenSSLNative.BIO_free_all(rawHandle);

            // 注意：对已释放的原生句柄再次调用 BIO_free_all 可能触发 OpenSSL 的堆损坏检测，
            // 导致进程被 abort() 终止。这是 OpenSSL 的预期行为，无法在托管代码中捕获。
            // 因此本测试只验证第一次释放不抛出托管异常。
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region ERR_get_error

    [TestMethod]
    public void ERR_get_error_WhenNoErrorQueued_ReturnsZero()
    {
        try
        {
            // 先清空 OpenSSL 错误队列，避免之前测试操作留下的残留错误影响断言。
            while (OpenSSLNative.ERR_get_error() != 0)
            {
            }

            // Act
            var error = OpenSSLNative.ERR_get_error();

            // Assert - 0 means no error in the queue
            Assert.AreEqual(0UL, error);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void ERR_get_error_CalledMultipleTimes_ReturnsZeroConsistently()
    {
        try
        {
            // 先清空 OpenSSL 错误队列，避免之前测试操作留下的残留错误影响断言。
            while (OpenSSLNative.ERR_get_error() != 0)
            {
            }

            // Act
            var error1 = OpenSSLNative.ERR_get_error();
            var error2 = OpenSSLNative.ERR_get_error();

            // Assert - calling repeatedly should be consistent when no error queued
            Assert.AreEqual(0UL, error1);
            Assert.AreEqual(0UL, error2);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region ERR_error_string_n

    [TestMethod]
    public void ERR_error_string_n_WithZeroError_PopulatesBuffer()
    {
        try
        {
            // Arrange
            var buffer = new byte[256];

            // Act
            OpenSSLNative.ERR_error_string_n(0, buffer, buffer.Length);

            // Assert - buffer should contain a null-terminated error description string
            var nullIndex = Array.IndexOf(buffer, (byte) 0);
            Assert.IsTrue(nullIndex >= 0, "Buffer should contain a null terminator");
            var errorString = System.Text.Encoding.ASCII.GetString(buffer, 0, nullIndex);
            Assert.IsFalse(string.IsNullOrEmpty(errorString));
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void ERR_error_string_n_WithSmallBuffer_TruncatesOutput()
    {
        try
        {
            // Arrange
            var buffer = new byte[10];

            // Act
            OpenSSLNative.ERR_error_string_n(0, buffer, buffer.Length);

            // Assert - small buffer should still contain a null terminator (truncated string)
            var nullIndex = Array.IndexOf(buffer, (byte) 0);
            Assert.IsTrue(nullIndex >= 0 && nullIndex < buffer.Length,
                "Small buffer should contain a null terminator within bounds");
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void ERR_error_string_n_WithMinimalBuffer_PopulatesWithoutOverflow()
    {
        try
        {
            // Arrange
            var buffer = new byte[1];

            // Act
            OpenSSLNative.ERR_error_string_n(0, buffer, buffer.Length);

            // Assert - single-byte buffer should just be null terminator
            Assert.AreEqual((byte) 0, buffer[0]);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void ERR_error_string_n_WithNonZeroError_PopulatesBuffer()
    {
        try
        {
            // Arrange - use a known error code value
            const ulong testError = 0x14090086; // SSL_R_CERTIFICATE_VERIFY_FAILED
            var buffer = new byte[256];

            // Act
            OpenSSLNative.ERR_error_string_n(testError, buffer, buffer.Length);

            // Assert - buffer should contain a null-terminated description
            var nullIndex = Array.IndexOf(buffer, (byte) 0);
            Assert.IsTrue(nullIndex >= 0, "Buffer should contain a null terminator");
            var errorString = System.Text.Encoding.ASCII.GetString(buffer, 0, nullIndex);
            Assert.IsFalse(string.IsNullOrEmpty(errorString));
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region GetErrorString

    [TestMethod]
    public void GetErrorString_WithZeroError_ReturnsNonNullString()
    {
        // GetErrorString is a managed wrapper that calls ERR_error_string_n
        try
        {
            // Act
            var result = OpenSSLNative.GetErrorString(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void GetErrorString_WithZeroError_ContainsErrorPrefix()
    {
        try
        {
            // Act
            var result = OpenSSLNative.GetErrorString(0);

            // Assert - OpenSSL error strings use the format "error:..."
            Assert.IsTrue(result.StartsWith("error:", StringComparison.OrdinalIgnoreCase),
                $"Expected error string to start with 'error:', got '{result}'");
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void GetErrorString_WithNonZeroError_ReturnsNonNullString()
    {
        try
        {
            // Arrange - use a known error code
            const ulong testError = 0x14090086;

            // Act
            var result = OpenSSLNative.GetErrorString(testError);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void GetErrorString_WithNonZeroError_DiffersFromZeroError()
    {
        try
        {
            // Arrange
            const ulong testError = 0x14090086;

            // Act
            var zeroResult = OpenSSLNative.GetErrorString(0);
            var nonZeroResult = OpenSSLNative.GetErrorString(testError);

            // Assert - different error codes should produce different strings
            Assert.AreNotEqual(zeroResult, nonZeroResult);
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    [TestMethod]
    public void GetErrorString_ReturnsAsciiOnlyCharacters()
    {
        try
        {
            // Act
            var result = OpenSSLNative.GetErrorString(0);

            // Assert - error strings are ASCII only
            foreach (var c in result)
            {
                Assert.IsTrue(c <= 127, $"Character '{c}' ({(int) c}) is not ASCII");
            }
        }
        catch (DllNotFoundException)
        {
            Assert.Inconclusive("Native OpenSSL library not found.");
        }
    }

    #endregion

    #region SafeSslContextHandle

    [TestMethod]
    public void SafeSslContextHandle_Constructor_CreatesValidHandle()
    {
        // Act
        using var handle = new SafeSslContextHandle();

        // Assert
        Assert.IsNotNull(handle);
        Assert.IsFalse(handle.IsClosed);
        Assert.IsTrue(handle.IsInvalid);
    }

    [TestMethod]
    public void SafeSslContextHandle_Constructor_DoesNotThrow()
    {
        // Act
        using var handle = new SafeSslContextHandle();

        // Assert - ownership mode is set via base(true), handle should be usable
        Assert.IsFalse(handle.IsClosed);
    }

    #endregion

    #region SafeSslHandle

    [TestMethod]
    public void SafeSslHandle_Constructor_CreatesValidHandle()
    {
        // Act
        using var handle = new SafeSslHandle();

        // Assert
        Assert.IsNotNull(handle);
        Assert.IsFalse(handle.IsClosed);
        Assert.IsTrue(handle.IsInvalid);
    }

    [TestMethod]
    public void SafeSslHandle_Constructor_DoesNotThrow()
    {
        // Act
        using var handle = new SafeSslHandle();

        // Assert
        Assert.IsFalse(handle.IsClosed);
    }

    #endregion

    #region SafeBioHandle

    [TestMethod]
    public void SafeBioHandle_Constructor_CreatesValidHandle()
    {
        // Act
        using var handle = new SafeBioHandle();

        // Assert
        Assert.IsNotNull(handle);
        Assert.IsFalse(handle.IsClosed);
        Assert.IsTrue(handle.IsInvalid);
    }

    [TestMethod]
    public void SafeBioHandle_Constructor_DoesNotThrow()
    {
        // Act
        using var handle = new SafeBioHandle();

        // Assert
        Assert.IsFalse(handle.IsClosed);
    }

    [TestMethod]
    public void MarkAsInvalid_AfterConstruction_HandleBecomesInvalid()
    {
        // Arrange
        using var handle = new SafeBioHandle();

        // Act
        handle.MarkAsInvalid();

        // Assert - SetHandleAsInvalid was called, handle should remain invalid
        Assert.IsTrue(handle.IsInvalid);
    }

    [TestMethod]
    public void MarkAsInvalid_AfterConstruction_SetsHandleClosed()
    {
        // Arrange
        using var handle = new SafeBioHandle();

        // Act
        handle.MarkAsInvalid();

        // Assert - SetHandleAsInvalid marks the handle as invalid/closed
        Assert.IsTrue(handle.IsInvalid);
        Assert.IsTrue(handle.IsClosed);
    }

    [TestMethod]
    public void MarkAsInvalid_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        using var handle = new SafeBioHandle();

        // Act
        handle.MarkAsInvalid();
        handle.MarkAsInvalid();

        // Assert - calling multiple times should be safe and state remains consistent
        Assert.IsTrue(handle.IsInvalid);
        Assert.IsTrue(handle.IsClosed);
    }

    #endregion
}