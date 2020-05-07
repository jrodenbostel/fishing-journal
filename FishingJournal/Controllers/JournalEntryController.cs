using System.Linq;
using System.Threading.Tasks;
using FishingJournal.Data;
using FishingJournal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FishingJournal.Controllers
{
    [Authorize]
    public class JournalEntryController : Controller
    {
        private readonly DefaultContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration; 

        public JournalEntryController(DefaultContext context, UserManager<User> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        // GET: JournalEntry
        public async Task<IActionResult> Index()
        {
            ViewData["MapboxKey"] = _configuration.GetValue<string>("MapboxKey");
            return View(await _context.JournalEntries.OrderByDescending(item => item.Date).ToListAsync());
        }

        // GET: JournalEntry/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var journalEntry = await _context.JournalEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (journalEntry == null)
            {
                return NotFound();
            }

            return View(journalEntry);
        }

        // GET: JournalEntry/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: JournalEntry/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Notes,Latitude,Longitude,LocationOverride,Precipitation,Temperature,Humidity,BarometricPressure,WindSpeed,WindDirection,Date")]
            JournalEntry journalEntry)
        {
            var user = await _userManager.GetUserAsync(User);
            journalEntry.Email = user.Email;
            if (ModelState.IsValid)
            {
                _context.Add(journalEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(journalEntry);
        }

        // GET: JournalEntry/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var journalEntry = await _context.JournalEntries.FindAsync(id);
            if (journalEntry == null)
            {
                return NotFound();
            }

            return View(journalEntry);
        }

        // POST: JournalEntry/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Notes,Latitude,Longitude,LocationOverride,Precipitation,Temperature,Humidity,BarometricPressure,WindSpeed,WindDirection,Date")]
            JournalEntry journalEntry)
        {
            if (id != journalEntry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(journalEntry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JournalEntryExists(journalEntry.Id))
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

            return View(journalEntry);
        }

        // GET: JournalEntry/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var journalEntry = await _context.JournalEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (journalEntry == null)
            {
                return NotFound();
            }

            return View(journalEntry);
        }

        // POST: JournalEntry/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var journalEntry = await _context.JournalEntries.FindAsync(id);
            _context.JournalEntries.Remove(journalEntry);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JournalEntryExists(int id)
        {
            return _context.JournalEntries.Any(e => e.Id == id);
        }
    }
}