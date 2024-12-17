using BerberSalonu.Models;
using Microsoft.EntityFrameworkCore;

namespace BerberSalonu.Veritabanı
{
    public class BerberContext : DbContext
    {
        public BerberContext(DbContextOptions<BerberContext> options) : base(options) { }

        public DbSet<Berber> Berberler { get; set; }
        public DbSet<Musteri> Musteriler { get; set; }
        public DbSet<Yetenek> Yetenekler { get; set; }
        public DbSet<BerberYetenek> BerberYetenekler { get; set; }
        public DbSet<Rol> Roller{ get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Randevu> Randevular {  get; set; }

    }
}
