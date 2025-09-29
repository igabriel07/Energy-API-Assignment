using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StellarBlueAssignment.Models{
    public class EnergyOptions{
        public const string StellarBlueApiSettings = "StellarBlueApiSettings";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}