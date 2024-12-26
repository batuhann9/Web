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
        public DbSet<Rol> Roller { get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Berber Silme Kontrolü
            var silinenBerberler = ChangeTracker.Entries<Berber>()
                .Where(e => e.State == EntityState.Deleted)
                .Select(e => e.Entity.Id)
                .ToList();

            if (silinenBerberler.Any())
            {
                var randevular = Randevular
                    .Where(r => silinenBerberler.Contains(r.BerberId))
                    .ToList();

                randevular.ForEach(r => r.Durum = RandevuDurum.IptalEdildi);
            }

            // Yetenek Silme Kontrolü
            var silinenYetenekler = ChangeTracker.Entries<Yetenek>()
                .Where(e => e.State == EntityState.Deleted)
                .Select(e => e.Entity.Id)
                .ToList();

            if (silinenYetenekler.Any())
            {
                var randevular = Randevular
                    .Where(r => silinenYetenekler.Contains(r.YetenekId))
                    .ToList();

                randevular.ForEach(r => r.Durum = RandevuDurum.IptalEdildi);
            }

            // Berber'den Yetenek Silindiğinde Randevular Güncellenir
            var silinenBerberYetenekler = ChangeTracker.Entries<BerberYetenek>()
                .Where(e => e.State == EntityState.Deleted)
                .Select(e => new { e.Entity.BerberId, e.Entity.YetenekId })
                .ToList();

            if (silinenBerberYetenekler.Any())
            {
                foreach (var item in silinenBerberYetenekler)
                {
                    var randevular = Randevular
                        .Where(r => r.BerberId == item.BerberId && r.YetenekId == item.YetenekId)
                        .ToList();

                    randevular.ForEach(r => r.Durum = RandevuDurum.IptalEdildi);
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}