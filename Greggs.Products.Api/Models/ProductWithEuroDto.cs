namespace Greggs.Products.Api.Models;

public class ProductWithEuroDto
{
    public string Name { get; set; } = default!; //Product dispaly name when called, default to supress nullable warnings

    public decimal PriceInPounds { get; set; } //Source price in GBP

    public decimal PriceInEuros { get; set; } //EUR price set by fixed rate to convert from GBP set to 2 decimal place
}
