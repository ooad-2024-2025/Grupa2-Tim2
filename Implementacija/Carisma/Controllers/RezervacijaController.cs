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
using Stripe;
using Stripe.Checkout;
using System.Configuration;

namespace Carisma.Controllers
{
    public class RezervacijaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public RezervacijaController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;

            // Postavi Stripe konfiguraciju
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        // GET: Rezervacija - Korisničke rezervacije
        [Authorize(Roles = "Korisnik")]
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
                VoziloId = vozilo.Id, // DODATO: Postavi VoziloId
                DatumPocetka = DateTime.Today.AddDays(1),
                DatumZavrsetka = DateTime.Today.AddDays(2)
            };

            return View(model);
        }

        // POST: Rezervacija/Kreiraj - DIREKTNO KREIRA STRIPE SESIJU
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Kreiraj(RezervacijaViewModel model)
        {
            System.IO.File.AppendAllText("debug.txt", $"AKCIJA POZVANA: {DateTime.Now}\n");
            Console.WriteLine("=== KREIRANJE REZERVACIJE ===");
            Console.WriteLine("=== KREIRANJE REZERVACIJE ===");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"VoziloId: {model.VoziloId}");
            Console.WriteLine($"DatumPocetka: {model.DatumPocetka}");
            Console.WriteLine($"DatumZavrsetka: {model.DatumZavrsetka}");
            Console.WriteLine($"PrihvatamOdgovornost: {model.PrihvatamOdgovornost}");

            // Učitaj vozilo ako nije učitano
            if (model.Vozilo == null)
            {
                model.Vozilo = await _context.Vozila.FindAsync(model.VoziloId);
            }

          

            // Validacija datuma
            if (model.DatumPocetka < DateTime.Today)
            {
                ModelState.AddModelError("DatumPocetka", "Datum početka ne može biti u prošlosti.");
                return View(model);
            }

            if (model.DatumZavrsetka <= model.DatumPocetka)
            {
                ModelState.AddModelError("DatumZavrsetka", "Datum završetka mora biti nakon datuma početka.");
                return View(model);
            }

            // Provjeri da li je prihvaćena odgovornost
            if (!model.PrihvatamOdgovornost)
            {
                ModelState.AddModelError("PrihvatamOdgovornost", "Morate prihvatiti odgovornost za štete.");
                return View(model);
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var osoba = await _context.Osoba.FirstOrDefaultAsync(o => o.IdentityUserId == currentUser.Id);
                var vozilo = await _context.Vozila.FindAsync(model.VoziloId);

                if (osoba == null || vozilo == null)
                {
                    Console.WriteLine($"Osoba: {osoba?.Id}, Vozilo: {vozilo?.Id}");
                    return NotFound();
                }

                // Kreiraj rezervaciju
                var rezervacija = new Rezervacija
                {
                    datumPocetka = model.DatumPocetka,
                    datumZavrsetka = model.DatumZavrsetka,
                    datumRezervacije = DateTime.Now,
                    Status = StatusRezervacije.UToku, // Postavi na UToku dok se ne završi plaćanje
                    korisnikId = osoba.Id,
                    voziloId = vozilo.Id,
                    vozilo = vozilo,
                    korisnik = osoba
                };

                // Izračunaj cijenu
                rezervacija.izracunajCijenu();
                Console.WriteLine($"Ukupna cijena: {rezervacija.izracunajCijenu()}");

                _context.Rezervacija.Add(rezervacija);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Rezervacija kreirana sa ID: {rezervacija.Id}");

                // KREIRAJ STRIPE SESIJU DIREKTNO
                var domain = $"{Request.Scheme}://{Request.Host}";
                Console.WriteLine($"Domain: {domain}");

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(rezervacija.izracunajCijenu() * 100), // Stripe koristi cente
                                Currency = "eur", // Ili "usd" - provjerite šta podržava vaš Stripe account
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Rezervacija - {vozilo.Marka} {vozilo.Model}",
                                    Description = $"Od {model.DatumPocetka:dd.MM.yyyy} do {model.DatumZavrsetka:dd.MM.yyyy}"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = $"{domain}/Rezervacija/StripeSuccess?session_id={{CHECKOUT_SESSION_ID}}&rezervacija_id={rezervacija.Id}",
                    CancelUrl = $"{domain}/Rezervacija/StripeCancel?rezervacija_id={rezervacija.Id}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "rezervacija_id", rezervacija.Id.ToString() },
                        { "user_id", osoba.Id.ToString() }
                    }
                };

                Console.WriteLine("Kreiranje Stripe sesije...");
                var service = new SessionService();
                var session = await service.CreateAsync(options);

                Console.WriteLine($"Stripe sesija kreirana: {session.Id}");
                Console.WriteLine($"Preusmjeravanje na: {session.Url}");

                return Redirect(session.Url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GREŠKA: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                ModelState.AddModelError("", $"Došlo je do greške: {ex.Message}");
                return View(model);
            }
        }

        // GET: Rezervacija/Placanje/5 - MOŽDA VIŠE NEĆE BITI POTREBNO
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

        // Uspješno plaćanje
        [Authorize]
