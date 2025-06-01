using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using readingsapi;

namespace readingsapi_tests;

public class EndToEndInvalidDataTests : IClassFixture<WebApplicationFactory<Program>>
{
    // Validation: 
    // You should not be able to load the same entry twice
    // A meter reading must be associated with an Account ID to be deemed valid
    // Reading values should be in the format NNNNN

    WebApplicationFactory<Program> _factory;
    public EndToEndInvalidDataTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SubmitEmptyFileData()
    {
        // Note: This test is not part of the acceptance criteria but following the 0/1/many rule this is required
        // When I submit empty data
        var client = _factory.CreateClient();
        var content = TestHelpers.CreateFakeMultiPartFormData(string.Empty);
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then no recordds should be processed
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"0\"", responseData);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("11")]
    [InlineData("111")]
    [InlineData("11111")]
    [InlineData("111111")]
    [InlineData("-1111")]
    [InlineData("abcd")]
    public async Task SubmitInvalidMeterReadValueForSingleCustomer(string meterReadingValue)
    {
        // Given I have a customer account
        string localDbName = "TestDB_" + Guid.NewGuid().ToString();
        var localWebFactory = TestHelpers.CreateSeededWebFactory(_factory, [new Account(2344, "John", "Doe")], localDbName);

        // And I have a single entry of meter reading data
        var csvDataBuilder = new StringBuilder();
        csvDataBuilder.AppendLine("2344,22/04/2019 09:24,1002,");
        csvDataBuilder.AppendLine($"2344,22/04/2019 12:25,{meterReadingValue},");
        csvDataBuilder.AppendLine("2344,08/04/2019 09:24,0000,");
        var readingsData = csvDataBuilder.ToString();

        // When I submit the data
        var client = localWebFactory.CreateClient();
        var content = TestHelpers.CreateFakeMultiPartFormData(readingsData);
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed the reading was successfully submitted
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"2\"", responseData);

        // And I can see the reading was persisted
        Assert.NotNull(localDbName);
        Assert.NotEqual(string.Empty, localDbName);
        var readings = TestHelpers.GetReadingsFromContext(localDbName).ToList();
        Assert.Equal(2, readings.Count);
        TestHelpers.AssertMeterReading(readings[0], 2344, new DateTime(2019, 4, 8, 9, 24, 0), 0);
        TestHelpers.AssertMeterReading(readings[1], 2344, new DateTime(2019, 4, 22, 9, 24, 0), 1002);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("41/11/1999 12:00")]
    [InlineData("abcd")]
    public async Task SubmitInvalidMeterReadingDateTimeForSingleCustomer(string meterReadingDateTime)
    {
        // Given I have a customer account
        string localDbName = "TestDB_" + Guid.NewGuid().ToString();
        var localWebFactory = TestHelpers.CreateSeededWebFactory(_factory, [new Account(2344, "John", "Doe")], localDbName);

        // And I have a single entry of meter reading data
        var csvDataBuilder = new StringBuilder();
        csvDataBuilder.AppendLine("2344,22/04/2019 09:24,1002,");
        csvDataBuilder.AppendLine($"2344,{meterReadingDateTime},1004,");
        csvDataBuilder.AppendLine("2344,08/04/2019 09:24,0000,");
        var readingsData = csvDataBuilder.ToString();

        // When I submit the data
        var client = localWebFactory.CreateClient();
        var content = TestHelpers.CreateFakeMultiPartFormData(readingsData);
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed the reading was successfully submitted
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"2\"", responseData);

        // And I can see the reading was persisted
        Assert.NotNull(localDbName);
        Assert.NotEqual(string.Empty, localDbName);
        var readings = TestHelpers.GetReadingsFromContext(localDbName).ToList();
        Assert.Equal(2, readings.Count);
        TestHelpers.AssertMeterReading(readings[0], 2344, new DateTime(2019, 4, 8, 9, 24, 0), 0);
        TestHelpers.AssertMeterReading(readings[1], 2344, new DateTime(2019, 4, 22, 9, 24, 0), 1002);
    }

    [Fact]
    public async Task SubmitMeterReadingInvalidCustomer()
    {
        // Given I have a customer account
        string localDbName = "TestDB_" + Guid.NewGuid().ToString();
        var localWebFactory = TestHelpers.CreateSeededWebFactory(_factory, [new Account(2344, "John", "Doe")], localDbName);

        // And I have a single entry of meter reading data
        var csvDataBuilder = new StringBuilder();
        csvDataBuilder.AppendLine("2344,22/04/2019 09:24,1002,");
        csvDataBuilder.AppendLine("1111,22/04/2019 12:25,1004,");
        csvDataBuilder.AppendLine("2344,08/04/2019 09:24,0000,");
        var readingsData = csvDataBuilder.ToString();

        // When I submit the data
        var client = localWebFactory.CreateClient();
        var content = TestHelpers.CreateFakeMultiPartFormData(readingsData);
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed the reading was successfully submitted
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"2\"", responseData);

        // And I can see the reading was persisted
        Assert.NotNull(localDbName);
        Assert.NotEqual(string.Empty, localDbName);
        var readings = TestHelpers.GetReadingsFromContext(localDbName).ToList();
        Assert.Equal(2, readings.Count);
        TestHelpers.AssertMeterReading(readings[0], 2344, new DateTime(2019, 4, 8, 9, 24, 0), 0);
        TestHelpers.AssertMeterReading(readings[1], 2344, new DateTime(2019, 4, 22, 9, 24, 0), 1002);
    }
    
    //TODO: Add tests for duplicate entries, and other invalid data scenarios
}
