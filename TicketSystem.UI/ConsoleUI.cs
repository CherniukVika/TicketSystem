using TicketSystem.BLL.Dto;
using TicketSystem.BLL.Services;
using TicketSystem.BLL.Strategy;

namespace TicketSystem.UI
{
    public class ConsoleUI
    {
        private readonly ITheaterService _theaterService;

        public ConsoleUI(ITheaterService theaterService)
        {
            _theaterService = theaterService;
        }

        public async Task Run()
        {
            await DisplayMainMenu();
        }

        private async Task DisplayMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Дата: {DateTime.Now:dd MMMM yyyy}");
                Console.WriteLine("\nАфіша:");

                var performances = await _theaterService.GetAllPerformancesAsync();
                for (int i = 0; i < performances.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {performances[i].Title}");
                }

                Console.Write("\nОберіть виставу: ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= performances.Count)
                {
                    await DisplayPerformanceMenu(performances[choice - 1]);
                }
                else
                {
                    Console.Write("Некоректний вибір! Спробуйте ще раз.");
                    Console.ReadKey();
                }
            }
        }

        private async Task DisplayPerformanceMenu(PerformanceDto performance)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Вистава “{performance.Title}”");
                Console.WriteLine($"Автор: {await _theaterService.GetAuthorNameAsync(performance.Id)}");
                Console.WriteLine($"Жанр: {string.Join(", ", await _theaterService.GetGenreNamesAsync(performance.Id))}");
                Console.WriteLine("\nДоступні дії:");
                Console.WriteLine("1. Придбати квиток");
                Console.WriteLine("2. Повернути квиток");
                Console.WriteLine("0. Повернутись до афіші");

