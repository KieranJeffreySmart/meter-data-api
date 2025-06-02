using System.Text;
using Moq;
using readingsapi;

namespace readingsapi_tests;

public class MeterReadingFileParserUnitTests
{
    [Fact]
    public async Task ParseEmptyFile()
    {
        // Given an empty file
        var contentStream = new MemoryStream();

        // When I parse the file
        var mockValidator = new Mock<IMeterReadingValidator>();
        mockValidator.Setup(v => v.IsValidCsvAsync(It.IsAny<string>())).ReturnsAsync(true);
        var parser = new MeterReadingsFileParser(mockValidator.Object);
        var records = new List<NewMeterReadingDto>();
        await foreach (var (err, record) in parser.ParseAsync(contentStream))
        {
            records.Add(record);
        }

        // Then no readings should be returned
        Assert.Empty(records);
    }

    [Fact]
    public async Task ParseSingleRecord()
    {
        // Given a file with one record        
        var readingsData = "2344,22/04/2019 09:24,1002,";
        var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(readingsData));

        // When I parse the file
        var mockValidator = new Mock<IMeterReadingValidator>();
        mockValidator.Setup(v => v.IsValidCsvAsync(It.IsAny<string>())).ReturnsAsync(true);
        var parser = new MeterReadingsFileParser(mockValidator.Object);
        var records = new List<NewMeterReadingDto>();
        await foreach (var (err, record) in parser.ParseAsync(contentStream))
        {
            records.Add(record);
        }

        // Then a single readings should be returned
        Assert.Single(records);
        TestHelpers.AssertMeterReading(records.First(), 2344, new DateTime(2019, 4, 22, 9, 24, 0), 1002);
    }

    [Fact]
    public async Task ParseMultipleRecords()
    {
        // Given a file with one record      
        var csvDataBuilder = new StringBuilder();
        csvDataBuilder.AppendLine("2344,22/04/2019 09:24,1002,");
        csvDataBuilder.AppendLine("2233,22/04/2019 12:25,0323,");
        var readingsData = csvDataBuilder.ToString();
        var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(readingsData));

        // When I parse the file
        var mockValidator = new Mock<IMeterReadingValidator>();
        mockValidator.Setup(v => v.IsValidCsvAsync(It.IsAny<string>())).ReturnsAsync(true);
        var parser = new MeterReadingsFileParser(mockValidator.Object);
        var records = new List<NewMeterReadingDto>();
        await foreach (var (err, record) in parser.ParseAsync(contentStream))
        {
            records.Add(record);
        }

        // Then two readings should be returned
        Assert.Equal(2, records.Count);
        TestHelpers.AssertMeterReading(records[0], 2344, new DateTime(2019, 4, 22, 9, 24, 0), 1002);
        TestHelpers.AssertMeterReading(records[1], 2233, new DateTime(2019, 4, 22, 12, 25, 0), 323);
    }

    [Fact]
    public async Task ParseMultipleRecordsWithSingleInvaluidData()
    {
        // Given a file with one record      
        var csvDataBuilder = new StringBuilder();
        csvDataBuilder.AppendLine("2344,22/04/2019 09:24,1002,");
        csvDataBuilder.AppendLine("INVALID_DATA");
        csvDataBuilder.AppendLine("2344,08/04/2019 09:24,0000,");
        var readingsData = csvDataBuilder.ToString();
        var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(readingsData));

        // When I parse the file
        var mockValidator = new Mock<IMeterReadingValidator>();
        mockValidator.Setup(v => v.IsValidCsvAsync(It.Is<string>(s => !s.Contains("INVALID_DATA")))).ReturnsAsync(true);
        var parser = new MeterReadingsFileParser(mockValidator.Object);
        var records = new List<NewMeterReadingDto>();
        var numberOfInvalidLines = 0;
        await foreach (var (err, record) in parser.ParseAsync(contentStream))
        {
            if (err)
            {
                numberOfInvalidLines++;
                continue;
            }

            records.Add(record);
        }

        // Then two readings should be returned
        Assert.Equal(2, records.Count);
        TestHelpers.AssertMeterReading(records[0], 2344, new DateTime(2019, 4, 22, 9, 24, 0), 1002);
        TestHelpers.AssertMeterReading(records[1], 2344, new DateTime(2019, 4, 8, 9, 24, 0), 0);
    }
    
    [Fact]
    public async Task ParseMultipleRecordsWithSingleEmptyLine()
    {
        // Given a file with one record      
        var csvDataBuilder = new StringBuilder();
        csvDataBuilder.AppendLine("2344,22/04/2019 09:24,1002,");
        csvDataBuilder.AppendLine("");
        csvDataBuilder.AppendLine("2344,08/04/2019 09:24,0000,");
        var readingsData = csvDataBuilder.ToString();
        var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(readingsData));

        // When I parse the file
        var mockValidator = new Mock<IMeterReadingValidator>();
        mockValidator.Setup(v => v.IsValidCsvAsync(It.IsAny<string>())).ReturnsAsync(true);
        var parser = new MeterReadingsFileParser(mockValidator.Object);
        var records = new List<NewMeterReadingDto>();
        var numberOfInvalidLines = 0;
        await foreach (var (err, record) in parser.ParseAsync(contentStream))
        {
            if (err)
            {
                numberOfInvalidLines++;
                continue;
            }

            records.Add(record);
        }

        // Then two readings should be returned
        Assert.Equal(2, records.Count);
        TestHelpers.AssertMeterReading(records[0], 2344, new DateTime(2019, 4, 22, 9, 24, 0), 1002);
        TestHelpers.AssertMeterReading(records[1], 2344, new DateTime(2019, 4, 8, 9, 24, 0), 0);
    }

}