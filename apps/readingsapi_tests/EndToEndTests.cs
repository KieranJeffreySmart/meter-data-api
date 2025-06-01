using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace readingsapi_tests;

public class EndToEndTests : IClassFixture<WebApplicationFactory<Program>>
{
    // As an Energy Company Account Manager, 
    // I want to be able to load a CSV file of Customer Meter Readings 
    // so that we can monitor their energy consumption and charge them accordingly

    // Create the following endpoint:	
    // 	POST => /meter-reading-uploads
    // The endpoint should be able to process a CSV of meter readings. An example CSV file has been provided (Meter_reading.csv)
    // Each entry in the CSV should be validated and if valid, stored in a DB.
    // After processing, the number of successful/failed readings should be returned.

    // Validation: 
    // You should not be able to load the same entry twice
    // A meter reading must be associated with an Account ID to be deemed valid
    // Reading values should be in the format NNNNN

    // NICE TO HAVE
    // Create a client in the technology of your choosing to consume the API. You can use angular/react/whatever you like
    // When an account has an existing read, ensure the new read isn’t older than the existing read

    WebApplicationFactory<Program> _factory;
    public EndToEndTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SubmitAcceptanceData()
    {
        // Given I have many customer accounts
        var accountsData = await File.ReadAllTextAsync("./test_data/Test_Accounts.csv");
        //TODO: seed accounts data into the database

        // And I have a collection of meter readings data
        var readingsData = await File.ReadAllTextAsync("./test_data/Meter_Reading.csv");

        // When I submit the data
        var client = _factory.CreateClient();
        var content = new StringContent(readingsData);
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed of the number of successful readings submitted
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadAsStringAsync();
        Assert.Contains("10", responseData); // TODO: Adjust based on actual valid data from file

        // And I can see all the data was persisted
        // Note: This step is not actusally part of the acceptance criteria and I could use flat files
        //       however the exercise requires that the data is persisted to a DB and so I have added this to the test
        // TODO: Figure out the easiest way to get persisted data
    }

    [Fact]
    public async Task SubmitEmptyData()
    {
        // When I submit empty data
        var client = _factory.CreateClient();
        var content = new MultipartFormDataContent {  };
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed my request was bad
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseData = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"Bad Request: Invalid data provided.\"", responseData);
    }

    [Fact]
    public async Task SubmitSingleValidReadingForSingleCustomer()
    {
        // Given I have a customer account
        string localDbName = string.Empty;
        var localWebFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(s =>
            {
                s.AddDbContext<MeterReadingsContext>(options =>
                {
                    localDbName = "TestDB_" + Guid.NewGuid().ToString();
                    options.UseInMemoryDatabase(localDbName)
                    .UseAsyncSeeding(async (context, _, camcellationToken) =>
                    {
                        
                        (context as MeterReadingsContext)?.Accounts.Add(new Account(1002, "John", "Doe"));
                        await context.SaveChangesAsync(camcellationToken);
                    });
                });
            });
        });

        // And I have a single entry of meter reading data
        var readingsData = "2344,22/04/2019 09:24,1002,";

        // When I submit the data
        var client = localWebFactory.CreateClient();
        var content = CreateFakeMultiPartFormData(readingsData);
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed the reading was successfully submitted
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadAsStringAsync();
        Assert.Contains("1", responseData);

        // And I can see the reading was persisted
        Assert.NotNull(localDbName);
        Assert.NotEqual(string.Empty, localDbName);
        var localContext = new MeterReadingsContext(
            new DbContextOptionsBuilder<MeterReadingsContext>()
                .UseInMemoryDatabase(localDbName)
                .Options);
        Assert.Equal(1, localContext.Readings.Count());
        var reading = localContext.Readings.First();
        Assert.Equal(2344, reading.AccountId);
        Assert.Equal(new DateTime(2019, 4, 22, 9, 24, 0), reading.MeterReadingDateTime);
        Assert.Equal(1002, reading.MeterReadValue);
    }
    
    private static MultipartFormDataContent CreateFakeMultiPartFormData(string content)
    {
        ByteArrayContent byteContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));

        MultipartFormDataContent multipartContent = new MultipartFormDataContent { {byteContent, "file", "readings.csv"} };
        return multipartContent;
    }
}
