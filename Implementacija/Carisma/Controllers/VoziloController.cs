using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Carisma.Data;
using Carisma.Models;


namespace Carisma.Controllers
{
    public class VoziloController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VoziloController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vozilo
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Vozila.Include(v => v.Osoba);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Vozilo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vozilo = await _context.Vozila
                .Include(v => v.Osoba)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vozilo == null)
            {
                return NotFound();
            }

            return View(vozilo);
        }

        // GET: Vozilo/Create
        public IActionResult Create()
        {
            ViewData["OsobaId"] = new SelectList(_context.Osoba, "Id", "Id");
            return View();
        }

        // POST: Vozilo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Marka,Model,Godina,CijenaPoDanu,OsobaId,Status")] Vozilo vozilo)
        {
           // if (ModelState.IsValid)
          //  {
                _context.Add(vozilo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
           // }
            ViewData["OsobaId"] = new SelectList(_context.Osoba, "Id", "Id", vozilo.OsobaId);
            return View(vozilo);
        }

        // GET: Vozilo/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
            ViewData["OsobaId"] = new SelectList(_context.Osoba, "Id", "Id", vozilo.OsobaId);
            return View(vozilo);
        }

        // POST: Vozilo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Marka,Model,Godina,CijenaPoDanu,OsobaId,Status")] Vozilo vozilo)
        {
            if (id != vozilo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vozilo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoziloExists(vozilo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["OsobaId"] = new SelectList(_context.Osoba, "Id", "Id", vozilo.OsobaId);
            return View(vozilo);
        }

        // GET: Vozilo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vozilo = await _context.Vozila
                .Include(v => v.Osoba)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vozilo == null)
            {
                return NotFound();
            }

            return View(vozilo);
        }

        // POST: Vozilo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vozilo = await _context.Vozila.FindAsync(id);
            if (vozilo != null)
            {
                _context.Vozila.Remove(vozilo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        //
        // GET: Osoba/JeDostupno/5
        public async Task<IActionResult> JeDostupno(int id)
        {
            var osoba = await _context.Osoba.FindAsync(id);
            if (osoba == null)
            {
                return NotFound();
            }

            // Ovde bi trebalo proveriti kriterijum dostupnosti (npr. da li je blokiran)
            bool dostupna = !osoba.blokiran;

            // Vrati kao JSON (za AJAX), ili kao View ako praviš poseban ekran
            return Json(new { dostupna = dostupna });
        }

        // GET: Vozilo/Rezervisi/5
        public async Task<IActionResult> Rezervisi(int? id)
        {
            if (id == null)
                return NotFound();

            var vozilo = await _context.Vozila.FindAsync(id);
            if (vozilo == null)
                return NotFound();

            return View(new Rezervacija { vozilo = vozilo });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rezervisi(int id, DateTime datumRezervacije)
        {
            var vozilo = await _context.Vozila.FindAsync(id);
            if (vozilo == null)
                return NotFound();

            // Pronađi trenutno ulogovanog korisnika (ili privremeno hardkodiraj ako nemaš autentikaciju)
            var korisnik = await _context.Osoba.FirstOrDefaultAsync(); // ← zameni sa pravim korisnikom!

            var novaRezervacija = new Rezervacija
            {
                vozilo = vozilo,
                korisnik = korisnik,
                datumRezervacije = datumRezervacije,
                Status = StatusRezervacije.UToku, // ili tvoj default status
                cijena = vozilo.CijenaPoDanu
            };

            _context.Rezervacija.Add(novaRezervacija);
            await _context.SaveChangesAsync();

            TempData["Poruka"] = "Vozilo uspešno rezervisano.";
            return RedirectToAction("Index", "Vozilo");
        }

        // PUT ili POST: Vozilo/AzurirajVozilo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AzurirajVozilo(int id, [Bind("Id,Marka,Model,Godina,CijenaPoDanu,OsobaId,Status")] Vozilo novoVozilo)
        {
            if (id != novoVozilo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(novoVozilo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoziloExists(novoVozilo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)); // ili gde treba
            }
            return View(novoVozilo);
        }


        // GET: Vozilo/Pretrazi?pojam=nekiTekst
        public async Task<IActionResult> Pretrazi(string pojam)
        {
            if (string.IsNullOrEmpty(pojam))
            {
                // Ako nema pojma, vrati sve ili prazan rezultat
                return View(await _context.Vozila.ToListAsync());
            }

            // Pretraga po marki ili modelu, delimično podudaranje (Contains)
            var rezultat = await _context.Vozila
                .Where(v => v.Marka.Contains(pojam) || v.Model.Contains(pojam))
                .ToListAsync();

            return View(rezultat);
        }

        //

        private bool VoziloExists(int id)
        {
            return _context.Vozila.Any(e => e.Id == id);
        }
    }
}
