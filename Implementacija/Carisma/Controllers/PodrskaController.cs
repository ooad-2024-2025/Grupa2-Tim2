using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Carisma.Data;
using Carisma.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Carisma.Controllers
{
    public class PodrskaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PodrskaController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Podrska - samo za ulogovane korisnike
        [Authorize]
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

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var podrska = await _context.Podrska.FirstOrDefaultAsync(m => m.Id == id);
            if (podrska == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Podrska") && !User.IsInRole("Administrator") && podrska.KorisnikId != currentUser.Id)
            {
                return Forbid();
            }

            // Ako je korisnik vlasnik zahtjeva i ima novi odgovor, označi kao pročitano
            if (podrska.KorisnikId == currentUser.Id && podrska.ImaNoviOdgovor)
            {
                podrska.ImaNoviOdgovor = false;
                _context.Update(podrska);
                await _context.SaveChangesAsync();
            }

            return View(podrska);
        }

        // GET: Podrska/Create - DOSTUPNO SVIMA (i neregistrovanim korisnicima)
        public IActionResult Create()
        {
            // Ako je korisnik ulogovan i admin/podrška, preusmjeri na Index
            if (User.Identity.IsAuthenticated && (User.IsInRole("Podrska") || User.IsInRole("Administrator")))
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Pitanje,Hitnost,Email,ImePrezime")] Podrska podrska)
        {
            if (ModelState.IsValid)
            {
                podrska.DatumPostavljanja = DateTime.Now;
                podrska.Status = statusZahtjeva.Otvoren;

                // Ako je korisnik ulogovan, koristi njegov ID
                if (User.Identity.IsAuthenticated)
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    podrska.KorisnikId = currentUser.Id;
                }
                // Inače ostavi KorisnikId kao null (neregistrovani korisnik)

                _context.Add(podrska);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Vas zahtjev je uspjesno poslat! Odgovor cete dobiti u najkracem roku.";

                // PROMJENA: Svi korisnici (i registrovani i neregistrovani) idu na CreateSuccess
                return RedirectToAction("Create");
            }
            return View(podrska);
        }

        // GET: Podrska/CreateSuccess - stranica zahvalnice za neregistrovane korisnike
        public IActionResult CreateSuccess()
        {
            return View();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Podrska,Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Pitanje,DatumPostavljanja,Odgovor,Status,Hitnost,KorisnikId,DatumOdgovora,ImaNoviOdgovor")] Podrska podrska)
        {
            if (id != podrska.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    var originalPodrska = await _context.Podrska.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

                    // Provjeri da li je dodan novi odgovor
                    bool newResponseAdded = !string.IsNullOrEmpty(podrska.Odgovor) &&
                                           (originalPodrska == null || string.IsNullOrEmpty(originalPodrska.Odgovor));

                    if (newResponseAdded)
                    {
                        podrska.DatumOdgovora = DateOnly.FromDateTime(DateTime.Now);
                        podrska.PodrskaKorisnikId = currentUser.Id;
                        podrska.ImaNoviOdgovor = true; // NOVO: označi da ima novi odgovor
                    }
                    else
                    {
                        // Zadrži postojeće vrijednosti ako nije novi odgovor
                        podrska.DatumOdgovora = originalPodrska?.DatumOdgovora;
                        podrska.PodrskaKorisnikId = originalPodrska?.PodrskaKorisnikId;
                        podrska.ImaNoviOdgovor = originalPodrska?.ImaNoviOdgovor ?? false;
                    }

                    _context.Update(podrska);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Odgovor je uspjesno poslat korisniku!";
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
        [Authorize]
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

            TempData["Success"] = "Hvala vam na ocjeni! Vasa povratna informacija nam je vazna.";
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
            TempData["Success"] = "Zahtjev je uspjesno obrisan.";
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

        // API endpoint za provjeru novih notifikacija
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetNotifications()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Json(new { count = 0, notifications = new List<object>() });

            var newResponses = await _context.Podrska
                .Where(p => p.KorisnikId == currentUser.Id && p.ImaNoviOdgovor)
                .Select(p => new {
                    id = p.Id,
                    title = $"Novi odgovor na zahtjev #{p.Id}",
                    message = p.Pitanje.Length > 50 ? p.Pitanje.Substring(0, 50) + "..." : p.Pitanje,
                    date = p.DatumOdgovora != null ? p.DatumOdgovora.Value.ToString("dd.MM.yyyy") : "",
                    priority = p.Hitnost.ToString()
                })
                .ToListAsync();

            return Json(new { count = newResponses.Count, notifications = newResponses });
        }
    }
}