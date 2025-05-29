using TicketSystem.BLL.Services;

namespace TicketSystem.BLL.Tests
{
    [TestFixture]
    public class PhoneNumberValidatorTests
    {
        [Test]
        public void Validate_ValidPhoneNumber_DoesNotThrow()
        {
            string validPhoneNumber = "0671234567";

            Assert.DoesNotThrow(() => PhoneNumberValidator.Validate(validPhoneNumber),
                "Коректний номер телефону не повинен викликати виняток.");
        }

        [Test]
        public void Validate_InvalidLength_ThrowsArgumentException()
        {
            string invalidPhoneNumber = "067123";

            var ex = Assert.Throws<ArgumentException>(() => PhoneNumberValidator.Validate(invalidPhoneNumber),
                "Номер телефону з некоректною довжиною повинен викликати ArgumentException.");
            Assert.That(ex.Message, Is.EqualTo("Некоректний номер телефону. Формат: 0671234567"),
                "Повідомлення про помилку повинно відповідати очікуваному формату.");
        }

        [Test]
        public void Validate_NonDigitCharacters_ThrowsArgumentException()
        {
            string invalidPhoneNumber = "06712345ab";

            var ex = Assert.Throws<ArgumentException>(() => PhoneNumberValidator.Validate(invalidPhoneNumber),
                "Номер телефону з нецифровими символами повинен викликати ArgumentException.");
            Assert.That(ex.Message, Is.EqualTo("Некоректний номер телефону. Формат: 0671234567"),
                "Повідомлення про помилку повинно відповідати очікуваному формату.");
        }

        [Test]
        public void Validate_NullPhoneNumber_ThrowsArgumentException()
        {
            string nullPhoneNumber = null;

            var ex = Assert.Throws<ArgumentException>(() => PhoneNumberValidator.Validate(nullPhoneNumber),
                "Null номер телефону повинен викликати ArgumentException.");
            Assert.That(ex.Message, Is.EqualTo("Некоректний номер телефону. Формат: 0671234567"),
                "Повідомлення про помилку повинно відповідати очікуваному формату.");
        }

        [Test]
        public void Validate_EmptyPhoneNumber_ThrowsArgumentException()
        {
            string emptyPhoneNumber = "";

            var ex = Assert.Throws<ArgumentException>(() => PhoneNumberValidator.Validate(emptyPhoneNumber),
                "Порожній номер телефону повинен викликати ArgumentException.");
            Assert.That(ex.Message, Is.EqualTo("Некоректний номер телефону. Формат: 0671234567"),
                "Повідомлення про помилку повинно відповідати очікуваному формату.");
        }

        [Test]
        public void Validate_WhitespacePhoneNumber_ThrowsArgumentException()
        {
            string whitespacePhoneNumber = "   ";

            var ex = Assert.Throws<ArgumentException>(() => PhoneNumberValidator.Validate(whitespacePhoneNumber),
                "Номер телефону з пробілами повинен викликати ArgumentException.");
            Assert.That(ex.Message, Is.EqualTo("Некоректний номер телефону. Формат: 0671234567"),
                "Повідомлення про помилку повинно відповідати очікуваному формату.");
        }
    }
}
