namespace TicketSystem.ConsoleApp.Strategy
{
    public interface ITicketPricingStrategy
    {
        decimal CalculatePrice();
        string GetDescription();
    }
}