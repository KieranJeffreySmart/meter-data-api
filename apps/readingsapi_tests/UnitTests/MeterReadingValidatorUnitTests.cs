using System.Text;
using Moq;
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
        var mockAccountRepo = new Mock<IAccountsRepository>();
        mockAccountRepo.Setup(repo => repo.AccountExists(It.IsAny<int>()))
            .ReturnsAsync(true);
        
        var mockMeterReadingRepo = new Mock<IMeterReadingReadRepository>();
        mockMeterReadingRepo.Setup(repo => repo.Exists(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        
        var validator = new MeterReadingValidator(mockAccountRepo.Object, mockMeterReadingRepo.Object);
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
        var mockAccountRepo = new Mock<IAccountsRepository>();
        mockAccountRepo.Setup(repo => repo.AccountExists(It.IsAny<int>()))
            .ReturnsAsync(true);
        var mockMeterReadingRepo = new Mock<IMeterReadingReadRepository>();
        mockMeterReadingRepo.Setup(repo => repo.Exists(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        
        var validator = new MeterReadingValidator(mockAccountRepo.Object, mockMeterReadingRepo.Object);
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
        var mockAccountRepo = new Mock<IAccountsRepository>();
        mockAccountRepo.Setup(repo => repo.AccountExists(It.IsAny<int>()))
            .ReturnsAsync(true);
        var mockMeterReadingRepo = new Mock<IMeterReadingReadRepository>();
        mockMeterReadingRepo.Setup(repo => repo.Exists(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        
        var validator = new MeterReadingValidator(mockAccountRepo.Object, mockMeterReadingRepo.Object);
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
        var mockAccountRepo = new Mock<IAccountsRepository>();
        mockAccountRepo.Setup(repo => repo.AccountExists(It.IsAny<int>()))
            .ReturnsAsync(true);
        var mockMeterReadingRepo = new Mock<IMeterReadingReadRepository>();
        mockMeterReadingRepo.Setup(repo => repo.Exists(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        
        var validator = new MeterReadingValidator(mockAccountRepo.Object, mockMeterReadingRepo.Object);
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
        var mockAccountRepo = new Mock<IAccountsRepository>();
        mockAccountRepo.Setup(repo => repo.AccountExists(It.IsAny<int>()))
            .ReturnsAsync(true);
        var mockMeterReadingRepo = new Mock<IMeterReadingReadRepository>();
        mockMeterReadingRepo.Setup(repo => repo.Exists(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        
        var validator = new MeterReadingValidator(mockAccountRepo.Object, mockMeterReadingRepo.Object);
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
        var mockAccountRepo = new Mock<IAccountsRepository>();
        mockAccountRepo.Setup(repo => repo.AccountExists(It.IsAny<int>()))
            .ReturnsAsync(true);
        var mockMeterReadingRepo = new Mock<IMeterReadingReadRepository>();
        mockMeterReadingRepo.Setup(repo => repo.Exists(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        
        var validator = new MeterReadingValidator(mockAccountRepo.Object, mockMeterReadingRepo.Object);
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
        var mockAccountRepo = new Mock<IAccountsRepository>();
        mockAccountRepo.Setup(repo => repo.AccountExists(It.IsAny<int>()))
            .ReturnsAsync(true);
        var mockMeterReadingRepo = new Mock<IMeterReadingReadRepository>();
        mockMeterReadingRepo.Setup(repo => repo.Exists(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        
        var validator = new MeterReadingValidator(mockAccountRepo.Object, mockMeterReadingRepo.Object);
        var isValid = await validator.IsValidCsvAsync(readingsData);

        // Then false should be returned
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("1111")]
    [InlineData("0")]
    [InlineData("abcd")]
    public async Task ValidateDataWithUnknownAccountId(string accountId)
    {
        // Given a string with valid data
        var readingsData = $"{accountId},22/04/2019 09:24,1002,";

        // When I validate the data
        var mockAccountRepo = new Mock<IAccountsRepository>();
        mockAccountRepo.Setup(repo => repo.AccountExists(It.Is<int>(id => id != 1111)))
            .ReturnsAsync(true);
        mockAccountRepo.Setup(repo => repo.AccountExists(It.Is<int>(id => id == 1111)))
            .ReturnsAsync(false);
        var mockMeterReadingRepo = new Mock<IMeterReadingReadRepository>();
        mockMeterReadingRepo.Setup(repo => repo.Exists(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        
        var validator = new MeterReadingValidator(mockAccountRepo.Object, mockMeterReadingRepo.Object);
        var isValid = await validator.IsValidCsvAsync(readingsData);

        // Then true should be returned
        Assert.False(isValid);
    }
    
    [Fact]
    public async Task ValidateDataIsUnique()
    {
        // Given a string with valid data
        var readingsData = "2344,22/04/2019 09:24,1002,";

        // When I validate the data
        var mockAccountRepo = new Mock<IAccountsRepository>();
        mockAccountRepo.Setup(repo => repo.AccountExists(It.IsAny<int>()))
            .ReturnsAsync(true);
        var mockMeterReadingRepo = new Mock<IMeterReadingReadRepository>();
        mockMeterReadingRepo.Setup(repo => repo.Exists(It.Is<int>(id => id != 2344), It.Is<DateTime>(dt => dt != new DateTime(2019, 4, 22, 9, 24, 0))))
            .ReturnsAsync(false);
        mockMeterReadingRepo.Setup(repo => repo.Exists(It.Is<int>(id => id == 2344), It.Is<DateTime>(dt => dt == new DateTime(2019, 4, 22, 9, 24, 0))))
            .ReturnsAsync(true);
        
        var validator = new MeterReadingValidator(mockAccountRepo.Object, mockMeterReadingRepo.Object);
        var isValid = await validator.IsValidCsvAsync(readingsData);

        // Then true should be returned
        Assert.False(isValid);
    }
}