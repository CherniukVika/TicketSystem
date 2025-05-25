namespace TicketSystem.BLL.Strategy
{
    public interface ITicketPricingStrategy
    {
        decimal CalculatePrice();
        string GetDescription();
    }
}
