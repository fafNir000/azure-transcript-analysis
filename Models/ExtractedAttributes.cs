namespace Task_2_TranscriptAnalysis.Models;

/// <summary>
/// The PII (Personally Identifiable Information) attributes extracted from a transcript.
///
/// Every property is nullable on purpose: if an attribute is not found in the
/// transcript, the property stays null and is returned as null in the JSON response.
/// </summary>
public class ExtractedAttributes
{
    /// <summary>Person's full name. Azure PII category: "Person".</summary>
    public string? Name { get; set; }

    /// <summary>Postal / street address. Azure PII category: "Address".</summary>
    public string? Address { get; set; }

    /// <summary>US Social Security Number. Azure PII category: "USSocialSecurityNumber".</summary>
    public string? SocialSecurityNumber { get; set; }

    /// <summary>Phone number. Azure PII category: "PhoneNumber".</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Email address. Azure PII category: "Email".</summary>
    public string? Email { get; set; }
}
