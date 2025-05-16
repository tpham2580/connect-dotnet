using BusinessService.Models;

namespace BusinessService.Utils;

public static class Utils
{
    /// Check to see if all Business Info is valid
    public static List<string> IsValidBusinessInfo(BusinessModel business, bool requireId = false)
    {
        var errors = new List<string>();

        if (requireId && (!business.Id.HasValue || business.Id.Value <= 0))
            errors.Add("Business ID is required for update.");

        if (string.IsNullOrWhiteSpace(business.Name))
            errors.Add("Name is required.");

        if (string.IsNullOrWhiteSpace(business.Address))
            errors.Add("Address is required.");

        if (string.IsNullOrWhiteSpace(business.City))
            errors.Add("City is required.");

        if (string.IsNullOrWhiteSpace(business.State))
            errors.Add("State is required.");

        if (business.Latitude < -90 || business.Latitude > 90)
            errors.Add("Latitude must be between -90 and 90.");

        if (business.Longitude < -180 || business.Longitude > 180)
            errors.Add("Longitude must be between -180 and 180.");

        return errors;
    }
}
