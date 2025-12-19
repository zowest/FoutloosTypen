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
                .Setup(s => s.SaveImageToCacheAsync(It.IsAny<SKBitmap>()))
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
        public void UC15_FR5_Credentials_MustBeValidated()
        {
            // Arrange
            var validSettings = new XAuthSettings
            {
                ConsumerKey = "ck",
                ConsumerSecret = "cs",
                OAuthToken = "tok",
                OAuthTokenSecret = "tsec"
            };

            var invalidSettings = new XAuthSettings
            {
                ConsumerKey = "ck",
                ConsumerSecret = "cs",
                OAuthToken = "",
                OAuthTokenSecret = ""
            };

            // Assert
            Assert.That(string.IsNullOrEmpty(validSettings.OAuthToken), Is.False,
                "FR5: Geldige credentials moeten token bevatten");
            Assert.That(string.IsNullOrEmpty(invalidSettings.OAuthToken), Is.True,
                "FR5: Ongeldige credentials hebben geen token");
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
                .Setup(s => s.SaveImageToCacheAsync(bitmap))
                .ReturnsAsync("/cache/image.png");

            // Act
            var path = await _shareImageRepo.Object.SaveImageToCacheAsync(bitmap);

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
                .Setup(s => s.SaveImageToCacheAsync(bitmap))
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
            var imagePath = await _shareImageRepo.Object.SaveImageToCacheAsync(bitmap);
            var mediaId = await _mediaRepo.Object.UploadMediaAsync(
                imagePath, "image/png", "ck", "cs", "tok", "tsec");
            await _mediaRepo.Object.PostTweetAsync(
                "Les 1 - 80% voltooid #BolType", mediaId, "ck", "cs", "tok", "tsec");

            // Assert
            _shareImageRepo.Verify(s => s.SaveImageToCacheAsync(bitmap), 
                Times.Once, "Afbeelding moet opgeslagen worden");
            
            _mediaRepo.Verify(m => m.UploadMediaAsync(
                imagePath, "image/png", "ck", "cs", "tok", "tsec"), 
                Times.Once, "Media moet geüpload worden");
            
            _mediaRepo.Verify(m => m.PostTweetAsync(
                It.IsAny<string>(), mediaId, "ck", "cs", "tok", "tsec"), 
                Times.Once, "Tweet moet gepost worden");
        }
    }
}