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
        public DbSet<CompanyTable> Company { get; set; }
        public DbSet<CarTable> Car { get; set; }
        public DbSet<EmployementTable> Employement { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=localhost;database=fivem_mev;user=root;password=");
        }
    }

    [Table("company")]
    public class CompanyTable
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("chest")]
        public string Chest { get; set; }
        [Column("taxes")]
        public string Taxes { get; set; }
    }

    [Table("car")]
    public class CarTable
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("player_id")]
        public int PlayerId { get; set; }
        [Column("model")]
        public string Model { get; set; }
        [Column("plate")]
        public string Plate { get; set; }
        [Column("state")]
        public string State { get; set; }
        [Column("color_type")]
        public int ColorType { get; set; }
        [Column("primary_color")]
        public int PrimaryColor { get; set; }
        [Column("secondary_color")]
        public int SecondaryColor { get; set; }
        [Column("mods")]
        public string Mods { get; set; }
        [Column("boot")]
        public string Boot { get; set; }
    }

    [Table("player")]
    public class PlayerTable
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("license")]
        public string License { get; set; }
        [Column("discord")]
        public string Discord { get; set; }
        [Column("bitcoin")]
        public int Bitcoin { get; set; }
        [Column("state")]
        public string State { get; set; }
        [Column("skin")]
        public string Skin { get; set; }
        [Column("firstname")]
        public string FirstName { get; set; }
        [Column("lastname")]
        public string LastName { get; set; }
        [Column("rank")]
        public string Rank { get; set; }
        [Column("organisation")]
        public string Organisation { get; set; }
        [Column("clothes")]
        public string Clothes { get; set; }
        [Column("clothes_list")]
        public string ClothesList { get; set; }
        [Column("money")]
        public int Money { get; set; }
        [Column("bills")]
        public string Bills { get; set; }
        [Column("phone")]
        public string Phone { get; set; }
        [Column("inventory")]
        public string Inventory { get; set; }
        [Column("birth")]
        public string Birth { get; set; }
        [Column("last_position")]
        public string LastPosition { get; set; }
    }

    [Table("employment")]
    public class EmployementTable
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("player_id")]
        public int PlayerId { get; set; }
        [Column("company_id")]
        public int CompanyId { get; set; }
        [Column("rank")]
        public int Rank { get; set; }
    }
}
