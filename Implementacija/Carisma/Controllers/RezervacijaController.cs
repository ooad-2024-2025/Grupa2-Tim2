using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Carisma.Data;
using Carisma.Models;
using Microsoft.AspNetCore.Authorization;


namespace Carisma.Controllers
{
    [Authorize]
    public class RezervacijaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RezervacijaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rezervacija
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Rezervacija.Include(r => r.korisnik).Include(r => r.vozilo);
            return View(await applicationDbContext.ToListAsync());
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

            return View(rezervacija);
        }

        // GET: Rezervacija/Create
        public IActionResult Create()
        {
            ViewData["korisnikId"] = new SelectList(_context.Osoba, "Id", "Id");
            ViewData["voziloId"] = new SelectList(_context.Vozila, "Id", "Marka");
            return View();
        }

        // POST: Rezervacija/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,datumPocetka,datumZavrsetka,datumRezervacije,Status,cijena,korisnikId,voziloId")] Rezervacija rezervacija)
        {
            if (ModelState.IsValid)
           {
                _context.Add(rezervacija);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["korisnikId"] = new SelectList(_context.Osoba, "Id", "Id", rezervacija.korisnikId);
            ViewData["voziloId"] = new SelectList(_context.Vozila, "Id", "Marka", rezervacija.voziloId);
            return View(rezervacija);
        }

        // GET: Rezervacija/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.Rezervacija.FindAsync(id);
            if (rezervacija == null)
            {
                return NotFound();
            }
            ViewData["korisnikId"] = new SelectList(_context.Osoba, "Id", "Id", rezervacija.korisnikId);
            ViewData["voziloId"] = new SelectList(_context.Vozila, "Id", "Marka", rezervacija.voziloId);
            return View(rezervacija);
        }

        // POST: Rezervacija/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,datumPocetka,datumZavrsetka,datumRezervacije,Status,cijena,korisnikId,voziloId")] Rezervacija rezervacija)
        {
            if (id != rezervacija.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
           {
                try
                {
                    _context.Update(rezervacija);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RezervacijaExists(rezervacija.Id))
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
            ViewData["korisnikId"] = new SelectList(_context.Osoba, "Id", "Id", rezervacija.korisnikId);
            ViewData["voziloId"] = new SelectList(_context.Vozila, "Id", "Marka", rezervacija.voziloId);
            return View(rezervacija);
        }

        // GET: Rezervacija/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(rezervacija);
        }

        // POST: Rezervacija/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rezervacija = await _context.Rezervacija.FindAsync(id);
            if (rezervacija != null)
            {
                _context.Rezervacija.Remove(rezervacija);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Rezervacija/Potvrdi/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Potvrdi(int id)
        {
            var rezervacija = await _context.Rezervacija.FindAsync(id);
            if (rezervacija == null)
                return NotFound();

            if (rezervacija.potvrdiRezervaciju())
            {
                _context.Update(rezervacija);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Rezervacija/Otkazi/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Otkazi(int id)
        {
            var rezervacija = await _context.Rezervacija.FindAsync(id);
            if (rezervacija == null)
                return NotFound();

            if (rezervacija.otkaziRezervaciju())
            {
                _context.Update(rezervacija);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Rezervacija/IzracunajCijenu/5
        public async Task<IActionResult> IzracunajCijenu(int? id)
        {
            if (id == null)
                return NotFound();

            var rezervacija = await _context.Rezervacija
                .Include(r => r.vozilo)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rezervacija == null)
                return NotFound();

            var cijenaPoDanu = rezervacija.vozilo?.CijenaPoDanu ?? 0;
            rezervacija.izracunajCijenu(cijenaPoDanu);

            _context.Update(rezervacija);
            await _context.SaveChangesAsync();

            TempData["poruka"] = $"Cijena je izračunata: {rezervacija.cijena} KM";
            return RedirectToAction(nameof(Details), new { id = rezervacija.Id });
        }

        // GET: Rezervacija/KreirajAutomatski
        public IActionResult KreirajAutomatski()
        {
            ViewData["korisnikId"] = new SelectList(_context.Osoba, "Id", "ImePrezime");
            ViewData["voziloId"] = new SelectList(_context.Vozila, "Id", "Marka");
            return View();
        }

        // POST: Rezervacija/KreirajAutomatski
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KreirajAutomatski(int korisnikId, int voziloId)
        {
            var korisnik = await _context.Osoba.FindAsync(korisnikId);
            var vozilo = await _context.Vozila.FindAsync(voziloId);

            if (korisnik == null || vozilo == null)
                return NotFound();

            var nova = Rezervacija.rezervisiVozilo(vozilo, DateTime.Now, korisnik);

            _context.Rezervacija.Add(nova);
            await _context.SaveChangesAsync();

            TempData["poruka"] = "Rezervacija je uspješno kreirana.";
            return RedirectToAction(nameof(Index));
        }


        private bool RezervacijaExists(int id)
        {
            return _context.Rezervacija.Any(e => e.Id == id);
        }
    }
}