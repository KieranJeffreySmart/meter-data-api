using System.Text;
using Microsoft.AspNetCore.Http;
using readingsapi;

namespace readingsapi_tests;

public class MeterReadingFileParserUnitTests
{
    [Fact]
    public async Task ParseEmptyFile()
    {
        // Given an empty file
        var emptyFile = new FormFile(new MemoryStream(), 0, 0, "name", "empty.txt");

        // When I parse the file
        var parser = new MeterReadingsFileParser();
        var records = new List<MeterReading>();
        await foreach (var reading in parser.ParseAsync(emptyFile))
        {
            records.Add(reading);
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
        var file = new FormFile(contentStream, 0, contentStream.Length, "name", "empty.txt");

        // When I parse the file
        var parser = new MeterReadingsFileParser();
        var records = new List<MeterReading>();
        await foreach (var reading in parser.ParseAsync(file))
        {
            records.Add(reading);
        }

        // Then a single readings should be returned
        Assert.Single(records);
        var targetReading = records.First();
        Assert.Equal(2344, targetReading.AccountId);
        Assert.Equal(new DateTime(2019, 4, 22, 9, 24, 0), targetReading.MeterReadingDateTime);
        Assert.Equal(1002, targetReading.MeterReadValue);
    }

    [Fact]
    public async Task ParseMultipleRecords()
    {
        // Given a file with one record        
        var readingsData = "2344,22/04/2019 09:24,1002,";
        readingsData += "\n2233,22/04/2019 12:25,323,";
        var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(readingsData));
        var file = new FormFile(contentStream, 0, contentStream.Length, "name", "empty.txt");

        // When I parse the file
        var parser = new MeterReadingsFileParser();
        var records = new List<MeterReading>();
        await foreach (var reading in parser.ParseAsync(file))
        {
            records.Add(reading);
        }

        // Then a single readings should be returned
        Assert.Equal(2, records.Count);
        var targetReading = records[0];
        Assert.Equal(2344, targetReading.AccountId);
        Assert.Equal(new DateTime(2019, 4, 22, 9, 24, 0), targetReading.MeterReadingDateTime);
        Assert.Equal(1002, targetReading.MeterReadValue);
        targetReading = records[1];
        Assert.Equal(2233, targetReading.AccountId);
        Assert.Equal(new DateTime(2019, 4, 22, 12, 25, 0), targetReading.MeterReadingDateTime);
        Assert.Equal(323, targetReading.MeterReadValue);
    }

}