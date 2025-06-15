using Carisma.Data;
using Carisma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Carisma.Controllers
{
    public class RezervacijaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RezervacijaController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Rezervacija - Korisničke rezervacije
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var osoba = await _context.Osoba.FirstOrDefaultAsync(o => o.IdentityUserId == currentUser.Id);

            if (osoba == null)
            {
                ViewBag.Message = "Možda vas zanimaju ovi automobili?";
                var dostupnaVozila = await _context.Vozila
                    .Where(v => v.Status == Dostupnost.Dostupno)
                    .Take(5)
                    .ToListAsync();
                ViewBag.PredloziVozila = dostupnaVozila;
                return View(new List<Rezervacija>());
            }

            var rezervacije = await _context.Rezervacija
                .Include(r => r.vozilo)
                .Where(r => r.korisnikId == osoba.Id)
                .OrderByDescending(r => r.datumRezervacije)
                .ToListAsync();

            if (!rezervacije.Any())
            {
                ViewBag.Message = "Možda vas zanimaju ovi automobili?";
                var dostupnaVozila = await _context.Vozila
                    .Where(v => v.Status == Dostupnost.Dostupno)
                    .Take(5)
                    .ToListAsync();
                ViewBag.PredloziVozila = dostupnaVozila;
            }

            return View(rezervacije);
        }

        // GET: Rezervacija/AdminIndex - Sve rezervacije za administratore
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AdminIndex()
        {
            var rezervacije = await _context.Rezervacija
                .Include(r => r.korisnik)
                .Include(r => r.vozilo)
                .OrderByDescending(r => r.datumRezervacije)
                .ToListAsync();
            return View(rezervacije);
        }

        // GET: Rezervacija/Kreiraj/5 - vozilo ID
        public async Task<IActionResult> Kreiraj(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vozilo = await _context.Vozila.FindAsync(id);
            if (vozilo == null)
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated)
            {
                TempData["VoziloZaRezervaciju"] = id;
                TempData["Poruka"] = "Morate se registrovati ili prijaviti da biste mogli rezervisati vozilo.";
                return RedirectToAction("Register", "Account");
            }

            var model = new RezervacijaViewModel
            {
                Vozilo = vozilo,
                DatumPocetka = DateTime.Today.AddDays(1),
                DatumZavrsetka = DateTime.Today.AddDays(2)
            };

            return View(model);
        }

        // POST: Rezervacija/Kreiraj
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Kreiraj(RezervacijaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Vozilo = await _context.Vozila.FindAsync(model.VoziloId);
                return View(model);
            }

            // Validacija datuma
            if (model.DatumPocetka < DateTime.Today)
            {
                ModelState.AddModelError("DatumPocetka", "Datum početka ne može biti u prošlosti.");
                model.Vozilo = await _context.Vozila.FindAsync(model.VoziloId);
                return View(model);
            }

            if (model.DatumZavrsetka <= model.DatumPocetka)
            {
                ModelState.AddModelError("DatumZavrsetka", "Datum završetka mora biti nakon datuma početka.");
                model.Vozilo = await _context.Vozila.FindAsync(model.VoziloId);
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var osoba = await _context.Osoba.FirstOrDefaultAsync(o => o.IdentityUserId == currentUser.Id);
            var vozilo = await _context.Vozila.FindAsync(model.VoziloId);

            if (osoba == null || vozilo == null)
            {
                return NotFound();
            }

            var rezervacija = new Rezervacija
            {
                datumPocetka = model.DatumPocetka,
                datumZavrsetka = model.DatumZavrsetka,
                datumRezervacije = DateTime.Now,
                Status = StatusRezervacije.Kreirana,
                korisnikId = osoba.Id,
                voziloId = vozilo.Id,
                vozilo = vozilo,
                korisnik = osoba
            };

            // Izračunaj cijenu
            rezervacija.izracunajCijenu(vozilo.CijenaPoDanu);

            _context.Rezervacija.Add(rezervacija);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezervacija je uspješno kreirana!";
            return RedirectToAction("Placanje", new { id = rezervacija.Id });
        }

        // GET: Rezervacija/Placanje/5
        [Authorize]
        public async Task<IActionResult> Placanje(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.Rezervacija
                .Include(r => r.vozilo)
                .Include(r => r.korisnik)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rezervacija == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var osoba = await _context.Osoba.FirstOrDefaultAsync(o => o.IdentityUserId == currentUser.Id);

            if (rezervacija.korisnikId != osoba?.Id)
            {
                return Forbid();
            }

            return View(rezervacija);
        }

        // POST: Rezervacija/PotvrdiPlacanje/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> PotvrdiPlacanje(int id, bool prihvatamOdgovornost)
        {
            if (!prihvatamOdgovornost)
            {
                TempData["Error"] = "Morate prihvatiti odgovornost za štetu da biste završili rezervaciju.";
                return RedirectToAction("Placanje", new { id });
            }

            var rezervacija = await _context.Rezervacija.FindAsync(id);
            if (rezervacija == null)
            {
                return NotFound();
            }

            rezervacija.Status = StatusRezervacije.UToku;
            _context.Update(rezervacija);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezervacija je uspješno potvrđena! Možete preuzeti vozilo na dan početka rezervacije.";
            return RedirectToAction("Index");
        }

        // POST: Rezervacija/Otkazi/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Otkazi(int id)
        {
            var rezervacija = await _context.Rezervacija
                .Include(r => r.korisnik)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rezervacija == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var osoba = await _context.Osoba.FirstOrDefaultAsync(o => o.IdentityUserId == currentUser.Id);

            if (rezervacija.korisnikId != osoba?.Id && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            // Provjera 48h pravila
            var razlika = rezervacija.datumPocetka - DateTime.Now;
            if (razlika.TotalHours < 48 && !User.IsInRole("Administrator"))
            {
                TempData["Error"] = "Ne možete otkazati rezervaciju manje od 48 sati prije početka.";
                return RedirectToAction("Index");
            }

            rezervacija.Status = StatusRezervacije.Otkazana;
            _context.Update(rezervacija);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezervacija je uspješno otkazana.";
            return RedirectToAction("Index");
        }

        // GET: Rezervacija/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.Rezervacija
                .Include(r => r.korisnik)
                .Include(r => r.vozilo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rezervacija == null)
            {
                return NotFound();
            }

            // Provjeri dozvole
            if (User.IsInRole("Administrator"))
            {
                return View(rezervacija);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var osoba = await _context.Osoba.FirstOrDefaultAsync(o => o.IdentityUserId == currentUser.Id);

            if (rezervacija.korisnikId != osoba?.Id)
            {
                return Forbid();
            }

            return View(rezervacija);
        }

        private bool RezervacijaExists(int id)
        {
            return _context.Rezervacija.Any(e => e.Id == id);
        }
    }

    // ViewModel za kreiranje rezervacije
    public class RezervacijaViewModel
    {
        public int VoziloId { get; set; }
        public Vozilo Vozilo { get; set; }

        [Required(ErrorMessage = "Datum početka je obavezan.")]
        [Display(Name = "Datum početka")]
        public DateTime DatumPocetka { get; set; }

        [Required(ErrorMessage = "Datum završetka je obavezan.")]
        [Display(Name = "Datum završetka")]
        public DateTime DatumZavrsetka { get; set; }

        public bool PrihvatamOdgovornost { get; set; }

        public double UkupnaCijena
        {
            get
            {
                if (Vozilo != null && DatumZavrsetka > DatumPocetka)
                {
                    int brojDana = (DatumZavrsetka.Date - DatumPocetka.Date).Days + 1;
                    return brojDana * Vozilo.CijenaPoDanu;
                }
                return 0;
            }
        }

        public int BrojDana
        {
            get
            {
                if (DatumZavrsetka > DatumPocetka)
                {
                    return (DatumZavrsetka.Date - DatumPocetka.Date).Days + 1;
                }
                return 0;
            }
        }
    }
}