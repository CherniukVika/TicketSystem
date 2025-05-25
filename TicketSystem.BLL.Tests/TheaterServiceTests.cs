using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NUnit.Framework;
using TicketSystem.BLL.Dto;
using TicketSystem.BLL.Services;
using TicketSystem.BLL.Strategy;
using TicketSystem.DAL.Models;
using TicketSystem.DAL.UnitOfWork;

namespace TicketSystem.BLL.Tests
{
    [TestFixture]
    public class TheaterServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IMapper> _mapperMock;
        private TheaterService _theaterService;

        [SetUp]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _theaterService = new TheaterService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task GetAuthorNameAsync_PerformanceExists_ReturnsAuthorName()
        {
            int performanceId = 1;
            var performance = new Performance { Id = performanceId, Author = new Author { Name = "Ярослав Стельмах" } };
            _unitOfWorkMock.Setup(u => u.Performances.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Performance, bool>>>(),
                It.IsAny<Func<IQueryable<Performance>, IQueryable<Performance>>>()))
                .ReturnsAsync(performance);

            var result = await _theaterService.GetAuthorNameAsync(performanceId);

            Assert.That(result, Is.EqualTo("Ярослав Стельмах"));
        }

        [Test]
        public async Task GetAuthorNameAsync_PerformanceNotFound_ReturnsUnknownAuthor()
        {
            int performanceId = 1;
            _unitOfWorkMock.Setup(u => u.Performances.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Performance, bool>>>(),
                It.IsAny<Func<IQueryable<Performance>, IQueryable<Performance>>>()))
                .ReturnsAsync((Performance)null);

            var result = await _theaterService.GetAuthorNameAsync(performanceId);

            Assert.That(result, Is.EqualTo("Невідомий автор"));
        }

        [Test]
        public async Task GetGenreNamesAsync_PerformanceExists_ReturnsGenreNames()
        {
            int performanceId = 1;
            var performance = new Performance
            {
                Id = performanceId,
                Genres = new List<Genre> { new Genre { Name = "Драма" }, new Genre { Name = "Трагедія" } }
            };
            _unitOfWorkMock.Setup(u => u.Performances.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Performance, bool>>>(),
                It.IsAny<Func<IQueryable<Performance>, IQueryable<Performance>>>()))
                .ReturnsAsync(performance);

            var result = await _theaterService.GetGenreNamesAsync(performanceId);

            Assert.That(result, Is.EquivalentTo(new List<string> { "Драма", "Трагедія" }));
        }

        [Test]
        public async Task GetGenreNamesAsync_PerformanceNotFound_ReturnsEmptyList()
        {
            int performanceId = 1;
            _unitOfWorkMock.Setup(u => u.Performances.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Performance, bool>>>(),
                It.IsAny<Func<IQueryable<Performance>, IQueryable<Performance>>>()))
                .ReturnsAsync((Performance)null);

            var result = await _theaterService.GetGenreNamesAsync(performanceId);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllPerformancesAsync_ReturnsMappedPerformances()
        {
            var performances = new List<Performance>
        {
            new Performance { Id = 1, Title = "Синій автомобіль", Author = new Author { Name = "Ярослав Стельмах" } }
        };
                var performanceDtos = new List<PerformanceDto>
        {
            new PerformanceDto { Id = 1, Title = "Синій автомобіль", Author = new AuthorDto { Name = "Ярослав Стельмах" } }
        };

            _unitOfWorkMock.Setup(u => u.Performances.GetWithIncludeAsync(
                It.IsAny<Func<IQueryable<Performance>, IQueryable<Performance>>[]>()
            )).ReturnsAsync(performances);

            _mapperMock.Setup(m => m.Map<List<PerformanceDto>>(performances)).Returns(performanceDtos);

            var result = await _theaterService.GetAllPerformancesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Синій автомобіль"));
        }


        [Test]
        public async Task GetAvailableSeatsAsync_ValidInput_ReturnsAvailableSeats()
        {
            int performanceId = 1;
            int scheduleId = 1;
            string location = "Hall";
            var seats = new List<Seat>
            {
                new Seat { Id = 1, Number = 1, Location = "Hall", PerformanceScheduleId = scheduleId, Ticket = null }
            };
            var seatDtos = new List<SeatDto>
            {
                new SeatDto { Id = 1, Number = 1, Location = "Hall", PerformanceScheduleId = scheduleId }
            };
            _unitOfWorkMock.Setup(u => u.Seats.FindAsync(
             It.IsAny<Expression<Func<Seat, bool>>>(),
             It.IsAny<Expression<Func<Seat, object>>>(),
             It.IsAny<Expression<Func<Seat, object>>>()))
             .ReturnsAsync(seats);

            _mapperMock.Setup(m => m.Map<List<SeatDto>>(seats)).Returns(seatDtos);

            var result = await _theaterService.GetAvailableSeatsAsync(performanceId, scheduleId, location);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Location, Is.EqualTo(location));
        }

        [Test]
        public async Task GetAvailableSeatsCountAsync_ValidInput_ReturnsCount()
        {
            int performanceId = 1;
            int scheduleId = 1;
            string location = "Hall";
            var seats = new List<Seat>
            {
                new Seat { Id = 1, Number = 1, Location = "Hall", PerformanceScheduleId = scheduleId, Ticket = null },
                new Seat { Id = 2, Number = 2, Location = "Hall", PerformanceScheduleId = scheduleId, Ticket = new Ticket { Status = TicketSystem.DAL.TicketStatus.Returned } }
            };
            _unitOfWorkMock.Setup(u => u.Seats.FindAsync(
                 It.IsAny<Expression<Func<Seat, bool>>>(),
                 It.IsAny<Expression<Func<Seat, object>>[]>()))
                 .ReturnsAsync(seats);


            var result = await _theaterService.GetAvailableSeatsCountAsync(performanceId, scheduleId, location);

            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public async Task BuyTicketAsync_ValidInput_ReturnsTicketDto()
        {
            int performanceId = 1;
            int scheduleId = 1;
            int seatId = 1;
            string phoneNumber = "0671234567";

            var pricingStrategy = new Mock<ITicketPricingStrategy>();
            pricingStrategy.Setup(p => p.CalculatePrice()).Returns(300m);

            var schedule = new PerformanceSchedule { Id = scheduleId, PerformanceId = performanceId };
            var performance = new Performance { Id = performanceId, Tickets = new List<Ticket>() };
            var seat = new Seat
            {
                Id = seatId,
                PerformanceScheduleId = scheduleId,
                Ticket = null,
                PerformanceSchedule = new PerformanceSchedule { Id = scheduleId, PerformanceId = performanceId }
            };
            var ticket = new Ticket
            {
                Id = 1,
                PerformanceId = performanceId,
                PerformanceScheduleId = scheduleId,
                SeatId = seatId,
                Status = TicketSystem.DAL.TicketStatus.Sold,
                Price = 300m,
                PurchaseDate = DateTime.Now,
                PhoneNumber = phoneNumber
            };
            var ticketDto = new TicketDto
            {
                Id = 1,
                PerformanceId = performanceId,
                PerformanceScheduleId = scheduleId,
                SeatId = seatId,
                Status = TicketSystem.BLL.Dto.TicketStatus.Sold,
                Price = 300m,
                PurchaseDate = ticket.PurchaseDate,
                PhoneNumber = phoneNumber
            };

            _unitOfWorkMock.Setup(u => u.PerformanceSchedules.FirstOrDefaultAsync(
                 It.IsAny<Expression<Func<PerformanceSchedule, bool>>>(),
                 It.IsAny<Func<IQueryable<PerformanceSchedule>, IQueryable<PerformanceSchedule>>>()))
                 .ReturnsAsync(schedule);

            _unitOfWorkMock.Setup(u => u.Performances.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Performance, bool>>>(),
                It.IsAny<Func<IQueryable<Performance>, IQueryable<Performance>>>()))
                .ReturnsAsync(performance);

            _unitOfWorkMock.Setup(u => u.Seats.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Seat, bool>>>(),
                It.IsAny<Func<IQueryable<Seat>, IQueryable<Seat>>>()))
                .ReturnsAsync(seat);


            _unitOfWorkMock.Setup(u => u.Tickets.AddAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.PerformanceSchedules.FirstOrDefaultAsync(It.IsAny<Expression<Func<PerformanceSchedule, bool>>>()))
                .ReturnsAsync(schedule);

            _unitOfWorkMock.Setup(u => u.Performances.FirstOrDefaultAsync(It.IsAny<Expression<Func<Performance, bool>>>()))
                .ReturnsAsync(performance);

            _unitOfWorkMock.Setup(u => u.Seats.FirstOrDefaultAsync(It.IsAny<Expression<Func<Seat, bool>>>()))
                .ReturnsAsync(seat);

            _mapperMock.Setup(m => m.Map<TicketDto>(It.IsAny<Ticket>())).Returns(ticketDto);

            var result = await _theaterService.BuyTicketAsync(performanceId, scheduleId, seatId, pricingStrategy.Object, phoneNumber);

            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.Status, Is.EqualTo(TicketSystem.BLL.Dto.TicketStatus.Sold), "Ticket status should be Sold");
            Assert.That(result.Price, Is.EqualTo(300m), "Ticket price should be 300");
            Assert.That(result.PhoneNumber, Is.EqualTo(phoneNumber), "Phone number should match");
        }



        [Test]
        public async Task BuyTicketAsync_SeatAlreadySold_ReturnsNull()
        {
            int performanceId = 1;
            int scheduleId = 1;
            int seatId = 1;
            string phoneNumber = "0671234567";
            var pricingStrategy = new Mock<ITicketPricingStrategy>();
            var schedule = new PerformanceSchedule { Id = scheduleId, PerformanceId = performanceId };
            var performance = new Performance { Id = performanceId };
            var seat = new Seat
            {
                Id = seatId,
                PerformanceScheduleId = scheduleId,
                Ticket = new Ticket { PerformanceId = performanceId, PerformanceScheduleId = scheduleId, Status = TicketSystem.DAL.TicketStatus.Sold }
            };

            _unitOfWorkMock.Setup(u => u.PerformanceSchedules.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PerformanceSchedule, bool>>>(),
                It.IsAny<Func<IQueryable<PerformanceSchedule>, IQueryable<PerformanceSchedule>>>()))
                .ReturnsAsync(schedule);
            _unitOfWorkMock.Setup(u => u.Performances.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Performance, bool>>>(),
                It.IsAny<Func<IQueryable<Performance>, IQueryable<Performance>>>()))
                .ReturnsAsync(performance);
            _unitOfWorkMock.Setup(u => u.Seats.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Seat, bool>>>()))
                .ReturnsAsync(seat);

            var result = await _theaterService.BuyTicketAsync(performanceId, scheduleId, seatId, pricingStrategy.Object, phoneNumber);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task BuyTicketAsync_NonExistentPerformance_ReturnsNull()
        {
            int performanceId = 1;
            int scheduleId = 1;
            int seatId = 1;
            string phoneNumber = "0671234567";
            var pricingStrategy = new Mock<ITicketPricingStrategy>();
            var schedule = new PerformanceSchedule { Id = scheduleId, PerformanceId = performanceId };

            _unitOfWorkMock.Setup(u => u.PerformanceSchedules.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PerformanceSchedule, bool>>>(),
                It.IsAny<Func<IQueryable<PerformanceSchedule>, IQueryable<PerformanceSchedule>>>()))
                .ReturnsAsync(schedule);
            _unitOfWorkMock.Setup(u => u.Performances.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Performance, bool>>>(),
                It.IsAny<Func<IQueryable<Performance>, IQueryable<Performance>>>()))
                .ReturnsAsync((Performance)null);

            var result = await _theaterService.BuyTicketAsync(performanceId, scheduleId, seatId, pricingStrategy.Object, phoneNumber);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task BuyTicketAsync_NonExistentSchedule_ReturnsNull()
        {
            int performanceId = 1;
            int scheduleId = 1;
            int seatId = 1;
            string phoneNumber = "0671234567";
            var pricingStrategy = new Mock<ITicketPricingStrategy>();

            _unitOfWorkMock.Setup(u => u.PerformanceSchedules.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PerformanceSchedule, bool>>>(),
                It.IsAny<Func<IQueryable<PerformanceSchedule>, IQueryable<PerformanceSchedule>>>()))
                .ReturnsAsync((PerformanceSchedule)null);

            var result = await _theaterService.BuyTicketAsync(performanceId, scheduleId, seatId, pricingStrategy.Object, phoneNumber);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task BuyTicketAsync_NonExistentSeat_ReturnsNull()
        {
            int performanceId = 1;
            int scheduleId = 1;
            int seatId = 1;
            string phoneNumber = "0671234567";
            var pricingStrategy = new Mock<ITicketPricingStrategy>();
            var schedule = new PerformanceSchedule { Id = scheduleId, PerformanceId = performanceId };
            var performance = new Performance { Id = performanceId };

            _unitOfWorkMock.Setup(u => u.PerformanceSchedules.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<PerformanceSchedule, bool>>>(),
                It.IsAny<Func<IQueryable<PerformanceSchedule>, IQueryable<PerformanceSchedule>>>()))
                .ReturnsAsync(schedule);
            _unitOfWorkMock.Setup(u => u.Performances.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Performance, bool>>>(),
                It.IsAny<Func<IQueryable<Performance>, IQueryable<Performance>>>()))
                .ReturnsAsync(performance);
            _unitOfWorkMock.Setup(u => u.Seats.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Seat, bool>>>()))
                .ReturnsAsync((Seat)null);

            var result = await _theaterService.BuyTicketAsync(performanceId, scheduleId, seatId, pricingStrategy.Object, phoneNumber);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ReturnTicketAsync_ValidTicketAndPhone_ReturnsSuccess()
        {
            int ticketId = 1;
            string phoneNumber = "0671234567";

            var scheduleDate = DateTime.Now.AddDays(5);

            var ticket = new Ticket
            {
                Id = ticketId,
                PhoneNumber = phoneNumber,
                Status = TicketSystem.DAL.TicketStatus.Sold,
                IsReturned = false,
                Price = 300m,
                PerformanceSchedule = new PerformanceSchedule { Date = scheduleDate }
            };

            var ticketDto = new TicketDto
            {
                Id = ticketId,
                PhoneNumber = phoneNumber,
                Status = TicketSystem.BLL.Dto.TicketStatus.Returned,
                IsReturned = true,
                Price = 240m
            };

            _unitOfWorkMock.Setup(u => u.Tickets.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                It.IsAny<Func<IQueryable<Ticket>, IQueryable<Ticket>>>(),
                It.IsAny<Func<IQueryable<Ticket>, IQueryable<Ticket>>>()))
                .ReturnsAsync(ticket);

            _unitOfWorkMock.Setup(u => u.Tickets.Update(It.IsAny<Ticket>())).Verifiable();
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<TicketDto>(It.IsAny<Ticket>())).Returns(ticketDto);

            var (success, result) = await _theaterService.ReturnTicketAsync(ticketId, phoneNumber);

            Assert.That(success, Is.True);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(TicketSystem.BLL.Dto.TicketStatus.Returned));
            Assert.That(result.Price, Is.EqualTo(240m));
        }


        [Test]
        public async Task ReturnTicketAsync_TicketWithinTwoDays_ReturnsFailure()
        {
            int ticketId = 1;
            string phoneNumber = "0671234567";
            var ticket = new Ticket
            {
                Id = ticketId,
                PhoneNumber = phoneNumber,
                Status = TicketSystem.DAL.TicketStatus.Sold,
                IsReturned = false,
                PerformanceSchedule = new PerformanceSchedule { Date = DateTime.Now.AddDays(1) }
            };

            _unitOfWorkMock.Setup(u => u.Tickets.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>()))
                .ReturnsAsync(ticket);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            var (success, result) = await _theaterService.ReturnTicketAsync(ticketId, phoneNumber);

            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ReturnTicketAsync_AlreadyReturnedTicket_ReturnsFailure()
        {
            int ticketId = 1;
            string phoneNumber = "0671234567";
            var ticket = new Ticket
            {
                Id = ticketId,
                PhoneNumber = phoneNumber,
                Status = TicketSystem.DAL.TicketStatus.Returned,
                IsReturned = true,
                PerformanceSchedule = new PerformanceSchedule { Date = DateTime.Now.AddDays(3) }
            };

            _unitOfWorkMock.Setup(u => u.Tickets.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>()))
                .ReturnsAsync(ticket);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            var (success, result) = await _theaterService.ReturnTicketAsync(ticketId, phoneNumber);

            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ReturnTicketAsync_NonExistentTicket_ReturnsFailure()
        {
            int ticketId = 1;
            string phoneNumber = "0671234567";

            _unitOfWorkMock.Setup(u => u.Tickets.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>()))
                .ReturnsAsync((Ticket)null);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            var (success, result) = await _theaterService.ReturnTicketAsync(ticketId, phoneNumber);

            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ReturnTicketAsync_IncorrectPhoneNumber_ReturnsFailure()
        {
            int ticketId = 1;
            string correctPhoneNumber = "0671234567";
            string incorrectPhoneNumber = "0501234567";
            var ticket = new Ticket
            {
                Id = ticketId,
                PhoneNumber = correctPhoneNumber,
                Status = TicketSystem.DAL.TicketStatus.Sold,
                IsReturned = false,
                PerformanceSchedule = new PerformanceSchedule { Date = DateTime.Now.AddDays(3) }
            };

            _unitOfWorkMock.Setup(u => u.Tickets.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>()))
                .ReturnsAsync(ticket);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            var (success, result) = await _theaterService.ReturnTicketAsync(ticketId, incorrectPhoneNumber);

            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetTicketsByStatusAsync_ValidStatus_ReturnsTickets()
        {
            var status = TicketSystem.BLL.Dto.TicketStatus.Sold;
            var dalStatus = TicketSystem.DAL.TicketStatus.Sold;
            var tickets = new List<Ticket>
            {
                new Ticket { Id = 1, Status = dalStatus, Performance = new Performance(), Seat = new Seat() }
            };
            var ticketDtos = new List<TicketDto>
            {
                new TicketDto { Id = 1, Status = status }
            };
            _unitOfWorkMock.Setup(u => u.Tickets.FindAsync(
                 It.IsAny<Expression<Func<Ticket, bool>>>(),
                 It.IsAny<Expression<Func<Ticket, object>>[]>()))
                 .ReturnsAsync(tickets);

            _mapperMock.Setup(m => m.Map<List<TicketDto>>(tickets)).Returns(ticketDtos);

            var result = await _theaterService.GetTicketsByStatusAsync(status);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Status, Is.EqualTo(status));
        }

        [Test]
        public async Task GetTicketsByStatusAsync_NoTickets_ReturnsEmptyList()
        {
            var status = TicketSystem.BLL.Dto.TicketStatus.Sold;
            var tickets = new List<Ticket>();
            var ticketDtos = new List<TicketDto>();
            _unitOfWorkMock.Setup(u => u.Tickets.FindAsync(
                It.IsAny<Expression<Func<Ticket, bool>>>()))
                .ReturnsAsync(tickets);
            _mapperMock.Setup(m => m.Map<List<TicketDto>>(tickets)).Returns(ticketDtos);

            var result = await _theaterService.GetTicketsByStatusAsync(status);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetTicketsByStatusAsync_InvalidStatus_ThrowsArgumentException()
        {
            var invalidStatus = (TicketSystem.BLL.Dto.TicketStatus)999;

            Assert.ThrowsAsync<ArgumentException>(() => _theaterService.GetTicketsByStatusAsync(invalidStatus));
        }

        [Test]
        public void BuyTicketAsync_InvalidPhoneNumber_ThrowsArgumentException()
        {
            int performanceId = 1;
            int scheduleId = 1;
            int seatId = 1;
            string invalidPhoneNumber = "123";
            var pricingStrategy = new Mock<ITicketPricingStrategy>();

            Assert.ThrowsAsync<ArgumentException>(() => _theaterService.BuyTicketAsync(performanceId, scheduleId, seatId, pricingStrategy.Object, invalidPhoneNumber));
        }
    }
}