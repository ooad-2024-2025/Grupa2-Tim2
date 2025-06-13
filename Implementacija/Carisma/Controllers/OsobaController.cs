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
    public class OsobaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OsobaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Osoba
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Osoba.ToListAsync());
        }

        // GET: Osoba/Details/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var osoba = await _context.Osoba
                .FirstOrDefaultAsync(m => m.Id == id);
            if (osoba == null)
            {
                return NotFound();
            }

            return View(osoba);
        }

        // GET: Osoba/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Osoba/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Id,email,lozinka,broj_telefona,korisnicko_ime,uloga,blokiran")] Osoba osoba)
        {
            if (ModelState.IsValid)
            {
                _context.Add(osoba);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(osoba);
        }

        // GET: Osoba/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var osoba = await _context.Osoba.FindAsync(id);
            if (osoba == null)
            {
                return NotFound();
            }
            return View(osoba);
        }

        // POST: Osoba/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,email,lozinka,broj_telefona,korisnicko_ime,uloga,blokiran")] Osoba osoba)
        {
            if (id != osoba.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(osoba);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OsobaExists(osoba.Id))
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
            return View(osoba);
        }

        // GET: Osoba/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var osoba = await _context.Osoba
                .FirstOrDefaultAsync(m => m.Id == id);
            if (osoba == null)
            {
                return NotFound();
            }

            return View(osoba);
        }

        // POST: Osoba/Delete/5
        [Authorize(Roles = "Administrator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var osoba = await _context.Osoba.FindAsync(id);
            if (osoba != null)
            {
                _context.Osoba.Remove(osoba);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //registracija
        // GET: Osoba/Registracija
        public IActionResult Registracija()
        {
            return View(); // prikazuje formu
        }

        // POST: Osoba/Registracija
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registracija(Osoba novaOsoba)
        {
            if (ModelState.IsValid)
            {
                // Možeš dodatno proveriti da li već postoji korisnik s istim imenom ili email-om
                var postoji = _context.Osoba.Any(o => o.email == novaOsoba.email);
                if (postoji)
                {
                    ModelState.AddModelError("email", "Email već postoji.");
                    return View(novaOsoba);
                }

                _context.Osoba.Add(novaOsoba);
                _context.SaveChanges();

                TempData["Poruka"] = "Registracija uspešna za korisnika: " + novaOsoba.korisnicko_ime;
                return RedirectToAction("Index"); // ili gdje god želiš
            }

            return View(novaOsoba); // ako validacija ne prođe, ponovo prikaži formu s greškama
        }

        // POST: Osoba/Blokiraj/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Blokiraj(int id)
        {
            var korisnik = await _context.Osoba.FindAsync(id);
            if (korisnik == null)
            {
                TempData["Poruka"] = "Korisnik nije pronađen.";
                return RedirectToAction("Index");
            }

            korisnik.blokiran = true;
            await _context.SaveChangesAsync();

            TempData["Poruka"] = $"Korisnik '{korisnik.korisnicko_ime}' je blokiran.";
            return RedirectToAction("Index");
        }

        // GET: Osoba/Pregledaj/5
        public async Task<IActionResult> Pregledaj(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _context.Osoba
                .FirstOrDefaultAsync(o => o.Id == id);

            if (korisnik == null)
            {
                return NotFound();
            }

            Console.WriteLine("Pregled korisnika je pozvan."); // samo za test
            return View(korisnik); // Prikazuje View sa podacima o korisniku
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OdgovoriNaZahtjev(int idZahtjeva, string odgovor)
        {
            // Ovdje bi inače tražio zahtjev u bazi i ažurirao ga
            Console.WriteLine($"Odgovor na zahtjev: {idZahtjeva}: {odgovor}");

            TempData["Poruka"] = $"Odgovoreno na zahtjev {idZahtjeva}: {odgovor}";
            return RedirectToAction("Index");
        }

        //registracija
        private bool OsobaExists(int id)
        {
            return _context.Osoba.Any(e => e.Id == id);
        }


    }


}
