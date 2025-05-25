using Moq;
using TicketSystem.BLL.Services;
using TicketSystem.DAL.Models;
using TicketSystem.DAL.UnitOfWork;

namespace TicketSystem.BLL.Tests
{
    [TestFixture]
    public class SeedDataServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private SeedDataService _seedDataService;

        [SetUp]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _seedDataService = new SeedDataService(_unitOfWorkMock.Object);
        }

        [Test]
        public async Task InitializeDataAsync_NoExistingPerformances_AddsData()
        {
            _unitOfWorkMock.Setup(u => u.Performances.GetAllAsync()).ReturnsAsync(new List<Performance>());
            _unitOfWorkMock.Setup(u => u.Authors.AddAsync(It.IsAny<Author>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.Genres.AddAsync(It.IsAny<Genre>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.Performances.AddAsync(It.IsAny<Performance>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await _seedDataService.InitializeDataAsync();

            _unitOfWorkMock.Verify(u => u.Authors.AddAsync(It.IsAny<Author>()), Times.Exactly(3));
            _unitOfWorkMock.Verify(u => u.Genres.AddAsync(It.IsAny<Genre>()), Times.Exactly(3));
            _unitOfWorkMock.Verify(u => u.Performances.AddAsync(It.IsAny<Performance>()), Times.Exactly(3));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once());
        }

        [Test]
        public async Task InitializeDataAsync_ExistingPerformances_SkipsInitialization()
        {
            var performances = new List<Performance> { new Performance { Id = 1 } };
            _unitOfWorkMock.Setup(u => u.Performances.GetAllAsync()).ReturnsAsync(performances);

            await _seedDataService.InitializeDataAsync();

            _unitOfWorkMock.Verify(u => u.Authors.AddAsync(It.IsAny<Author>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.Genres.AddAsync(It.IsAny<Genre>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.Performances.AddAsync(It.IsAny<Performance>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public void InitializeDataAsync_ThrowsException_PropagatesException()
        {
            _unitOfWorkMock.Setup(u => u.Performances.GetAllAsync()).ThrowsAsync(new Exception("Тестова помилка"));

            var ex = Assert.ThrowsAsync<Exception>(() => _seedDataService.InitializeDataAsync());
            Assert.That(ex.Message, Is.EqualTo("Тестова помилка"));
        }
    }
}