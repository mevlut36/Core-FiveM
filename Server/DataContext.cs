using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Server
{
    public class DataContext : DbContext
    {
        public DataContext() { }

        public DbSet<PlayerTable> Player { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=localhost;database=fivem;user=root;password=");
        }
    }

    [Table("player")]
    public class PlayerTable
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("license")]
        public string License { get; set; }
        [Column("gender")]
        public string Gender { get; set; }
        [Column("firstname")]
        public string FirstName { get; set; }
        [Column("lastname")]
        public string LastName { get; set; }
        [Column("clothes")]
        public string Clothes { get; set; }
        [Column("money")]
        public int Money { get; set; }
        [Column("cars")]
        public string Cars { get; set; }
        [Column("phone")]
        public string Phone { get; set; }
        [Column("inventory")]
        public string Inventory { get; set; }
        [Column("birth")]
        public string Birth { get; set; }
        [Column("last_position")]
        public string LastPosition { get; set; }
    }

}
