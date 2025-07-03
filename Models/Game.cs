using System;
using System.Collections.Generic;

namespace GameRentalManagement.Models;

public partial class Game
{
    public int GameId { get; set; }

    public string GameName { get; set; } = null!;

    public string? Platform { get; set; }

    public string? Genre { get; set; }

    public decimal PricePerDay { get; set; }

    public int Quantity { get; set; }

    public bool Status { get; set; }

    public virtual ICollection<RentalDetail> RentalDetails { get; set; } = new List<RentalDetail>();
}
