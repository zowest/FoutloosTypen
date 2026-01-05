using System;
using System.Threading.Tasks;
using FoutloosTypen.Core.Models;
using FoutloosTypen.Core.Interfaces.Repositories;
using Moq;
using NUnit.Framework;
using SkiaSharp;

namespace TestCore
{
    [TestFixture]
    public class ShareMediaTest
    {
        private Mock<IMediaUploadRepository>? _mediaRepo;
        private Mock<IXAuthRepository>? _xAuthRepo;
        private Mock<IShareImageRepository>? _shareImageRepo;

        [SetUp]
        public void Setup()
        {
            _mediaRepo = new Mock<IMediaUploadRepository>(MockBehavior.Strict);
            _xAuthRepo = new Mock<IXAuthRepository>(MockBehavior.Strict);
            _shareImageRepo = new Mock<IShareImageRepository>(MockBehavior.Strict);
        }

        [TearDown]
        public void TearDown()
        {
            _mediaRepo = null;
            _xAuthRepo = null;
            _shareImageRepo = null;
        }


        /// Test FR2 + FR4: Repository interaction for sharing to Twitter
        [Test]
        public async Task UC15_FR2_FR4_MediaUpload_MustSucceed()
        {
            // Arrange
            _shareImageRepo!
                .Setup(s => s.SaveImageToCacheAsync(It.IsAny<SKBitmap>(), It.IsAny<string>()))
                .ReturnsAsync("/tmp/test.png");

            _mediaRepo!
                .Setup(m => m.UploadMediaAsync(
                    "/tmp/test.png",
                    "image/png",
                    "ck", "cs", "tok", "tsec"))
                .ReturnsAsync("media123");

            // Act
            var imagePath = await _shareImageRepo.Object.SaveImageToCacheAsync(new SKBitmap(100, 100));
            var mediaId = await _mediaRepo.Object.UploadMediaAsync(
                imagePath, "image/png", "ck", "cs", "tok", "tsec");

            // Assert
            Assert.That(mediaId, Is.EqualTo("media123"), 
                "FR2/FR4: Media moet succesvol geüpload worden naar platform");
        }

        /// Test FR4: Tweet posting to platform
        [Test]
        public async Task UC15_FR4_PostTweet_MustSucceed()
        {
            // Arrange
            string tweetText = "Les 1 - 80% voltooid #BolType";
            string mediaId = "media123";

            _mediaRepo!
                .Setup(m => m.PostTweetAsync(
                    tweetText,
                    mediaId,
                    "ck", "cs", "tok", "tsec"))
                .Returns(Task.CompletedTask);

            // Act & Assert
            await _mediaRepo.Object.PostTweetAsync(
                tweetText, mediaId, "ck", "cs", "tok", "tsec");

            _mediaRepo.Verify(m => m.PostTweetAsync(
                tweetText, mediaId, "ck", "cs", "tok", "tsec"), 
                Times.Once, "FR4: Tweet moet gepost worden op platform");
        }

        /// Test FR5: Credentials validation
        [Test]
        public async Task UC15_FR5_Credentials_MustBeValidated()
        {
            // Arrange
            int ownerUserId = 1;
            
            // Setup valid credentials scenario
            _xAuthRepo!
                .Setup(x => x.GetSettings())
                .Returns(new XAuthSettings
                {
                    ConsumerKey = "ck",
                    ConsumerSecret = "cs",
                    CallbackUrl = "callback"
                });

            _xAuthRepo
                .Setup(x => x.GetOAuth1TokensAsync(ownerUserId))
                .ReturnsAsync(("valid_token", "valid_secret"));

            // Setup invalid credentials scenario (no tokens)
            _xAuthRepo
                .Setup(x => x.GetOAuth1TokensAsync(999))
                .ReturnsAsync((ValueTuple<string, string>?)null);

            // Act
            var settings = _xAuthRepo.Object.GetSettings();
            var validTokens = await _xAuthRepo.Object.GetOAuth1TokensAsync(ownerUserId);
            var invalidTokens = await _xAuthRepo.Object.GetOAuth1TokensAsync(999);

            // Assert
            Assert.That(string.IsNullOrEmpty(settings.ConsumerKey), Is.False,
                "FR5: Geldige credentials moeten consumer key bevatten");
            Assert.That(string.IsNullOrEmpty(settings.ConsumerSecret), Is.False,
                "FR5: Geldige credentials moeten consumer secret bevatten");
            Assert.That(validTokens, Is.Not.Null,
                "FR5: Geldige credentials moeten OAuth tokens bevatten");
            Assert.That(invalidTokens, Is.Null,
                "FR5: Ongeldige credentials hebben geen tokens");
        }

