using Xunit;
using BusinessService.Utils;
using BusinessService.Models;

namespace BusinessService.Tests.UnitTests;

public class UtilsTests
{
    [Fact]
    public void ReturnsNoErrors_WhenBusinessInfoIsValid()
    {
        var business = new BusinessModel
        {
            Name = "Test Business",
            Address = "123 Street",
            City = "Testville",
            State = "TS",
            Country = "USA",
            Latitude = 47.0,
            Longitude = -122.0
        };

        List<string> errors = Utils.Utils.IsValidBusinessInfo(business);
        Assert.Empty(errors);
    }

    [Fact]
    public void ReturnsError_WhenAddressIsMissing()
    {
        var business = new BusinessModel
        {
            Name = "Name",
            Address = " ",
            City = "City",
            State = "State",
            Country = "USA",
            Latitude = 45.0,
            Longitude = 90.0
        };

        var errors = Utils.Utils.IsValidBusinessInfo(business);
        Assert.Contains("Address is required.", errors);
    }

    [Fact]
    public void ReturnsErrors_WhenLatitudeAndLongitudeAreInvalid()
    {
        // Arrange
        var business = new BusinessModel
        {
            Name = "InvalidCoords",
            Address = "Somewhere",
            City = "Nowhere",
            State = "NA",
            Country = "USA",
            Latitude = -100.0,
            Longitude = 190.0
        };

        // Act
        var errors = Utils.Utils.IsValidBusinessInfo(business);

        // Assert
        Assert.Contains("Latitude must be between -90 and 90.", errors);
        Assert.Contains("Longitude must be between -180 and 180.", errors);
    }

    [Fact]
    public void ReturnsMultipleErrors_WhenMultipleFieldsAreInvalid()
    {
        var business = new BusinessModel
        {
            Name = "  ",
            Address = "",
            City = "",
            State = null,
            Country = "USA",
            Latitude = 120.0,
            Longitude = -200.0
        };

        var errors = Utils.Utils.IsValidBusinessInfo(business);

        Assert.Contains("Name is required.", errors);
        Assert.Contains("Address is required.", errors);
        Assert.Contains("City is required.", errors);
        Assert.Contains("State is required.", errors);
        Assert.Contains("Latitude must be between -90 and 90.", errors);
        Assert.Contains("Longitude must be between -180 and 180.", errors);
        Assert.Equal(6, errors.Count);
    }
}
