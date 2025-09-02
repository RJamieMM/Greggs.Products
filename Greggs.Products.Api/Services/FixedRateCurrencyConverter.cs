using System;

namespace Greggs.Products.Api;

//Stores single GBP value to convert to EUR that is provided on call
//If the From and To values are the same then the original amount is returned
//Unsupported currencies thrown
    
public class FixedRateCurrencyConverter : ICurrencyConverter
{
    //For this example, an immutable exchange rate is used
    private readonly decimal _gbpToEur;

    //Fixed rate is injected
    public FixedRateCurrencyConverter(decimal gbpToEur)
    {
        _gbpToEur = gbpToEur;
    }

    public decimal Convert(string from, string to, decimal amount)
    {
        // GBP to EUR - multiply by the fixed rate
        if (from.Equals("GBP", StringComparison.OrdinalIgnoreCase) &&
            to.Equals("EUR", StringComparison.OrdinalIgnoreCase))
        {
            return amount * _gbpToEur;
        }

        // Same currency so no conversion is needed
        if (from.Equals(to, StringComparison.OrdinalIgnoreCase)) return amount;

        // Any currency other than GBP or EUR is not supported
        throw new NotSupportedException($"Conversion {from}->{to} not supported in this exercise.");
    }
}
