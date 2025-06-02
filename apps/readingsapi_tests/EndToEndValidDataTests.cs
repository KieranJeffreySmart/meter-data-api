using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using readingsapi;

namespace readingsapi_tests;

public class EndToEndValidDataTests : IClassFixture<WebApplicationFactory<Program>>
{
    WebApplicationFactory<Program> _factory;
    public EndToEndValidDataTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SubmitAcceptanceData()
    {
        // Given I have many customer accounts
        string localDbName = "TestDB_" + Guid.NewGuid().ToString();
        var localWebFactory = await TestHelpers.CreateSeededWebFactoryAsync(_factory, "./test_data/Test_Accounts.csv", localDbName);

        // And I have a collection of meter readings data
        var readingsData = await File.ReadAllTextAsync("./test_data/Meter_Reading.csv");

        // When I submit the data
        var client = localWebFactory.CreateClient();
        var content = TestHelpers.CreateFakeMultiPartFormData(readingsData);
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed of the number of successful readings submitted
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
        Assert.NotNull(responseData);
        Assert.Equal(9, responseData.Succedded);
        Assert.Equal(27, responseData.Failed);

        // And I can see all the data was persisted
        // Note: This step is not part of the acceptance criteria and I could use flat files
        //       however the exercise requires that the data is persisted to a DB and so I have added this to the test
        DateTimeFormatInfo dtfi = new DateTimeFormatInfo
        {
            ShortDatePattern = "dd/MM/yyyy",
            LongDatePattern = "dd/MM/yyyy HH:mm"
        };

        var records = TestHelpers.GetReadingsFromContext(localDbName).ToList();

        TestHelpers.AssertMeterReading(records[0], 1234, DateTime.Parse("12/05/2019 09:24", dtfi), 9787);
        TestHelpers.AssertMeterReading(records[1], 1244, DateTime.Parse("25/05/2019 09:24", dtfi), 3478);
        TestHelpers.AssertMeterReading(records[2], 1246, DateTime.Parse("25/05/2019 09:24", dtfi), 3455);
        TestHelpers.AssertMeterReading(records[3], 1248, DateTime.Parse("26/05/2019 09:24", dtfi), 3467);
        TestHelpers.AssertMeterReading(records[4], 2344, DateTime.Parse("22/04/2019 09:24", dtfi), 1002);
        TestHelpers.AssertMeterReading(records[5], 2344, DateTime.Parse("22/04/2019 12:25", dtfi), 1002);
        TestHelpers.AssertMeterReading(records[6], 2350, DateTime.Parse("22/04/2019 12:25", dtfi), 5684);
        TestHelpers.AssertMeterReading(records[7], 2353, DateTime.Parse("22/04/2019 12:25", dtfi), 1212);
        TestHelpers.AssertMeterReading(records[8], 8766, DateTime.Parse("22/04/2019 12:25", dtfi), 3440);
    }

