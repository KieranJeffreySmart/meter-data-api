using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace readingsapi_tests;

public class EndToEndTests: IClassFixture<WebApplicationFactory<Program>>
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
    }

    [Fact]
    public async Task SubmitEmptyData()
    {
        // When I submit empty data
        var client = _factory.CreateClient();
        var content = new StringContent(string.Empty);
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed my request was bad
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SubmitSingleValidReadingForSingleCustomer()
    {
        // Given I have a customer account
        var accountData = "2344,Tommy,Test";
        //TODO: seed accounts data into the database

        // And I have some meter reading data
        var readingsData = "2344,22/04/2019 09:24,1002,";

        // When I submit the data
        var client = _factory.CreateClient();
        var content = new StringContent(readingsData);
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed all readings were successfully submitted
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadAsStringAsync();
        Assert.Contains("1", responseData);
    }
}
