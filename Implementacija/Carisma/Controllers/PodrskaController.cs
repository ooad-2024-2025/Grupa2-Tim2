using Carisma.Data;
using Carisma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Carisma.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> Create([Bind("Pitanje")] Podrska podrska)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");  // Preusmjeri na login ako korisnik nije prijavljen
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var korisnik = await _context.Osoba.FirstOrDefaultAsync(o => o.korisnicko_ime == userId);

            if (korisnik == null)
            {
                TempData["Error"] = "Niste registrovani u sistemu!";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                podrska.Korisnik = korisnik;
                podrska.Status = statusZahtjeva.Otvoren;
                podrska.Odgovor = " ";
                podrska.DatumPostavljanja = DateTime.Now;

                _context.Add(podrska);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(podrska);
        }

        [HttpPost]

        public async Task<IActionResult> podrskaKorisniku(string pitanje)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || string.IsNullOrWhiteSpace(pitanje))
            {
                return BadRequest("Neispravan zahtjev.");
            }

            var korisnik = await _context.Osoba.FirstOrDefaultAsync(o => o.korisnicko_ime == userId);
            if (korisnik == null)
            {
                return Unauthorized();
            }

            var noviZahtev = new Podrska
            {
                KorisnikId = korisnik.Id,
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
