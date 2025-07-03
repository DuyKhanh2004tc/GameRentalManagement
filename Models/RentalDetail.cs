using System;
using System.Collections.Generic;

namespace GameRentalManagement.Models;

public partial class RentalDetail
{
    public int RentalDetailId { get; set; }

    public int RentalId { get; set; }

    public int GameId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtRent { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual Rental Rental { get; set; } = null!;
}
