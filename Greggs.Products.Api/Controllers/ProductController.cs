using System;
using System.Collections.Generic;
using System.Linq;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.DataAccess; //Added access to products via IDataAccess
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Greggs.Products.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IDataAccess<Product> _products; //Injects data access to repalce random data
    private readonly ICurrencyConverter _converter; //Injects converter for EUR pricing

    public ProductController(
        ILogger<ProductController> logger,
        IDataAccess<Product> products,
        ICurrencyConverter converter)
    {
        _logger = logger;
        _products = products;
        _converter = converter;
    }

    [HttpGet]
    public ActionResult<IEnumerable<object>> Get( //Returns ActionResult to allow 400s and EUR projection
        [FromQuery] int? pageStart = 0, //Now nullable and validated
        [FromQuery] int? pageSize = 5, //Now nullable and validated
        [FromQuery] string currency = null) //Opt in for currency switch if desired
    {
        if (pageStart < 0) return BadRequest("pageStart must be >= 0"); //Guard rails
        if (pageSize <= 0) return BadRequest("pageSize must be > 0"); //Guard rails

        var items = _products.List(pageStart, pageSize); //Fetch from data access rather than random generation

        if (!string.IsNullOrWhiteSpace(currency) &&
            currency.Equals("EUR", StringComparison.OrdinalIgnoreCase)) //EUR path
        {
            var projected = items.Select(p => new ProductWithEuroDto //Project to DTO
            {
                Name = p.Name,
                PriceInPounds = p.PriceInPounds,
                PriceInEuros = Math.Round(
                    _converter.Convert("GBP", "EUR", p.PriceInPounds), //Uses converter's fixed rate
                    2,
                    MidpointRounding.ToEven) //Banker's rounding as dealing with currency
            });

            return Ok(projected);
        }

        return Ok(items);
    }
}
