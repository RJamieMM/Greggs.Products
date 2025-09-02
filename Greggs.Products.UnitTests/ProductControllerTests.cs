using System.Collections.Generic;
using System.Linq;
using Greggs.Products.Api;
using Greggs.Products.Api.Controllers;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Greggs.Products.UnitTests;

public class FakeDataAccess : IDataAccess<Product>
{
    private readonly List<Product> _data;
    public (int? start, int? size) LastCall { get; private set; }

    public FakeDataAccess(IEnumerable<Product> data)
    {
        _data = data.ToList();
    }

    public IEnumerable<Product> List(int? pageStart, int? pageSize)
    {
        LastCall = (pageStart, pageSize);
        var query = _data.AsQueryable();
        if (pageStart.HasValue) query = query.Skip(pageStart.Value);
        if (pageSize.HasValue) query = query.Take(pageSize.Value);
        return query.ToList();
    }
}

public class FakeConverter : ICurrencyConverter
{
    public decimal Convert(string from, string to, decimal amount) => amount * 1.11m;
}

public class ProductControllerTests
{
    [Fact]
    public void Get_UsesDataAccess_AndReturnsProductsInGBP()
    {
        // Arrange
        var seed = new[]
        {
            new Product { Name = "Sausage Roll", PriceInPounds = 1.20m },
            new Product { Name = "Steak Bake",    PriceInPounds = 2.00m },
            new Product { Name = "Yum Yum",       PriceInPounds = 0.85m }
        };
        var data = new FakeDataAccess(seed);
        var controller = new ProductController(new NullLogger<ProductController>(), data, new FakeConverter());

        // Act
        var result = controller.Get(0, 2, null).Result as OkObjectResult;
        var items = Assert.IsAssignableFrom<IEnumerable<Product>>(result!.Value);

        // Assert
        Assert.Equal((0, 2), data.LastCall);
        Assert.Equal(2, items.Count());
        Assert.Contains(items, p => p.Name == "Sausage Roll");
    }

    [Fact]
    public void Get_WhenCurrencyEUR_IncludesPriceInEuros()
    {
        // Arrange
        var seed = new[]
        {
            new Product { Name = "Sausage Roll", PriceInPounds = 1.00m },
        };
        var data = new FakeDataAccess(seed);
        var controller = new ProductController(new NullLogger<ProductController>(), data, new FakeConverter());

        // Act
        var result = controller.Get(0, 1, "EUR").Result as OkObjectResult;

        // Assert: anonymous type => use dynamic
        dynamic item = ((IEnumerable<object>)result!.Value!).Single();
        Assert.Equal("Sausage Roll", (string)item.Name);
        Assert.Equal(1.00m, (decimal)item.PriceInPounds);
        Assert.Equal(1.11m,  (decimal)item.PriceInEuros); // 1.00 * 1.11
    }
}
