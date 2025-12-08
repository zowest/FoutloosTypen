using FoutloosTypen.Core.Helpers;
using NUnit.Framework;

namespace TestCore
{
    public class TestHashing
    {
        [SetUp]
        public void Setup()
        {
        }

        // UT17-01: Correct wachtwoord matcht hash
        [Test]
        public void TestPasswordHashReturnsTrue()
        {
            string password = "boltype";
            string passwordHash = "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s=";
            Assert.That(PasswordHelper.VerifyPassword(password, passwordHash), Is.True);
        }

        // UT17-02: Correct wachtwoord + testcase matcht hash
        [TestCase("boltype", "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s=")]
        public void TestPasswordHashReturnsTrueWithTestCase(string password, string passwordHash)
        {
            Assert.That(PasswordHelper.VerifyPassword(password, passwordHash), Is.True);
        }

        // UT17-03: Verkeerd wachtwoord faalt
        [Test]
        public void TestPasswordHashReturnsFalse()
        {
            string wrongPassword = "verkeerdeWachtwoord";
            string correctHash = "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s=";
            Assert.That(PasswordHelper.VerifyPassword(wrongPassword, correctHash), Is.False);
        }

        // UT17-04: Ongeldige hash gooit exception
        [TestCase("boltype", "bHbXpFGYmI/YjrZvIVvu0Q==.kf5UUcO9kF5t9hSplOjbDUX2u2vle52Y4FHj4cFgE+s")]
        public void TestPasswordHashThrowsOnInvalidHash(string password, string passwordHash)
        {
            Assert.Throws<FormatException>(() =>
                PasswordHelper.VerifyPassword(password, passwordHash));
        }
    }
}
