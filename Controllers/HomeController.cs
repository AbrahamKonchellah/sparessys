using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SparePartsWeb.Models;
using SparePartsWeb.Data;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace SparePartsWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Dashboard summary counts
            ViewBag.TotalSpareParts = await _context.SpareParts.CountAsync();
            ViewBag.TotalVendors = await _context.Vendors.CountAsync();
            ViewBag.LowStockCount = await _context.SpareParts.CountAsync(s => s.Quantity < 5);

            // Low stock list
            var lowStockParts = await _context.SpareParts
                .Where(s => s.Quantity < 5)
                .OrderBy(s => s.Quantity)
                .Take(10)
                .ToListAsync();
            ViewBag.LowStockParts = lowStockParts;

            // Recent additions
            ViewBag.RecentSpareParts = await _context.SpareParts
                .OrderByDescending(s => s.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentVendors = await _context.Vendors
                .OrderByDescending(v => v.Id)
                .Take(5)
                .ToListAsync();

            return View();
        }

        // Generate Restock Report as PDF
        public async Task<IActionResult> GenerateRestockReport()
        {
            int targetLevel = 10;

            var restockRecommendations = await _context.SpareParts
                .Where(s => s.Quantity < targetLevel)
                .Select(s => new
                {
                    s.Name,
                    s.Brand,
                    s.Quantity,
                    Needed = targetLevel - s.Quantity,
                    EstimatedCost = (targetLevel - s.Quantity) * s.Price
                })
                .OrderByDescending(s => s.EstimatedCost)
                .ToListAsync();

            using (var memoryStream = new MemoryStream())
            {
                // Create PDF document
                Document doc = new Document(PageSize.A4, 30, 30, 30, 30);
                PdfWriter.GetInstance(doc, memoryStream);
                doc.Open();

                // Fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                // Title
                doc.Add(new Paragraph("Spare Parts Restock Report", titleFont));
                doc.Add(new Paragraph($"Generated on: {DateTime.Now:dd MMM yyyy HH:mm}\n\n", textFont));

                // Table setup
                PdfPTable table = new PdfPTable(5) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 2f, 2f, 1f, 1f, 2f });

                string[] headers = { "Part Name", "Brand", "Current Qty", "Needed", "Estimated Cost (Ksh)" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, headerFont))
                    {
                        BackgroundColor = new BaseColor(230, 230, 250)
                    };
                    table.AddCell(cell);
                }

                foreach (var item in restockRecommendations)
                {
                    table.AddCell(new Phrase(item.Name, textFont));
                    table.AddCell(new Phrase(item.Brand, textFont));
                    table.AddCell(new Phrase(item.Quantity.ToString(), textFont));
                    table.AddCell(new Phrase(item.Needed.ToString(), textFont));
                    table.AddCell(new Phrase(item.EstimatedCost.ToString("N2"), textFont));
                }

                doc.Add(table);
                doc.Add(new Paragraph("\nReport generated automatically by SparePartsWeb System.", textFont));
                doc.Close();

                byte[] bytes = memoryStream.ToArray();
                return File(bytes, "application/pdf", "RestockReport.pdf");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
