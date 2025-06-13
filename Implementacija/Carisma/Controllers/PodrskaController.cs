using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Carisma.Data;
using Carisma.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Carisma.Controllers
{
    [Authorize]
    public class PodrskaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PodrskaController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Podrska
        public async Task<IActionResult> Index(string statusFilter, string hitnostFilter)
        {
            ViewBag.StatusFilter = statusFilter;
            ViewBag.HitnostFilter = hitnostFilter;

            var currentUser = await _userManager.GetUserAsync(User);
            var zahtjevi = _context.Podrska.AsQueryable();

            // Ako nije podrška ili admin, vidi samo svoje zahtjeve
            if (!User.IsInRole("Podrska") && !User.IsInRole("Administrator"))
            {
                zahtjevi = zahtjevi.Where(p => p.KorisnikId == currentUser.Id);
            }

            // Filtriranje
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<statusZahtjeva>(statusFilter, out var status))
            {
                zahtjevi = zahtjevi.Where(p => p.Status == status);
            }

            if (!string.IsNullOrEmpty(hitnostFilter) && Enum.TryParse<NivoHitnosti>(hitnostFilter, out var hitnost))
            {
                zahtjevi = zahtjevi.Where(p => p.Hitnost == hitnost);
            }

            // Sortiranje po hitnosti i datumu
            zahtjevi = zahtjevi.OrderByDescending(p => p.Hitnost)
                             .ThenByDescending(p => p.DatumPostavljanja);

            return View(await zahtjevi.ToListAsync());
        }

        // GET: Podrska/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var podrska = await _context.Podrska
                .FirstOrDefaultAsync(m => m.Id == id);

            if (podrska == null) return NotFound();

            // Provjeri da li korisnik može vidjeti ovaj zahtjev
            var currentUser = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Podrska") && !User.IsInRole("Administrator") && podrska.KorisnikId != currentUser.Id)
            {
                return Forbid();
            }

            return View(podrska);
        }

        // GET: Podrska/Create
        public IActionResult Create()
        {
            if (User.IsInRole("Podrska") || User.IsInRole("Administrator"))
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }


        // POST: Podrska/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Pitanje,Hitnost")] Podrska podrska)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);

                podrska.DatumPostavljanja = DateTime.Now;
                podrska.Status = statusZahtjeva.Otvoren;
                podrska.KorisnikId = currentUser.Id;

                _context.Add(podrska);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Vaš zahtjev je uspješno poslat! Odgovoriće vam u najkraćem roku.";
                return RedirectToAction(nameof(Index));
            }
            return View(podrska);
        }

        // GET: Podrska/Edit/5
        [Authorize(Roles = "Podrska,Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var podrska = await _context.Podrska.FindAsync(id);
            if (podrska == null) return NotFound();

            return View(podrska);
        }

        // POST: Podrska/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Podrska,Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Pitanje,DatumPostavljanja,Odgovor,Status,Hitnost,KorisnikId,DatumOdgovora")] Podrska podrska)
        {
            if (id != podrska.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);

                    // Ako je dodan odgovor, postavi datum odgovora i korisnika koji je odgovorio
                    if (!string.IsNullOrEmpty(podrska.Odgovor) && podrska.DatumOdgovora == null)
                    {
                        podrska.DatumOdgovora = DateOnly.FromDateTime(DateTime.Now);
                        podrska.PodrskaKorisnikId = currentUser.Id;
                    }

                    _context.Update(podrska);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Odgovor je uspješno poslat korisniku!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PodrskaExists(podrska.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(podrska);
        }

        // POST: Podrska/Ocijeni/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ocijeni(int id, int ocjena, string komentar)
        {
            var podrska = await _context.Podrska.FindAsync(id);
            if (podrska == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (podrska.KorisnikId != currentUser.Id)
                return Forbid();

            podrska.OcjenaKorisnika = ocjena;
            podrska.KomentarOcjene = komentar;

            _context.Update(podrska);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hvala vam na ocjeni! Vaša povratna informacija nam je važna.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Podrska/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var podrska = await _context.Podrska
                .FirstOrDefaultAsync(m => m.Id == id);
            if (podrska == null) return NotFound();

            return View(podrska);
        }

        // POST: Podrska/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var podrska = await _context.Podrska.FindAsync(id);
            if (podrska != null)
            {
                _context.Podrska.Remove(podrska);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Zahtjev je uspješno obrisan.";
            return RedirectToAction(nameof(Index));
        }

        private bool PodrskaExists(int id)
        {
            return _context.Podrska.Any(e => e.Id == id);
        }

        // Statistike za podrška tim
        [Authorize(Roles = "Podrska,Administrator")]
        public async Task<IActionResult> Statistike()
        {
            var stats = new
            {
                UkupnoZahtjeva = await _context.Podrska.CountAsync(),
                OtvoreniZahtjevi = await _context.Podrska.CountAsync(p => p.Status == statusZahtjeva.Otvoren),
                ZahtjeviUObradi = await _context.Podrska.CountAsync(p => p.Status == statusZahtjeva.UObradi),
                ZatvorenikZahtjevi = await _context.Podrska.CountAsync(p => p.Status == statusZahtjeva.Zatvoren),
                KriticniZahtjevi = await _context.Podrska.CountAsync(p => p.Hitnost == NivoHitnosti.Kriticna && p.Status != statusZahtjeva.Zatvoren),
                ProsjecnaOcjena = await _context.Podrska.Where(p => p.OcjenaKorisnika.HasValue).AverageAsync(p => p.OcjenaKorisnika.Value)
            };

            return View(stats);
        }
    }
}