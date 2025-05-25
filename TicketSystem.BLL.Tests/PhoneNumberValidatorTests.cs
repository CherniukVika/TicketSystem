using TicketSystem.BLL.Services;

namespace TicketSystem.BLL.Tests
{
    [TestFixture]
    public class PhoneNumberValidatorTests
    {
        [Test]
        public void Validate_ValidPhoneNumber_DoesNotThrow()
        {
            string phoneNumber = "0671234567";

            Assert.DoesNotThrow(() => PhoneNumberValidator.Validate(phoneNumber));
        }

        [TestCase("123")]
        [TestCase("067123456a")]
        [TestCase("")]
        [TestCase("06712345678")]
        public void Validate_InvalidPhoneNumber_ThrowsArgumentException(string phoneNumber)
        {
            var ex = Assert.Throws<ArgumentException>(() => PhoneNumberValidator.Validate(phoneNumber));
            Assert.That(ex.Message, Is.EqualTo("Некоректний номер телефону. Формат: 0671234567"));
        }
    }
}