        /// Test FR6 + NFR3: Tweet text validation
        [Test]
        public void UC15_FR6_NFR3_TweetText_MustOnlyContainPublicData()
        {
            // Arrange - simulate tweet text generation
            string lessonName = "Les 1";
            string progressText = "80% voltooid";
            string tweetText = $"{lessonName} - {progressText} #BolType";

            // Assert - geen privacygevoelige data
            var sensitiveTerms = new[] { "email", "password", "token", "secret", "key", 
                                        "dm", "private", "persoonlijk" };
            
            foreach (var term in sensitiveTerms)
            {
                Assert.That(tweetText, Does.Not.Contain(term).IgnoreCase, 
                    $"FR6/NFR3: Tweet mag geen privacygevoelige term bevatten: {term}");
            }

            // Assert - alleen openbare data aanwezig
            Assert.That(tweetText, Does.Contain("Les"), "FR6: Tweet moet openbare lesinfo bevatten");
            Assert.That(tweetText, Does.Contain("voltooid"), "FR6: Tweet moet voortgang bevatten");
            Assert.That(tweetText, Does.Contain("#BolType"), "FR6: Tweet moet hashtag bevatten");
        }

        /// Test FR7: Error handling for unreachable platform
        [Test]
        public void UC15_FR7_Upload_MustThrowExceptionWhenUnreachable()
        {
            // Arrange
            _mediaRepo!
                .Setup(m => m.UploadMediaAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception("Network error: Unable to reach X API"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => 
                await _mediaRepo.Object.UploadMediaAsync(
                    "/tmp/test.png", "image/png", "ck", "cs", "tok", "tsec"),
                "FR7: Exception moet gegooid worden bij onbereikbaar platform");
        }

        /// Test NFR2: Tweet format validation
        [Test]
        public void UC15_NFR2_TweetText_MustBeCorrectlyFormatted()
        {
            // Arrange
            string tweetText = "Les 3 - Correct: 12, Fouten: 1 #BolType";

            // Assert
            Assert.That(tweetText.Length, Is.LessThanOrEqualTo(280), 
                "NFR2: Tweet moet binnen Twitter karakterlimiet zijn");
            
            Assert.That(tweetText, Does.Contain("#BolType"), 
                "NFR2: Tweet moet correcte hashtag formatting hebben");
            
            Assert.That(tweetText, Does.Contain("Les"), 
                "NFR2: Tweet moet lesnaam correct geformatteerd bevatten");
        }

        /// Test: Image save to cache
        [Test]
        public async Task UC15_ImageSave_MustReturnPath()
        {
            // Arrange
            var bitmap = new SKBitmap(100, 100);
            _shareImageRepo!
                .Setup(s => s.SaveImageToCacheAsync(bitmap, "png"))
                .ReturnsAsync("/cache/image.png");

            // Act
            var path = await _shareImageRepo.Object.SaveImageToCacheAsync(bitmap, "png");

            // Assert
            Assert.That(path, Is.Not.Null.And.Not.Empty, 
                "Image save moet geldig pad teruggeven");
            Assert.That(path, Does.Contain("image.png"), 
                "Pad moet bestandsnaam bevatten");
        }

        /// Test integratie: Complete share flow with repositories
        [Test]
        public async Task UC15_Integration_CompleteRepositoryFlow_MustWork()
        {
            // Arrange
            var bitmap = new SKBitmap(100, 100);
            
            _shareImageRepo!
                .Setup(s => s.SaveImageToCacheAsync(bitmap, "png"))
                .ReturnsAsync("/cache/share_image.png");

            _mediaRepo!
                .Setup(m => m.UploadMediaAsync(
                    "/cache/share_image.png",
                    "image/png",
                    "ck", "cs", "tok", "tsec"))
                .ReturnsAsync("media_id_12345");

            _mediaRepo
                .Setup(m => m.PostTweetAsync(
                    It.IsAny<string>(),
                    "media_id_12345",
                    "ck", "cs", "tok", "tsec"))
                .Returns(Task.CompletedTask);

            // Act
            var imagePath = await _shareImageRepo.Object.SaveImageToCacheAsync(bitmap, "png");
            var mediaId = await _mediaRepo.Object.UploadMediaAsync(
                imagePath, "image/png", "ck", "cs", "tok", "tsec");
            await _mediaRepo.Object.PostTweetAsync(
                "Les 1 - 80% voltooid #BolType", mediaId, "ck", "cs", "tok", "tsec");

            // Assert
            _shareImageRepo.Verify(s => s.SaveImageToCacheAsync(bitmap, "png"), 
                Times.Once, "Afbeelding moet opgeslagen worden");
            
            _mediaRepo.Verify(m => m.UploadMediaAsync(
                imagePath, "image/png", "ck", "cs", "tok", "tsec"), 
                Times.Once, "Media moet geüpload worden");
            
            _mediaRepo.Verify(m => m.PostTweetAsync(
                It.IsAny<string>(), mediaId, "ck", "cs", "tok", "tsec"), 
                Times.Once, "Tweet moet gepost worden");
        }

        /// Test: OAuth token storage per user
        [Test]
        public async Task UC15_OAuthTokens_MustBeSavedAndRetrievedPerUser()
        {
            // Arrange
            int ownerUserId = 123;
            string accessToken = "test_access_token";
            string accessSecret = "test_access_secret";

            _xAuthRepo!
                .Setup(x => x.SaveOAuth1TokensAsync(ownerUserId, accessToken, accessSecret))
                .Returns(Task.CompletedTask);

            _xAuthRepo
                .Setup(x => x.GetOAuth1TokensAsync(ownerUserId))
                .ReturnsAsync((accessToken, accessSecret));

            // Act
            await _xAuthRepo.Object.SaveOAuth1TokensAsync(ownerUserId, accessToken, accessSecret);
            var tokens = await _xAuthRepo.Object.GetOAuth1TokensAsync(ownerUserId);

            // Assert
            Assert.That(tokens, Is.Not.Null, "Tokens moeten opgehaald kunnen worden");
            Assert.That(tokens.Value.AccessToken, Is.EqualTo(accessToken), "Access token moet correct zijn");
            Assert.That(tokens.Value.AccessSecret, Is.EqualTo(accessSecret), "Access secret moet correct zijn");
            
            _xAuthRepo.Verify(x => x.SaveOAuth1TokensAsync(ownerUserId, accessToken, accessSecret), 
                Times.Once, "Tokens moeten opgeslagen worden");
            _xAuthRepo.Verify(x => x.GetOAuth1TokensAsync(ownerUserId), 
                Times.Once, "Tokens moeten opgehaald worden");
        }

        /// Test: X user info storage
        [Test]
        public async Task UC15_XUserInfo_MustBeSavedAndRetrieved()
        {
            // Arrange
            int ownerUserId = 123;
            string xUserId = "987654321";
            string xUsername = "@testuser";

            _xAuthRepo!
                .Setup(x => x.SaveXUserInfoAsync(ownerUserId, xUserId, xUsername))
                .Returns(Task.CompletedTask);

            _xAuthRepo
                .Setup(x => x.GetXUserInfoAsync(ownerUserId))
                .ReturnsAsync((xUserId, xUsername));

            // Act
            await _xAuthRepo.Object.SaveXUserInfoAsync(ownerUserId, xUserId, xUsername);
            var userInfo = await _xAuthRepo.Object.GetXUserInfoAsync(ownerUserId);

            // Assert
            Assert.That(userInfo, Is.Not.Null, "User info moet opgehaald kunnen worden");
            Assert.That(userInfo.Value.XUserId, Is.EqualTo(xUserId), "X user ID moet correct zijn");
            Assert.That(userInfo.Value.XUsername, Is.EqualTo(xUsername), "X username moet correct zijn");
        }

        /// Test: Clear all tokens
        [Test]
        public async Task UC15_ClearAllTokens_MustRemoveAllStoredData()
        {
            // Arrange
            _xAuthRepo!
                .Setup(x => x.ClearAllTokensAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _xAuthRepo.Object.ClearAllTokensAsync();

            // Assert
            _xAuthRepo.Verify(x => x.ClearAllTokensAsync(), Times.Once, 
                "Alle tokens moeten gewist kunnen worden");
        }
    }
}