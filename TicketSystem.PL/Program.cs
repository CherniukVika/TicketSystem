using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TicketSystem.BLL.Mapper;
using TicketSystem.BLL.Services;
using TicketSystem.DAL;
using TicketSystem.DAL.UnitOfWork;
using TicketSystem.PL.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TicketSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseLazyLoadingProxies());

builder.Services.AddAutoMapper(typeof(AutoMapperProfile), typeof(PresentationMappingProfile));

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterType<TheaterService>().As<ITheaterService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<SeedDataService>().InstancePerLifetimeScope();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var seedService = scope.ServiceProvider.GetRequiredService<SeedDataService>();
        await seedService.InitializeDataAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during data initialization: {ex.Message}\n{ex.StackTrace}");
        throw;
    }
}

app.Run();
