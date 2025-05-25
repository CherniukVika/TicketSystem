namespace TicketSystem.BLL.Strategy
{
    public class HallPricingStrategy : ITicketPricingStrategy
    {
        public decimal CalculatePrice()
        {
            return 300m;
        }

        public string GetDescription()
        {
            return "Зал: 300 грн";
        }
    }
}
