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

        // GET: Osoba - Prikaz svih korisnika
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var korisnici = await _context.Osoba.ToListAsync();
            return View(korisnici);
        }

        // GET: Osoba/Details/5 - Detalji korisnika
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Greska"] = "ID korisnika nije pronađen.";
                return RedirectToAction(nameof(Index));
            }

            var osoba = await _context.Osoba
                .FirstOrDefaultAsync(m => m.Id == id);
            if (osoba == null)
            {
                TempData["Greska"] = "Korisnik nije pronađen.";
                return RedirectToAction(nameof(Index));
            }

            return View(osoba);
        }

        // GET: Osoba/Edit/5 - Uređivanje korisnika
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Greska"] = "ID korisnika nije pronađen.";
                return RedirectToAction(nameof(Index));
            }

            var osoba = await _context.Osoba.FindAsync(id);
            if (osoba == null)
            {
                TempData["Greska"] = "Korisnik nije pronađen.";
                return RedirectToAction(nameof(Index));
            }

            
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)), osoba.uloga);

            return View(osoba);
        }

        // POST: Osoba/Edit/5 - Spremanje izmjena
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,email,broj_telefona,korisnicko_ime,uloga,blokiran")] Osoba osoba)
        {
            if (id != osoba.Id)
            {
                TempData["Greska"] = "Neispravni podaci korisnika.";
                return RedirectToAction(nameof(Index));
            }

            //if (ModelState.IsValid)
           // {
                try
                {
                    // Dohvati postojeći entitet
                    var postojeciKorisnik = await _context.Osoba.FindAsync(id);
                    if (postojeciKorisnik != null)
                    {
                        
                        postojeciKorisnik.email = osoba.email;
                        postojeciKorisnik.broj_telefona = osoba.broj_telefona;
                        postojeciKorisnik.korisnicko_ime = osoba.korisnicko_ime;
                        postojeciKorisnik.uloga = osoba.uloga;
                        postojeciKorisnik.blokiran = osoba.blokiran;

                        await _context.SaveChangesAsync();
                        TempData["Uspjeh"] = $"Korisnik '{osoba.korisnicko_ime}' je uspješno ažuriran.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Greska"] = "Korisnik nije pronađen.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OsobaExists(osoba.Id))
                    {
                        TempData["Greska"] = "Korisnik više ne postoji.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Greska"] = "Došlo je do greške prilikom spremanja. Molimo pokušajte ponovo.";
                        // Ponovno postavi ViewBag i vrati view
                        ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)), osoba.uloga);
                        return View(osoba);
                    }
                }
                catch (Exception ex)
                {
                    TempData["Greska"] = "Došlo je do neočekivane greške prilikom spremanja.";
                    ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)), osoba.uloga);
                    return View(osoba);
                }
            //}

            
            ViewBag.Uloge = new SelectList(Enum.GetValues(typeof(Uloga)), osoba.uloga);
            return View(osoba);
        }

        // POST: Osoba/PromijeniStatus/5 - Mijenjanje statusa blokiranosti
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PromijeniStatus(int id)
        {
            var korisnik = await _context.Osoba.FindAsync(id);
            if (korisnik == null)
            {
                TempData["Greska"] = "Korisnik nije pronađen.";
                return RedirectToAction(nameof(Index));
            }

            korisnik.blokiran = !korisnik.blokiran;
            await _context.SaveChangesAsync();

            string status = korisnik.blokiran ? "blokiran" : "odblokiran";
            TempData["Uspjeh"] = $"Korisnik '{korisnik.korisnicko_ime}' je {status}.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Osoba/PromijeniUlogu/5 - Mijenjanje uloge korisnika
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PromijeniUlogu(int id, Uloga novaUloga)
        {
            var korisnik = await _context.Osoba.FindAsync(id);
            if (korisnik == null)
            {
                TempData["Greska"] = "Korisnik nije pronađen.";
                return RedirectToAction(nameof(Index));
            }

            var staraUloga = korisnik.uloga;
            korisnik.uloga = novaUloga;
            await _context.SaveChangesAsync();

            TempData["Uspjeh"] = $"Uloga korisnika '{korisnik.korisnicko_ime}' je promijenjena sa '{staraUloga}' na '{novaUloga}'.";

            return RedirectToAction(nameof(Index));
        }

        // Registracija - dostupna svima
        // GET: Osoba/Registracija
        public IActionResult Registracija()
        {
            return View();
        }

        // POST: Osoba/Registracija
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registracija(Osoba novaOsoba)
        {
            if (ModelState.IsValid)
            {
                // Provjeri da li već postoji korisnik s istim email-om ili korisničkim imenom
                var postojiEmail = await _context.Osoba.AnyAsync(o => o.email == novaOsoba.email);
                var postojiKorisnickoIme = await _context.Osoba.AnyAsync(o => o.korisnicko_ime == novaOsoba.korisnicko_ime);

                if (postojiEmail)
                {
                    ModelState.AddModelError("email", "Email već postoji.");
                }

                if (postojiKorisnickoIme)
                {
                    ModelState.AddModelError("korisnicko_ime", "Korisničko ime već postoji.");
                }

                if (postojiEmail || postojiKorisnickoIme)
                {
                    return View(novaOsoba);
                }

                // Postavi defaultne vrijednosti
                novaOsoba.uloga = Uloga.RegistrovaniKorisnik;
                novaOsoba.blokiran = false;

                _context.Osoba.Add(novaOsoba);
                await _context.SaveChangesAsync();

                TempData["Uspjeh"] = $"Registracija uspješna! Dobrodošli, {novaOsoba.korisnicko_ime}!";
                return RedirectToAction("Index", "Home");
            }

            return View(novaOsoba);
        }

        private bool OsobaExists(int id)
        {
            return _context.Osoba.Any(e => e.Id == id);
        }
    }
}