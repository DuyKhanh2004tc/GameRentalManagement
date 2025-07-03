using System;
using System.Collections.Generic;

namespace GameRentalManagement.Models;

public partial class Rental
{
    public int RentalId { get; set; }

    public int CustomerId { get; set; }

    public DateOnly RentalDate { get; set; }

    public DateOnly DueDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public string Status { get; set; } = null!;

    public int ProcessedBy { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual User ProcessedByNavigation { get; set; } = null!;

    public virtual ICollection<RentalDetail> RentalDetails { get; set; } = new List<RentalDetail>();
}
