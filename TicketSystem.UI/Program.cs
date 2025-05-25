using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketSystem.BLL.Mapper;
using TicketSystem.BLL.Services;
using TicketSystem.DAL;
using TicketSystem.DAL.UnitOfWork;

namespace TicketSystem.UI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            var serviceProvider = ConfigureServices();

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<TicketSystemContext>();
                var seedService = scope.ServiceProvider.GetService<SeedDataService>();
                var ui = scope.ServiceProvider.GetService<ConsoleUI>();

                await context.Database.MigrateAsync();
                await seedService.InitializeDataAsync();
                await ui.Run(); 
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddDbContext<TicketSystemContext>(options =>
                options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TicketSystem;Trusted_Connection=True;"));

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITheaterService, TheaterService>();
            services.AddScoped<SeedDataService>();
            services.AddScoped<ConsoleUI>();

            return services.BuildServiceProvider();
        }
    }
}