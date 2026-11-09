namespace Staybnb.Services
{
    // One place for the price rule:
    // TotalPrice = (PricePerNight x Nights) + CleaningFee + ServiceFee
    public static class BookingPricing
    {
        public const decimal ServiceFeeRate = 0.12m; // 12% of the nightly total

        public static decimal ServiceFee(decimal pricePerNight, int nights)
        {
            return Math.Round(pricePerNight * nights * ServiceFeeRate, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal Total(decimal pricePerNight, int nights, decimal cleaningFee)
        {
            return (pricePerNight * nights) + cleaningFee + ServiceFee(pricePerNight, nights);
        }
    }
}
