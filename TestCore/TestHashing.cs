using FoutloosTypen.Core.Helpers;

namespace TestCore
{
    public class TestHashing
    {
        [SetUp]
        public void Setup()
        {
        }

        // Happy flow
        [Test]
        public void TestPasswordHashReturnsTrue()
        {
            string password = "boltype";
            string passwordHash = "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s=";
            Assert.IsTrue(PasswordHelper.VerifyPassword(password, passwordHash));
        }

        [TestCase("boltype", "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s=")]
        public void TestPasswordHashReturnsTrue(string password, string passwordHash)
        {
            Assert.IsTrue(PasswordHelper.VerifyPassword(password, passwordHash));
        }

        // Unhappy flow
        [Test]
        public void TestPasswordHashReturnsFalse()
        {
            // correct hash maar verkeerd wachtwoord
            string wrongPassword = "verkeerdeWachtwoord";
            string correctHash = "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s=";
            Assert.IsFalse(PasswordHelper.VerifyPassword(wrongPassword, correctHash));
        }

        [TestCase("boltype", "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s")]
        public void TestPasswordHashThrowsOnInvalidHash(string password, string passwordHash)
        {
            Assert.Throws<FormatException>(() =>
                PasswordHelper.VerifyPassword(password, passwordHash));
        }
    }
}