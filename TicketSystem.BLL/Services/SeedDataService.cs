using TicketSystem.DAL.Models;
using TicketSystem.DAL.UnitOfWork;

namespace TicketSystem.BLL.Services
{
    public class SeedDataService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SeedDataService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task InitializeDataAsync()
        {
            try
            {
                var performances = await _unitOfWork.Performances.GetAllAsync();
                if (!performances.Any())
                {
                    var author1 = new Author { Name = "Ярослав Стельмах" };
                    var author2 = new Author { Name = "Вільям Шекспір" };
                    var author3 = new Author { Name = "Іван Франко" };

                    var genre1 = new Genre { Name = "Драма" };
                    var genre2 = new Genre { Name = "Трагедія" };
                    var genre3 = new Genre { Name = "Детективна мелодрама" };

                    var schedule1 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 5, 30, 16, 0, 0),
                        Seats = GenerateSeats()
                    };

                    var schedule1_1 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 6, 3, 18, 0, 0),
                        Seats = GenerateSeats()
                    };

                    var schedule2 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 5, 31, 18, 0, 0),
                        Seats = GenerateSeats()
                    };

                    var schedule2_2 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 6, 5, 18, 0, 0),
                        Seats = GenerateSeats()
                    };

                    var schedule3 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 6, 2, 18, 0, 0),
                        Seats = GenerateSeats()
                    };

                    var schedule3_3 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 6, 7, 16, 0, 0),
                        Seats = GenerateSeats()
                    };

                    var performance1 = new Performance
                    {
                        Title = "Синій автомобіль",
                        Author = author1,
                        Genres = new List<Genre> { genre1 },
                        Schedules = new List<PerformanceSchedule> { schedule1, schedule1_1 }
                    };

                    var performance2 = new Performance
                    {
                        Title = "Макбет",
                        Author = author2,
                        Genres = new List<Genre> { genre2 },
                        Schedules = new List<PerformanceSchedule> { schedule2, schedule2_2 }
                    };

                    var performance3 = new Performance
                    {
                        Title = "Сойка",
                        Author = author3,
                        Genres = new List<Genre> { genre3 },
                        Schedules = new List<PerformanceSchedule> { schedule3, schedule3_3 }
                    };

                    await _unitOfWork.Authors.AddAsync(author1);
                    await _unitOfWork.Authors.AddAsync(author2);
                    await _unitOfWork.Authors.AddAsync(author3);

                    await _unitOfWork.Genres.AddAsync(genre1);
                    await _unitOfWork.Genres.AddAsync(genre2);
                    await _unitOfWork.Genres.AddAsync(genre3);

                    await _unitOfWork.Performances.AddAsync(performance1);
                    await _unitOfWork.Performances.AddAsync(performance2);
                    await _unitOfWork.Performances.AddAsync(performance3);

                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при ініціалізації даних: {ex.Message}");
                throw;
            }
        }

        private List<Seat> GenerateSeats()
        {
            var seats = new List<Seat>();
            for (int i = 1; i <= 50; i++)
            {
                seats.Add(new Seat { Number = i, Location = "Hall" });
            }
            for (int i = 1; i <= 30; i++)
            {
                seats.Add(new Seat { Number = i, Location = "Balcony" });
            }
            return seats;
        }
    }
}
