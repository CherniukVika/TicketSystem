namespace TicketSystem.BLL.Strategy
{
    public class BalconyPricingStrategy : ITicketPricingStrategy
    {
        public decimal CalculatePrice()
        {
            return 250m;
        }

        public string GetDescription()
        {
            return "Балкон: 250 грн";
        }
    }
}
