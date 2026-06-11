using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DotNetCampus.HttpClientOverOpenSsl.Interop;

namespace DotNetCampus.HttpClientOverOpenSsl.Tests;

[TestClass]
public sealed class OpenSslCertificateLoaderTests
{
    [TestMethod]
    public void ExportCertificateAsPem_ValidCertificate_ReturnsPemStringWithCorrectFormat()
    {
        // Arrange
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest("CN=Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var cert = req.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddDays(1));

        // Act
        var pem = OpenSslCertificateLoader.ExportCertificateAsPem(cert);

        // Assert
        Assert.IsNotNull(pem);
        StringAssert.StartsWith(pem, "-----BEGIN CERTIFICATE-----");
        StringAssert.Contains(pem, "-----END CERTIFICATE-----");
        Assert.IsTrue(pem.Length > 0);
    }

    [TestMethod]
    public void ExportCertificateAsPem_ValidCertificate_ContainsBase64EncodedRawData()
    {
        // Arrange
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest("CN=Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var cert = req.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddDays(1));

        // Act
        var pem = OpenSslCertificateLoader.ExportCertificateAsPem(cert);

        // Assert
        var lines = pem.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        // PEM format: BEGIN line, base64 line(s), END line
        Assert.IsTrue(lines.Length >= 3, "PEM should have at least 3 lines");
        Assert.AreEqual("-----BEGIN CERTIFICATE-----", lines[0].TrimEnd('\r'));
        Assert.AreEqual("-----END CERTIFICATE-----", lines[^1].TrimEnd('\r'));

        // The middle lines should be valid base64
        for (var i = 1; i < lines.Length - 1; i++)
        {
            var base64Line = lines[i].TrimEnd('\r');
            Assert.IsTrue(Convert.TryFromBase64String(base64Line, new byte[64], out var bytesWritten),
                $"Line {i} should be valid base64: '{base64Line}'");
        }
    }

    [TestMethod]
    public void ExportCertificateAsPem_ValidCertificate_HasCorrectHeaderFooter()
    {
        // Arrange
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest("CN=Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var cert = req.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddDays(1));

        // Act
        var pem = OpenSslCertificateLoader.ExportCertificateAsPem(cert);

        // Assert
        StringAssert.StartsWith(pem, "-----BEGIN CERTIFICATE-----");
        // StringBuilder.AppendLine uses Environment.NewLine, so trailing newline is platform-specific
        Assert.IsTrue(pem.EndsWith("-----END CERTIFICATE-----" + Environment.NewLine),
            "PEM should end with the certificate footer followed by a newline.");
    }

    [TestMethod]
    public void ExportCertificateAsPem_NullCertificate_ThrowsNullReferenceException()
    {
        try
        {
            OpenSslCertificateLoader.ExportCertificateAsPem(null!);
            Assert.Fail("Expected NullReferenceException was not thrown.");
        }
        catch (NullReferenceException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task LoadWindowsRootCertsAsync_NonWindows_ReturnsImmediately()
    {
        // On non-Windows, the method returns before accessing the cert store.
        // On Windows, this test simply passes (no assertion to make on the non-Windows path).
        if (!OperatingSystem.IsWindows())
        {
            using var sslContext = new SafeSslContextHandle();
            await OpenSslCertificateLoader.LoadWindowsRootCertsAsync(sslContext);
            // Assert: no exception thrown means success for non-Windows path.
        }
    }

    [TestMethod]
    public async Task LoadWindowsRootCertsAsync_NullSslContext_NonWindows_ReturnsImmediately()
    {
        // On non-Windows, the OS guard returns before sslContext is dereferenced.
        if (!OperatingSystem.IsWindows())
        {
            await OpenSslCertificateLoader.LoadWindowsRootCertsAsync(null!);
            // Assert: no exception thrown means success for non-Windows path.
        }
    }

    [TestMethod]
    public async Task LoadWindowsRootCertsAsync_NullSslContext_Windows_ThrowsException()
    {
        // On Windows, null SafeSslContextHandle is passed to native interop which throws
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("This test only validates the Windows path.");
        }

        try
        {
            await OpenSslCertificateLoader.LoadWindowsRootCertsAsync(null!);
            Assert.Fail("Expected an exception was not thrown.");
        }
        catch (ArgumentNullException)
        {
            // SafeHandle interop throws ArgumentNullException: "SafeHandle cannot be null."
        }
        catch (NullReferenceException)
        {
            // Possible alternative path
        }
    }
}