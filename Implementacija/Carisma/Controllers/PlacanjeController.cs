using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Carisma.Data;
using Carisma.Models;
using Stripe;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;

namespace Carisma.Controllers
{
    public class PlacanjeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PlacanjeController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            // Postavi Stripe konfiguraciju
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        // Postojeći kod...
        public async Task<IActionResult> Index()
        {
            return View(await _context.Placanje.ToListAsync());
        }

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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,iznos,datumPlacanja,statusPlacanja,brojKartice")] Placanje placanje)
        {
            if (ModelState.IsValid)
            {
                // Postavi datum i status
                placanje.datumPlacanja = DateTime.Now;
                placanje.statusPlacanja = StatusPlacanja.NaCekanju;

                _context.Add(placanje);
                await _context.SaveChangesAsync();

                // Preusmeri na Stripe plaćanje
                return RedirectToAction(nameof(StripeCheckout), new { id = placanje.Id });
            }
            return View(placanje);
        }

        // NOVE STRIPE METODE

        // Kreira Stripe checkout session
        public async Task<IActionResult> StripeCheckout(int id)
        {
            var placanje = await _context.Placanje.FindAsync(id);
            if (placanje == null) return NotFound();

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(placanje.iznos * 100), // Stripe koristi cente
                            Currency = "usd", // ili "eur", "bam" itd.
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Plaćanje #{placanje.Id}",
                                Description = $"Plaćanje za iznos {placanje.iznos:C}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{domain}/Placanje/StripeSuccess?session_id={{CHECKOUT_SESSION_ID}}&placanje_id={id}",
                CancelUrl = $"{domain}/Placanje/StripeCancel?placanje_id={id}",
                Metadata = new Dictionary<string, string>
                {
                    { "placanje_id", id.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            // Sačuvaj session ID
            placanje.statusPlacanja = StatusPlacanja.NaCekanju;
            _context.Update(placanje);
            await _context.SaveChangesAsync();

            return Redirect(session.Url);
        }

        // Uspešno plaćanje
        public async Task<IActionResult> StripeSuccess(string session_id, int placanje_id)
        {
            var placanje = await _context.Placanje.FindAsync(placanje_id);
            if (placanje == null) return NotFound();

            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(session_id);

                if (session.PaymentStatus == "paid")
                {
                    placanje.statusPlacanja = StatusPlacanja.Uspjesno;
                    placanje.datumPlacanja = DateTime.Now;
                    _context.Update(placanje);
                    await _context.SaveChangesAsync();

                    ViewBag.Message = "Plaćanje je uspešno završeno!";
                    ViewBag.SessionId = session_id;
                    ViewBag.Amount = placanje.iznos;
                }
                else
                {
                    ViewBag.Message = "Plaćanje nije uspešno završeno.";
                }
            }
            catch (StripeException ex)
            {
                ViewBag.Message = $"Greška: {ex.Message}";
            }

            return View(placanje);
        }

        // Otkazano plaćanje
        public async Task<IActionResult> StripeCancel(int placanje_id)
        {
            var placanje = await _context.Placanje.FindAsync(placanje_id);
            if (placanje == null) return NotFound();

            placanje.statusPlacanja = StatusPlacanja.Neuspjesno;
            _context.Update(placanje);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Plaćanje je otkazano.";
            return View(placanje);
        }

        // Webhook za Stripe eventi (opcionalno)
        [HttpPost]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _configuration["Stripe:WebhookSecret"] // Dodaj ovo u appsettings
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session.Metadata.TryGetValue("placanje_id", out var placanjeIdStr) &&
                        int.TryParse(placanjeIdStr, out var placanjeId))
                    {
                        var placanje = await _context.Placanje.FindAsync(placanjeId);
                        if (placanje != null)
                        {
                            placanje.statusPlacanja = StatusPlacanja.Uspjesno;
                            placanje.datumPlacanja = DateTime.Now;
                            _context.Update(placanje);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
        }

        // Postojeći kod...
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

        // Postojeće validacije - možeš ih zadržati ili izbaciti
        public IActionResult ValidirajKarticu(int id)
        {
            var placanje = _context.Placanje.FirstOrDefault(p => p.Id == id);
            if (placanje == null) return NotFound();

            bool validna = placanje.validirajKarticu();
            ViewBag.Rezultat = validna ? "Kartica je validna." : "Kartica nije validna.";
            return View("RezultatValidacije");
        }

        // Zameniti sa Stripe integracijom
        public IActionResult ProcesirajPlacanje(int id)
        {
            // Preusmeri na Stripe checkout umesto lokalne obrade
            return RedirectToAction(nameof(StripeCheckout), new { id = id });
        }

        public IActionResult GenerisiPotvrdu(int id)
        {
            var placanje = _context.Placanje.FirstOrDefault(p => p.Id == id);
            if (placanje == null) return NotFound();

            string potvrda = placanje.generisiPotvrdu();
            ViewBag.Potvrda = string.IsNullOrEmpty(potvrda) ? "Potvrda nije generisana." : potvrda;
            return View("PotvrdaPlacanja");
        }

        // Zameniti sa Stripe integracijom
        public IActionResult PlatiOnline(int id)
        {
            // Preusmeri na Stripe checkout
            return RedirectToAction(nameof(StripeCheckout), new { id = id });
        }

        private bool PlacanjeExists(int id)
        {
            return _context.Placanje.Any(e => e.Id == id);
        }
    }
}