public async Task<IActionResult> StripeSuccess(string session_id, int rezervacija_id)
        {
            Console.WriteLine($"=== STRIPE SUCCESS ===");
            Console.WriteLine($"Session ID: {session_id}");
            Console.WriteLine($"Rezervacija ID: {rezervacija_id}");

            var rezervacija = await _context.Rezervacija
                .Include(r => r.vozilo)
                .Include(r => r.korisnik)
                .FirstOrDefaultAsync(r => r.Id == rezervacija_id);

            if (rezervacija == null)
            {
                Console.WriteLine("Rezervacija nije pronađena");
                return NotFound();
            }

            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(session_id);
                Console.WriteLine($"Stripe session status: {session.PaymentStatus}");

                if (session.PaymentStatus == "paid")
                {
                    // Kreiraj plaćanje record
                    var placanje = new Placanje
                    {
                        iznos = rezervacija.izracunajCijenu(),
                        datumPlacanja = DateTime.Now,
                        statusPlacanja = StatusPlacanja.Uspjesno,
                        korisnikId = rezervacija.korisnik.Id, // Foreign key za korisnika
                        //brojKartice = GetMaskedCardNumber(session), // Broj kartice (nullable)
                        RezervacijaId = rezervacija.Id, // Foreign key za rezervaciju (nullable)
                        StripeSessionId = session_id, // Stripe session ID
                        StripePaymentIntentId = session.PaymentIntentId // Stripe payment intent ID
                    };

                    _context.Placanje.Add(placanje);

                    // Ažuriraj rezervaciju na potvrđenu
                    rezervacija.Status = StatusRezervacije.Kreirana;
                    _context.Update(rezervacija);

                    await _context.SaveChangesAsync();

                    Console.WriteLine("Plaćanje uspješno zabilježeno");
                    TempData["Success"] = "Plaćanje je uspješno završeno! Rezervacija je potvrđena.";
                    ViewBag.SessionId = session_id;
                    ViewBag.Amount = rezervacija.izracunajCijenu();
                }
                else
                {
                    Console.WriteLine("Plaćanje nije uspješno");
                    TempData["Error"] = "Plaćanje nije uspješno završeno.";

                    // Otkažite rezervaciju ako plaćanje nije uspješno
                    rezervacija.Status = StatusRezervacije.Otkazana;
                    _context.Update(rezervacija);
                    await _context.SaveChangesAsync();
                }
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Stripe greška: {ex.Message}");
                TempData["Error"] = $"Greška pri obradi plaćanja: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Općenita greška: {ex.Message}");
                TempData["Error"] = "Došlo je do greške pri obradi zahtjeva.";
            }

            return View(rezervacija);
        }

       

        // Otkazano plaćanje
        [Authorize]
        public async Task<IActionResult> StripeCancel(int rezervacija_id)
        {
            Console.WriteLine($"=== STRIPE CANCEL ===");
            Console.WriteLine($"Rezervacija ID: {rezervacija_id}");

            var rezervacija = await _context.Rezervacija
                .Include(r => r.vozilo)
                .FirstOrDefaultAsync(r => r.Id == rezervacija_id);

            if (rezervacija == null) return NotFound();

            // Otkažite rezervaciju
            rezervacija.Status = StatusRezervacije.Otkazana;
            _context.Update(rezervacija);
            await _context.SaveChangesAsync();

            TempData["Error"] = "Plaćanje je otkazano. Rezervacija je otkazana.";
            return RedirectToAction("Index");
        }

        // Ostatak metoda ostaje isti...
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

            // Preusmjerenje zavisno od uloge
            if (User.IsInRole("Administrator"))
            {
                return RedirectToAction("AdminIndex");
            }
            else
            {
                return RedirectToAction("Index");
            }
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

        [Required(ErrorMessage = "Morate prihvatiti odgovornost.")]
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