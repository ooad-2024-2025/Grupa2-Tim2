using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Carisma.Data;
using Carisma.Models;
using System;


namespace Carisma.Controllers
{
    public class PodrskaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PodrskaController(ApplicationDbContext context)
        {
            _context = context;
        }




        // GET: Podrska
        public async Task<IActionResult> Index()
        {
            return View(await _context.Podrska.ToListAsync());
        }

        // GET: Podrska/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var podrska = await _context.Podrska
                .FirstOrDefaultAsync(m => m.Id == id);
            if (podrska == null)
            {
                return NotFound();
            }

            return View(podrska);
        }

        // GET: Podrska/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Podrska/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Pitanje,DatumPostavljanja,Odgovor,Status,DatumOdgovora")] Podrska podrska)
        {
            //////////////////////SLEDECE cetiri LINIJE NAKNADNO(NISU BILE TU)
            ///

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                // Niko nije prijavljen, preusmjeri ili vrati grešku
                return Unauthorized();
            }

            // Pronađi Osoba entitet koji ima taj Identity korisnički ID
            var korisnik = await _context.Osoba.FirstOrDefaultAsync(o => o.korisnicko_ime== userId);

            if (korisnik == null)
            {
                // Prijavljeni korisnik nije pronađen u Osoba tabeli - možda nije registrovan?
                return Unauthorized();
            }

            // Postavi korisnika u zahtev za podršku
            podrska.Korisnik = korisnik;

            podrska.Status = statusZahtjeva.Otvoren;
            podrska.Odgovor = " ";
            podrska.DatumPostavljanja = DateTime.Now;
            ///////////////do ovde sam dodavala!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            if (ModelState.IsValid)
            {
                _context.Add(podrska);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(podrska);
        }


        [HttpPost]
        
        public async Task<IActionResult> podrskaKorisniku(Osoba korisnik, String pitanje)
        {
            if (korisnik==null || String.IsNullOrEmpty(pitanje))
            {
                return NotFound("Neispravan unos!");
            }

            var noviZahtev = new Podrska
            {
                Korisnik = korisnik,
                Pitanje = pitanje,
                DatumPostavljanja = DateTime.Now,
                Status = statusZahtjeva.Otvoren

            };

            _context.Podrska.Add(noviZahtev);
            await _context.SaveChangesAsync();


            evidentirajInterakciju();

            return Ok("Zahtjev je primljen");
            }

        private void evidentirajInterakciju()
        {
            var interakcija = new Podrska()
            {
                Pitanje = "Sistem: Nova interakcija evidentirana: ",
                DatumPostavljanja = DateTime.Now,
                Status = statusZahtjeva.UObradi
            };
            _context.Podrska.Add(interakcija);
            _context.SaveChanges();

        }
        

        // GET: Podrska/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var podrska = await _context.Podrska.FindAsync(id);
            if (podrska == null)
            {
                return NotFound();
            }
            return View(podrska);
        }


        // POST: Podrska/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Pitanje,DatumPostavljanja,Odgovor,Status,DatumOdgovora")] Podrska podrska)
        {
            if (id != podrska.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(podrska);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PodrskaExists(podrska.Id))
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
            return View(podrska);
        }




        // GET: Podrska/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var podrska = await _context.Podrska
                .FirstOrDefaultAsync(m => m.Id == id);
            if (podrska == null)
            {
                return NotFound();
            }

            return View(podrska);
        }

        // POST: Podrska/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var podrska = await _context.Podrska.FindAsync(id);
            if (podrska != null)
            {
                _context.Podrska.Remove(podrska);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PodrskaExists(int id)
        {
            return _context.Podrska.Any(e => e.Id == id);
        }
    }
}
