using Microsoft.EntityFrameworkCore;
using TicketSystem.Data;

namespace TicketSystem.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            SeedData seed = new SeedData();

            var optionsBuilder = new DbContextOptionsBuilder<TicketSystemContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TicketSystem;Trusted_Connection=True;");

            using var context = new TicketSystemContext(optionsBuilder.Options);
            var service = new TheaterService(context);
            var ui = new ConsoleUI(service, context);

            context.Database.Migrate();

            seed.InitializeData(context);

            ui.Run();
        }
    }
}