                Console.Write("\nОберіть опцію: ");
                switch (Console.ReadLine())
                {
                    case "1":
                        bool bought = await BuyTicketMenu(performance);
                        if (bought)
                        {
                            return;
                        }
                        break;
                    case "2":
                        await ReturnTicketMenu(performance);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private async Task<bool> BuyTicketMenu(PerformanceDto performance)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Вистава “{performance.Title}”");
                Console.WriteLine("Доступні дати:");

                var schedules = performance.Schedules.ToList();
                for (int i = 0; i < schedules.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {schedules[i].Date:dd MMMM yyyy о HH:mm}");
                }

                Console.WriteLine("0. Назад");
                Console.Write("\nОберіть дату: ");
                string input = Console.ReadLine();

                if (input == "0")
                    return false;

                if (int.TryParse(input, out int dateChoice) && dateChoice > 0 && dateChoice <= schedules.Count)
                {
                    var selectedSchedule = schedules[dateChoice - 1];
                    bool backToDateMenu = await DisplaySeatMenu(performance, selectedSchedule);
                    if (backToDateMenu)
                        continue;
                    else
                        return true;
                }
                else
                {
                    Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
                    Console.ReadKey();
                }
            }
        }

        private async Task<bool> DisplaySeatMenu(PerformanceDto performance, PerformanceScheduleDto schedule)
        {
            string location = null;
            ITicketPricingStrategy pricingStrategy = null;

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Вистава “{performance.Title}”");
                Console.WriteLine($"Дата вистави: {schedule.Date:dd MMMM yyyy о HH:mm}");
                Console.WriteLine("\nДоступні місця:");
                Console.WriteLine("1. Зал");
                Console.WriteLine("2. Балкон");
                Console.WriteLine("0. Назад");

                Console.Write("\nОберіть місце: ");
                string venueChoice = Console.ReadLine();

                if (venueChoice == "1")
                {
                    location = "Hall";
                    pricingStrategy = new HallPricingStrategy();
                    break;
                }
                else if (venueChoice == "2")
                {
                    location = "Balcony";
                    pricingStrategy = new BalconyPricingStrategy();
                    break;
                }
                else if (venueChoice == "0")
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
                    Console.ReadKey();
                }
            }

            int availableSeatsCount = await _theaterService.GetAvailableSeatsCountAsync(performance.Id, schedule.Id, location);
            if (availableSeatsCount == 0)
            {
                Console.WriteLine("Немає вільних місць!");
                Console.ReadKey();
                return false;
            }

            string locationInUkrainian = location == "Hall" ? "залі" : "балконі";
            Console.WriteLine($"\nНа {locationInUkrainian} доступно {availableSeatsCount} місць.");
            Console.WriteLine(pricingStrategy.GetDescription());

            var seats = await _theaterService.GetAvailableSeatsAsync(performance.Id, schedule.Id, location);
            if (!seats.Any())
            {
                Console.WriteLine("Немає вільних місць!");
                Console.ReadKey();
                return false;
            }

            string actionChoice;
            Console.WriteLine("\nДоступні дії:");
            Console.WriteLine("1. Купити");
            Console.WriteLine("0. Назад");

            while (true)
            {
                Console.Write("\nОберіть дію: ");
                actionChoice = Console.ReadLine();

                if (actionChoice == "0")
                {
                    return true;
                }
                else if (actionChoice == "1")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
                }
            }

            string phoneNumber;

            while (true)
            {
                Console.Write("\nВведіть номер телефону (наприклад, 0671234567): ");
                phoneNumber = Console.ReadLine();

                var existingTickets = (await _theaterService.GetTicketsByStatusAsync(TicketSystem.BLL.Dto.TicketStatus.Sold))
                    .Where(t => t.PhoneNumber == phoneNumber &&
                           t.PerformanceId == performance.Id &&
                           t.PerformanceScheduleId == schedule.Id &&
                           !t.IsReturned)
                    .ToList();

                if (existingTickets.Any())
                {
                    Console.WriteLine($"За цим номером вже придбано {existingTickets.Count} квитків.");
                }

                while (true)
                {
                    Console.Write("\nПідтвердження покупки квитка (так/ні): ");
                    string confirmation = Console.ReadLine()?.Trim().ToLower();

                    if (confirmation == "так")
                    {
                        var seat = seats.FirstOrDefault();
                        if (seat == null)
                        {
                            Console.WriteLine("Помилка: немає доступних місць!");
                            Console.ReadKey();
                            return false;
                        }

                        try
                        {
                            var ticket = await _theaterService.BuyTicketAsync(performance.Id, schedule.Id, seat.Id, pricingStrategy, phoneNumber);
                            if (ticket != null)
                            {
                                Console.WriteLine("\nКвиток успішно придбано!");
                                int updatedSeatsCount = await _theaterService.GetAvailableSeatsCountAsync(performance.Id, schedule.Id, location);
                                Console.WriteLine($"Залишилось {updatedSeatsCount} вільних місць на {locationInUkrainian}.");
                            }
                            else
                            {
                                Console.WriteLine("Помилка при покупці квитка!");
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        catch (InvalidOperationException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        Console.ReadKey();
                        return false;
                    }
                    else if (confirmation == "ні")
                    {
                        Console.WriteLine("Покупку скасовано.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
                    }
                }
            }
        }

        private async Task ReturnTicketMenu(PerformanceDto performance)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Вистава “{performance.Title}”");
                Console.WriteLine("Доступні дати:");

                var schedules = performance.Schedules.ToList();

                if (!schedules.Any())
                {
                    Console.WriteLine("Немає дат для повернення квитків!");
                    Console.ReadKey();
                    return;
                }

                for (int i = 0; i < schedules.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {schedules[i].Date:dd MMMM yyyy о HH:mm}");
                }

                Console.WriteLine("0. Назад");
                Console.Write("\nОберіть дату: ");
                string input = Console.ReadLine();

                if (input == "0")
                    return;

                if (int.TryParse(input, out int dateChoice) && dateChoice > 0 && dateChoice <= schedules.Count)
                {
                    var selectedSchedule = schedules[dateChoice - 1];

                    string phoneNumber;
                    while (true)
                    {
                        Console.Write("\nВведіть номер телефону (наприклад, 0671234567): ");
                        phoneNumber = Console.ReadLine();

                        if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Length == 10 && phoneNumber.All(char.IsDigit))
                            break;

                        Console.WriteLine("Некоректно введено номер телефону! Спробуйте ще раз.");
                    }

                    var tickets = (await _theaterService.GetTicketsByStatusAsync(TicketSystem.BLL.Dto.TicketStatus.Sold))
                        .Where(t => t.PerformanceId == performance.Id &&
                               t.PerformanceScheduleId == selectedSchedule.Id &&
                               t.PhoneNumber == phoneNumber)
                        .ToList();

                    if (!tickets.Any())
                    {
                        Console.WriteLine("У вас немає квитків на цю виставу.");
                        Console.ReadKey();
                        return;
                    }

                    if ((selectedSchedule.Date - DateTime.Now).TotalDays < 2)
                    {
                        Console.WriteLine("Повернення неможливе: до вистави менше 2 днів!");
                        Console.ReadKey();
                        return;
                    }

                    Console.WriteLine($"\nУ вас {tickets.Count} куплених квитків на цю виставу.");

                    int quantityToReturn;
                    while (true)
                    {
                        Console.Write("\nСкільки квитків ви хочете повернути: ");
                        string quantityInput = Console.ReadLine();

                        if (int.TryParse(quantityInput, out quantityToReturn) &&
                            quantityToReturn > 0 &&
                            quantityToReturn <= tickets.Count)
                        {
                            break;
                        }

                        Console.WriteLine("Некоректна кількість квитків для повернення. Спробуйте ще раз.");
                    }

                    while (true)
                    {
                        Console.Write("\nЧи дійсно бажаєте повернути квитки? (так/ні): ");
                        string confirmInput = Console.ReadLine().Trim().ToLower();

                        if (confirmInput == "так")
                        {
                            decimal totalRefunded = 0;
                            int successfulReturns = 0;
                            for (int i = 0; i < quantityToReturn; i++)
                            {
                                var ticket = tickets[i];
                                var (success, returnedTicket) = await _theaterService.ReturnTicketAsync(ticket.Id, phoneNumber);
                                if (success && returnedTicket != null)
                                {
                                    totalRefunded += returnedTicket.Price;
                                    successfulReturns++;
                                }
                            }
                            if (successfulReturns > 0)
                            {
                                Console.WriteLine($"\nУспішно повернено {successfulReturns} квитків.");
                                Console.WriteLine($"Загальна сума повернення: {totalRefunded:F2} грн.");
                            }
                            else
                            {
                                Console.WriteLine("\nНе вдалося повернути жоден квиток.");
                            }
                            Console.ReadKey();
                            return;
                        }
                        else if (confirmInput == "ні")
                        {
                            Console.WriteLine("Повернення скасовано.");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
                        }
                    }

                    Console.ReadKey();
                    break;
                }
                else
                {
                    Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
                    Console.ReadKey();
                }
            }
        }
    }
}