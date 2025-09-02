namespace Greggs.Products.Api;

public interface ICurrencyConverter
{
    decimal Convert(string from, string to, decimal amount); //Converts one currency to another then returns the converted amount

}
