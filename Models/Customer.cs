using System;
using System.Collections.Generic;

namespace GameRentalManagement.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
