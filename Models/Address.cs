﻿using E_Commers.Models;

public class Address
{
    public int AddressId { get; set; }
    public string UserId { get; set; }
    public string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public bool IsDefault { get; set; } = false;
    public string AddressType { get; set; } // Shipping, Billing, etc.

    // Navigation property
    public ApplicationUser User { get; set; }
}