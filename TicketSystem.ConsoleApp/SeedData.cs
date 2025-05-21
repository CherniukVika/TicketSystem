using TicketSystem.Data;

namespace TicketSystem.ConsoleApp
{
    public class SeedData
    {
        public void InitializeData(TicketSystemContext context)
        {
            try
            {
                if (!context.Performances.Any())
                {
                    var author1 = new Author { Name = "Ярослав Стельмах" };
                    var author2 = new Author { Name = "Вільям Шекспір" };
                    var author3 = new Author { Name = "Іван Франко" };

                    var genre1 = new Genre { Name = "Драма" };
                    var genre2 = new Genre { Name = "Трагедія" };
                    var genre3 = new Genre { Name = "Детективна мелодрама" };

                    var schedule1 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 5, 22, 16, 0, 0),
                        Seats = new List<Seat>()
                    };
                    for (int i = 1; i <= 50; i++)
                    {
                        schedule1.Seats.Add(new Seat { Number = i, Location = "Hall" });
                    }
                    for (int i = 1; i <= 30; i++)
                    {
                        schedule1.Seats.Add(new Seat { Number = i, Location = "Balcony" });
                    }

                    var schedule1_1 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 5, 24, 18, 0, 0),
                        Seats = new List<Seat>()
                    };
                    for (int i = 1; i <= 50; i++)
                    {
                        schedule1_1.Seats.Add(new Seat { Number = i, Location = "Hall" });
                    }
                    for (int i = 1; i <= 30; i++)
                    {
                        schedule1_1.Seats.Add(new Seat { Number = i, Location = "Balcony" });
                    }


                    var schedule2 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 5, 24, 18, 0, 0),
                        Seats = new List<Seat>()
                    };
                    for (int i = 1; i <= 50; i++)
                    {
                        schedule2.Seats.Add(new Seat { Number = i, Location = "Hall" });
                    }
                    for (int i = 1; i <= 30; i++)
                    {
                        schedule2.Seats.Add(new Seat { Number = i, Location = "Balcony" });
                    }

                    var schedule2_2 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 5, 26, 18, 0, 0),
                        Seats = new List<Seat>()
                    };
                    for (int i = 1; i <= 50; i++)
                    {
                        schedule2_2.Seats.Add(new Seat { Number = i, Location = "Hall" });
                    }
                    for (int i = 1; i <= 30; i++)
                    {
                        schedule2_2.Seats.Add(new Seat { Number = i, Location = "Balcony" });
                    }

                    
                    var schedule3 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 5, 26, 18, 0, 0),
                        Seats = new List<Seat>()
                    };
                    for (int i = 1; i <= 50; i++)
                    {
                        schedule3.Seats.Add(new Seat { Number = i, Location = "Hall" });
                    }
                    for (int i = 1; i <= 30; i++)
                    {
                        schedule3.Seats.Add(new Seat { Number = i, Location = "Balcony" });
                    }

                    var schedule3_3 = new PerformanceSchedule
                    {
                        Date = new DateTime(2025, 5, 30, 16, 0, 0),
                        Seats = new List<Seat>()
                    };
                    for (int i = 1; i <= 50; i++)
                    {
                        schedule3_3.Seats.Add(new Seat { Number = i, Location = "Hall" });
                    }
                    for (int i = 1; i <= 30; i++)
                    {
                        schedule3_3.Seats.Add(new Seat { Number = i, Location = "Balcony" });
                    }


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
                        Schedules = new List<PerformanceSchedule> { schedule3, schedule3_3}
                    };

                    context.Authors.AddRange(author1, author2, author3);
                    context.Genres.AddRange(genre1, genre2, genre3);
                    context.Performances.AddRange(performance1, performance2, performance3);

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при ініціалізації даних: {ex.Message}");
                throw;
            }
        }
    }
}
