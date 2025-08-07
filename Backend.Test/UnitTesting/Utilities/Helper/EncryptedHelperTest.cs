using FluentAssertions;
using System.Security.Cryptography;
using Backend.Utilities.Helper;

namespace Backend.Test.UnitTesting.Utilities.Helper;

public class EncryptedHelperTest
{
    [Fact]
    public void CanEncrypt()
    {
        #region Arrange
        var keyValues = new List<KeyValuePair<string, string?>>
        {
            new KeyValuePair<string, string?>("Encryption:PrivateKeyPassword", "8d3f3d9dedac545a052b1cec94e3e3c47c84bccf776365e890ce48d7064d3961")
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(keyValues)
            .Build();

        var encrypted = new EncryptedHelper(configuration);
        #endregion

        #region Act
        var result = encrypted.EncryptAes("test encrypt text");
        #endregion
        
        #region Assert
        result.Should().NotBeEmpty();
        #endregion
    }

    [Fact]
    public void CanDecrypt()
    {
        #region Arrange
        var keyValues = new List<KeyValuePair<string, string?>>
        {
            new KeyValuePair<string, string?>("Encryption:PrivateKeyPassword", "8d3f3d9dedac545a052b1cec94e3e3c47c84bccf776365e890ce48d7064d3961")
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(keyValues)
            .Build();

        var encrypted = new EncryptedHelper(configuration);
        #endregion

        #region Act
        var result = encrypted.DecryptAes("FYurFLvAnZQqKqrYRCioy53MPL1B3tg8OzeCvrd1mKg4raC4yltv2zZSFqBK");
        #endregion

        #region Assert
        result.Should().NotBeEmpty();
        result.Should().BeEquivalentTo("test encrypt text");
        #endregion
    }

    [Fact]
    public void CanCompareEncryptedTextWithPlainText()
    {
        #region Arrange
        var keyValues = new List<KeyValuePair<string, string?>>
        {
            new KeyValuePair<string, string?>("Encryption:PrivateKeyPassword", "8d3f3d9dedac545a052b1cec94e3e3c47c84bccf776365e890ce48d7064d3961")
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(keyValues)
            .Build();

        var encrypted = new EncryptedHelper(configuration);
        #endregion

        #region Act
        var result1 = encrypted.CompareEncryptedTextWithPlainText("FYurFLvAnZQqKqrYRCioy53MPL1B3tg8OzeCvrd1mKg4raC4yltv2zZSFqBK", "test encrypt text");
        var result2 = encrypted.CompareEncryptedTextWithPlainText("y3Hr6zR9/oLBvIXS2RHq9CZR/cTJosp3lguOK6cAuHuCb4Mwb5gkeo1NYKYV", "test encrypt text");
        var result3 = encrypted.CompareEncryptedTextWithPlainText("aUReQeWFq981lLMhhIP+4b55yOLr0vStlx1pcIIxSY1UI1PBacq8eeMyWJ9Qkg==", "test encrypt text");
        #endregion

        #region Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeFalse();
        #endregion
    }
}
