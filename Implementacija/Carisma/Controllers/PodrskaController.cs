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

        public async Task<IActionResult> Index(string statusFilter, string hitnostFilter)
        {
           
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Create));
            }

            ViewBag.StatusFilter = statusFilter;
            ViewBag.HitnostFilter = hitnostFilter;

            var currentUser = await _userManager.GetUserAsync(User);
            var zahtjevi = _context.Podrska.AsQueryable();

            if (!User.IsInRole("Podrska") && !User.IsInRole("Administrator"))
            {
                zahtjevi = zahtjevi.Where(p => p.KorisnikId == currentUser.Id);
            }

            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<statusZahtjeva>(statusFilter, out var status))
            {
                zahtjevi = zahtjevi.Where(p => p.Status == status);
            }

            if (!string.IsNullOrEmpty(hitnostFilter) && Enum.TryParse<NivoHitnosti>(hitnostFilter, out var hitnost))
            {
                zahtjevi = zahtjevi.Where(p => p.Hitnost == hitnost);
            }

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

            if (podrska.KorisnikId == currentUser.Id && podrska.ImaNoviOdgovor)
            {
                podrska.ImaNoviOdgovor = false;
                _context.Update(podrska);
                await _context.SaveChangesAsync();
            }

            return View(podrska);
        }

    
        public IActionResult Create()
        {
            
            if (User.Identity.IsAuthenticated && (User.IsInRole("Podrska") || User.IsInRole("Administrator")))
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] 
        public async Task<IActionResult> Create([Bind("Pitanje,Hitnost")] Podrska podrska)
        {
          
            if (User.IsInRole("Podrska") || User.IsInRole("Administrator"))
            {
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);

                    podrska.DatumPostavljanja = DateTime.Now;
                    podrska.Status = statusZahtjeva.Otvoren;
                    podrska.KorisnikId = currentUser.Id;

                    _context.Add(podrska);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Vaš zahtjev je uspješno poslat! Odgovor ćete dobiti u najkraćem roku.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Došlo je do greške prilikom čuvanja zahtjeva. Pokušajte ponovo.");
                }
            }

            return View(podrska);
        }

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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Pitanje,DatumPostavljanja,Odgovor,Status,Hitnost,KorisnikId,DatumOdgovora,ImaNoviOdgovor,OcjenaKorisnika,KomentarOcjene,PodrskaKorisnikId")] Podrska podrska)
        {
            if (id != podrska.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    var originalPodrska = await _context.Podrska.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

                    bool newResponseAdded = !string.IsNullOrEmpty(podrska.Odgovor) &&
                                           (originalPodrska == null || string.IsNullOrEmpty(originalPodrska.Odgovor) ||
                                            originalPodrska.Odgovor != podrska.Odgovor);

                    if (newResponseAdded)
                    {
                        podrska.DatumOdgovora = DateOnly.FromDateTime(DateTime.Now);
                        podrska.PodrskaKorisnikId = currentUser.Id;
                        podrska.ImaNoviOdgovor = true;
                    }
                    else
                    {
                        podrska.DatumOdgovora = originalPodrska?.DatumOdgovora;
                        podrska.PodrskaKorisnikId = originalPodrska?.PodrskaKorisnikId;
                        if (originalPodrska != null && !newResponseAdded)
                        {
                            podrska.ImaNoviOdgovor = originalPodrska.ImaNoviOdgovor;
                        }
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

            if (ocjena < 1 || ocjena > 5)
            {
                TempData["Error"] = "Ocjena mora biti između 1 i 5 zvjezdica.";
                return RedirectToAction(nameof(Details), new { id });
            }

            podrska.OcjenaKorisnika = ocjena;
            podrska.KomentarOcjene = komentar;

            _context.Update(podrska);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hvala vam na ocjeni! Vaša povratna informacija nam je važna.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Zatvori(int id)
        {
            var podrska = await _context.Podrska.FindAsync(id);
            if (podrska == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            if (!User.IsInRole("Podrska") && !User.IsInRole("Administrator") && podrska.KorisnikId != currentUser.Id)
            {
                return Forbid();
            }

            podrska.Status = statusZahtjeva.Zatvoren;
            podrska.ImaNoviOdgovor = false;

            _context.Update(podrska);
            await _context.SaveChangesAsync();

            TempData["Info"] = "Zahtjev je uspješno zatvoren.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var podrska = await _context.Podrska.FirstOrDefaultAsync(m => m.Id == id);
            if (podrska == null) return NotFound();

            return View(podrska);
        }

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

        [Authorize(Roles = "Podrska,Administrator")]
        public async Task<IActionResult> Statistike()
        {
            try
            {
                var stats = new
                {
                    UkupnoZahtjeva = await _context.Podrska.CountAsync(),
                    OtvoreniZahtjevi = await _context.Podrska.CountAsync(p => p.Status == statusZahtjeva.Otvoren),
                    ZahtjeviUObradi = await _context.Podrska.CountAsync(p => p.Status == statusZahtjeva.UObradi),
                    ZatvorenikZahtjevi = await _context.Podrska.CountAsync(p => p.Status == statusZahtjeva.Zatvoren),
                    KriticniZahtjevi = await _context.Podrska.CountAsync(p => p.Hitnost == NivoHitnosti.Kriticna && p.Status != statusZahtjeva.Zatvoren),
                    ProsjecnaOcjena = await _context.Podrska.Where(p => p.OcjenaKorisnika.HasValue).AnyAsync()
                        ? await _context.Podrska.Where(p => p.OcjenaKorisnika.HasValue).AverageAsync(p => p.OcjenaKorisnika.Value)
                        : 0.0
                };

                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Došlo je do greške prilikom učitavanja statistika.";
                return RedirectToAction(nameof(Index));
            }
        }

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