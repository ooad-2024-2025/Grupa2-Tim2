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
    public class PlacanjeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlacanjeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Placanje
        public async Task<IActionResult> Index()
        {
            return View(await _context.Placanje.ToListAsync());
        }

        // GET: Placanje/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var placanje = await _context.Placanje
                .FirstOrDefaultAsync(m => m.Id == id);
            if (placanje == null)
            {
                return NotFound();
            }

            return View(placanje);
        }

        // GET: Placanje/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Placanje/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,iznos,datumPlacanja,statusPlacanja,brojKartice")] Placanje placanje)
        {
            if (ModelState.IsValid)
            {
                _context.Add(placanje);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(placanje);
        }

        // GET: Placanje/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var placanje = await _context.Placanje.FindAsync(id);
            if (placanje == null)
            {
                return NotFound();
            }
            return View(placanje);
        }

        // POST: Placanje/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,iznos,datumPlacanja,statusPlacanja,brojKartice")] Placanje placanje)
        {
            if (id != placanje.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(placanje);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlacanjeExists(placanje.Id))
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
            return View(placanje);
        }

        // GET: Placanje/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var placanje = await _context.Placanje
                .FirstOrDefaultAsync(m => m.Id == id);
            if (placanje == null)
            {
                return NotFound();
            }

            return View(placanje);
        }

        // POST: Placanje/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var placanje = await _context.Placanje.FindAsync(id);
            if (placanje != null)
            {
                _context.Placanje.Remove(placanje);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Placanje/ValidirajKarticu/5
        public IActionResult ValidirajKarticu(int id)
        {
            var placanje = _context.Placanje.FirstOrDefault(p => p.Id == id);
            if (placanje == null) return NotFound();

            bool validna = placanje.validirajKarticu();
            ViewBag.Rezultat = validna ? "Kartica je validna." : "Kartica nije validna.";
            return View("RezultatValidacije");
        }

        // GET: Placanje/ProcesirajPlacanje/5
        public IActionResult ProcesirajPlacanje(int id)
        {
            var placanje = _context.Placanje.FirstOrDefault(p => p.Id == id);
            if (placanje == null) return NotFound();

            var status = placanje.procesirajPlacanje();
            ViewBag.Rezultat = $"Status plaćanja: {status}";
            return View("RezultatObrade");
        }

        // GET: Placanje/GenerisiPotvrdu/5
        public IActionResult GenerisiPotvrdu(int id)
        {
            var placanje = _context.Placanje.FirstOrDefault(p => p.Id == id);
            if (placanje == null) return NotFound();

            string potvrda = placanje.generisiPotvrdu();
            ViewBag.Potvrda = string.IsNullOrEmpty(potvrda) ? "Potvrda nije generisana." : potvrda;
            return View("PotvrdaPlacanja");
        }

        // GET: Placanje/PlatiOnline/5
        public IActionResult PlatiOnline(int id)
        {
            var placanje = _context.Placanje.FirstOrDefault(p => p.Id == id);
            if (placanje == null) return NotFound();

            bool uspjesno = placanje.platiOnline(placanje.iznos);
            ViewBag.Rezultat = uspjesno ? "Plaćanje je uspješno obavljeno." : "Plaćanje nije uspjelo.";
            return View("RezultatPlacanja");
        }


        private bool PlacanjeExists(int id)
        {
            return _context.Placanje.Any(e => e.Id == id);
        }
    }
}