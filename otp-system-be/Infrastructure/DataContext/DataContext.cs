using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Infrastructure.DataContext
{
    public class DataAppContext: DbContext
    {
        public DataAppContext() { }
        public DataAppContext(DbContextOptions<DataAppContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("DefaultConnection");
            }
        }

        public DbSet<User> Users { get; set; } = null!;
    }
}
