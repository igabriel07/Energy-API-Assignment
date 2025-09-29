using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellarBlueAssignment.Models{
    public class EnergyPriceEntity{
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public double AveragePrice { get; set; }
    }
}