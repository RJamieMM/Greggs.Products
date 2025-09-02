using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Greggs.Products.Api.DataAccess; //Added for IDataAccess<Product> and ProductAccess
using Greggs.Products.Api.Models; // Added for Product type used in DI registration

namespace Greggs.Products.Api;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen();

        services.AddSingleton<IDataAccess<Product>, ProductAccess>(); //Added to use data access layer instead of random data
        services.AddSingleton<ICurrencyConverter>(_ => new FixedRateCurrencyConverter(1.11m)); //Added to set the fixed rate currency conversaion
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Greggs Products API V1"); });

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}