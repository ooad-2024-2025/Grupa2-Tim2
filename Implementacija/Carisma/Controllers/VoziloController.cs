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
using Microsoft.AspNetCore.Identity;


namespace Carisma.Controllers
{
    
    public class VoziloController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public VoziloController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

       

        //novi za sortiranje
        public async Task<IActionResult> Index(string? sortOrder = null, string? pojam = null)
        {
            ViewData["CijenaSortParm"] = sortOrder == "cijena_asc" ? "cijena_desc" : "cijena_asc";
            ViewData["CurrentSort"] = sortOrder;
            ViewData["SearchString"] = pojam; // Dodano za čuvanje pretrage u view-u

            var vozila = from v in _context.Vozila.Include(v => v.Osoba)
                         select v;

            // Pretraga po marki ili modelu
            if (!string.IsNullOrEmpty(pojam))
            {
                
                string searchTerm = pojam.Trim().ToLower();

                vozila = vozila.Where(v =>
                    v.Marka.ToLower().Contains(searchTerm) ||
                    v.Model.ToLower().Contains(searchTerm) ||
                    (v.Marka + " " + v.Model).ToLower().Contains(searchTerm));
            }

            // Sortiranje
            switch (sortOrder)
            {
                case "cijena_desc":
                    vozila = vozila.OrderByDescending(v => v.CijenaPoDanu);
                    break;
                case "cijena_asc":
                    vozila = vozila.OrderBy(v => v.CijenaPoDanu);
                    break;
                default:
                    vozila = vozila.OrderBy(v => v.Id); // Default sortiranje
                    break;
            }

            // Debug informacije
            var rezultat = await vozila.ToListAsync();

            

            return View(rezultat);
        }

        // GET: Vozilo/Details/5
        
        //[Authorize(Roles = "Administrator, Korisnik")]
        
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

        //
        [Authorize(Roles = "Administrator")]
        //
        public IActionResult Create()
        {
            ViewData["OsobaId"] = new SelectList(_context.Osoba, "Id", "Id");
            return View();
        }

        // POST: Vozilo/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Marka,Model,Godina,CijenaPoDanu,OsobaId,Status")] Vozilo vozilo)
        {
            //if (ModelState.IsValid)
            //{
                _context.Add(vozilo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            //}
            ViewData["OsobaId"] = new SelectList(_context.Osoba, "Id", "Id", vozilo.OsobaId);
            return View(vozilo);
        }

        // GET: Vozilo/Edit/5

        [Authorize(Roles ="Administrator")]
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

            // Kreiranje SelectList sa svim osobama
            ViewData["OsobaId"] = new SelectList(_context.Osoba, "Id", "Id", vozilo.OsobaId);

            return View(vozilo);
        }

        // POST: Vozilo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Marka,Model,Godina,CijenaPoDanu,OsobaId,Status")] Vozilo vozilo)
        {
            if (id != vozilo.Id)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
                try
                {
                    _context.Update(vozilo);
                    await _context.SaveChangesAsync();
                    TempData["Poruka"] = "Vozilo je uspješno izmijenjeno!";
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
           // }

            
            ViewData["OsobaId"] = new SelectList(_context.Osoba, "Id", "Id", vozilo.OsobaId);

            return View(vozilo);
        }

        // GET: Vozilo/Delete/5


        [Authorize(Roles = "Administrator")]
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
        [Authorize(Roles = "Administrator")]
        
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

            if (vozilo.Status != Dostupnost.Dostupno)
            {
                TempData["Error"] = "Ovo vozilo trenutno nije dostupno za rezervaciju.";
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction("Kreiraj", "Rezervacija", new { id });
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
                return RedirectToAction(nameof(Index)); 
            }
            return View(novoVozilo);
        }



        private bool VoziloExists(int id)
        {
            return _context.Vozila.Any(e => e.Id == id);
        }
    }
}