    [Fact]
    public async Task SubmitEmptyFormData()
    {
        // Note: This test is not part of the acceptance criteria but following the 0/1/many rule this is required
        // When I submit empty data
        var client = _factory.CreateClient();
        var content = new MultipartFormDataContent { };
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed my request was bad
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseData = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"Bad Request: Invalid data provided.\"", responseData);
    }

    [Fact]
    public async Task SubmitSingleValidReadingForSingleCustomer()
    {
        // Given I have a customer account
        string localDbName = "TestDB_" + Guid.NewGuid().ToString();
        var localWebFactory = TestHelpers.CreateSeededWebFactory(_factory, [new Account(2344, "John", "Doe")], localDbName);

        // And I have a single entry of meter reading data
        var readingsData = "2344,22/04/2019 09:24,1002,";

        // When I submit the data
        var client = localWebFactory.CreateClient();
        var content = TestHelpers.CreateFakeMultiPartFormData(readingsData);
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed the reading was successfully submitted
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
        Assert.NotNull(responseData);
        Assert.Equal(1, responseData.Succedded);
        Assert.Equal(0, responseData.Failed);

        // And I can see the reading was persisted
        Assert.NotNull(localDbName);
        Assert.NotEqual(string.Empty, localDbName);
        var readings = TestHelpers.GetReadingsFromContext(localDbName).ToList();
        Assert.Single(readings);
        var reading = readings.First();
        TestHelpers.AssertMeterReading(reading, 2344, new DateTime(2019, 4, 22, 9, 24, 0), 1002);
    }

    [Fact]
    public async Task SubmitMultipleUnorderedValidReadingForSingleCustomer()
    {
        // Given I have a customer account
        string localDbName = "TestDB_" + Guid.NewGuid().ToString();
        var localWebFactory = TestHelpers.CreateSeededWebFactory(_factory, [new Account(2344, "John", "Doe")], localDbName);

        // And I have a single entry of meter reading data
        var csvDataBuilder = new StringBuilder();
        csvDataBuilder.AppendLine("2344,22/04/2019 09:24,1002,");
        csvDataBuilder.AppendLine("2344,22/04/2019 12:25,1004,");
        csvDataBuilder.AppendLine("2344,08/04/2019 09:24,0000,");
        var readingsData = csvDataBuilder.ToString();

        // When I submit the data
        var client = localWebFactory.CreateClient();
        var content = TestHelpers.CreateFakeMultiPartFormData(readingsData);
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed the reading was successfully submitted
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
        Assert.NotNull(responseData);
        Assert.Equal(3, responseData.Succedded);
        Assert.Equal(0, responseData.Failed);

        // And I can see the reading was persisted
        Assert.NotNull(localDbName);
        Assert.NotEqual(string.Empty, localDbName);
        var readings = TestHelpers.GetReadingsFromContext(localDbName).ToList();
        Assert.Equal(3, readings.Count);
        TestHelpers.AssertMeterReading(readings[0], 2344, new DateTime(2019, 4, 8, 9, 24, 0), 0);
        TestHelpers.AssertMeterReading(readings[1], 2344, new DateTime(2019, 4, 22, 9, 24, 0), 1002);
        TestHelpers.AssertMeterReading(readings[2], 2344, new DateTime(2019, 4, 22, 12, 25, 0), 1004);
    }
    
    [Fact]
    public async Task SubmitMultipleUnorderedValidReadingForSingleCustomerOverMultipleFiles()
    {
        // Given I have a customer account
        string localDbName = "TestDB_" + Guid.NewGuid().ToString();
        var localWebFactory = TestHelpers.CreateSeededWebFactory(_factory, [new Account(2344, "John", "Doe")], localDbName);

        // And I have a single entry of meter reading data
        var csvDataBuilder = new StringBuilder();
        csvDataBuilder.AppendLine("2344,22/04/2019 09:24,1002,");
        csvDataBuilder.AppendLine("2344,22/04/2019 12:25,1004,");
        var readingsData = csvDataBuilder.ToString();

        // When I submit the data
        var client = localWebFactory.CreateClient();
        var content = TestHelpers.CreateFakeMultiPartFormData(readingsData);
        
        ByteArrayContent byteContent = new(Encoding.UTF8.GetBytes("2344,08/04/2019 09:24,0000,"));
        content.Add(byteContent, "file", "readings1.csv");
        var response = await client.PostAsync("/meter-reading-uploads", content);

        // Then I should be informed the reading was successfully submitted
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
        Assert.NotNull(responseData);
        Assert.Equal(3, responseData.Succedded);
        Assert.Equal(0, responseData.Failed);

        // And I can see the reading was persisted
        Assert.NotNull(localDbName);
        Assert.NotEqual(string.Empty, localDbName);
        var readings = TestHelpers.GetReadingsFromContext(localDbName).ToList();
        Assert.Equal(3, readings.Count);
        TestHelpers.AssertMeterReading(readings[0], 2344, new DateTime(2019, 4, 8, 9, 24, 0), 0);
        TestHelpers.AssertMeterReading(readings[1], 2344, new DateTime(2019, 4, 22, 9, 24, 0), 1002);
        TestHelpers.AssertMeterReading(readings[2], 2344, new DateTime(2019, 4, 22, 12, 25, 0), 1004);
    }
}
