using System.Text;
using readingsapi;
using readingsapi.adaptors;

namespace readingsapi_tests;

public class MeterReadingValidatorUnitTests
{
    [Fact]
    public async Task ValidateEmptyData()
    {
        // Given an empty data string
        var readingsData = string.Empty;

        // When I validate the data
        var validator = new MeterReadingValidator();
        var isValid = await validator.IsValidCsvAsync(readingsData);

        // Then false should be returned
        Assert.False(isValid);
    }

    [Fact]
    public async Task ValidateValidData()
    {
        // Given a string with valid data
        var readingsData = "2344,22/04/2019 09:24,1002,";

        // When I validate the data
        var validator = new MeterReadingValidator();
        var isValid = await validator.IsValidCsvAsync(readingsData);

        // Then true should be returned
        Assert.True(isValid);
    }
    

    [Fact]
    public async Task ValidateDataWithMissingFields()
    {
        // Given a string with valid data
        var readingsData = "2344,1002";

        // When I validate the data
        var validator = new MeterReadingValidator();
        var isValid = await validator.IsValidCsvAsync(readingsData);

        // Then true should be returned
        Assert.False(isValid);
    }
    
    [Theory]
    [InlineData("2344,,1002,")]
    [InlineData("2344,1002,")]
    public async Task ValidateDataWithMissingDataFields(string readingsData)
    {
        // Given a string with valid data

        // When I validate the data
        var validator = new MeterReadingValidator();
        var isValid = await validator.IsValidCsvAsync(readingsData);

        // Then true should be returned
        Assert.False(isValid);
    }
    
    [Fact]
    public async Task ValidateDataWithFieldsIncorectlyOrdered()
    {
        // Given a string with valid data
        var readingsData = "2344,1002,22/04/2019 09:24,";

        // When I validate the data
        var validator = new MeterReadingValidator();
        var isValid = await validator.IsValidCsvAsync(readingsData);

        // Then true should be returned
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("11")]
    [InlineData("111")]
    [InlineData("11111")]
    [InlineData("111111")]
    [InlineData("-1111")]
    [InlineData("abcd")]
    public async Task ValidateDataWithInvalidMeterReadingValue(string meterReadingValue)
    {
        // Given a string with an invalid meter reading value
        var readingsData = $"2233,22/04/2019 12:25,{meterReadingValue},";

        // When I validate the data
        var validator = new MeterReadingValidator();
        var isValid = await validator.IsValidCsvAsync(readingsData);

        // Then false should be returned
        Assert.False(isValid);
    }
    
    [Theory]
    [InlineData("1")]
    [InlineData("41/11/1999 12:00")]
    [InlineData("abcd")]
    public async Task ValidateDataWithInvalidMeterReadingDateTime(string meterReadingDateTime)
    {
        // Given a string with an invalid meter reading value
        var readingsData = $"2344,{meterReadingDateTime},1004,";

        // When I validate the data
        var validator = new MeterReadingValidator();
        var isValid = await validator.IsValidCsvAsync(readingsData);

        // Then false should be returned
        Assert.False(isValid);
    }

}