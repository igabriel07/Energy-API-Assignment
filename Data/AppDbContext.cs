using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StellarBlueAssignment.Models;

namespace StellarBlueAssignment.Data
{
    public class AppDbContext : DbContext{
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){

        }

        public DbSet<EnergyPriceEntity> DailyEnergyPrices { get; set; }
    }
}