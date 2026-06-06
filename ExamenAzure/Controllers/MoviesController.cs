using ExamenAzure.Data;
using ExamenAzure.Models;
using ExamenAzure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ExamenAzure.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IBlobService _blobService;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(AppDbContext context, IBlobService blobService, ILogger<MoviesController> logger)
        {
            _context = context;
            _blobService = blobService;
            _logger = logger;
        }

        // Каталог фільмів (Головна сторінка)
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.ToListAsync();
            return View(movies);
        }

        // Сторінка додавання нового фільму (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Обробка додавання фільму (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string description, IFormFile videoFile)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || videoFile == null)
            {
                ModelState.AddModelError("", "Усі поля є обов'язковими для заповнення.");
                return View();
            }

            try
            {
                // Етап 3: Завантаження файлу в Azure Blob Storage
                string blobFileName = await _blobService.UploadVideoAsync(videoFile);

                // Етап 2: Збереження метаданих в SQL DB
                var movie = new Movie
                {
                    Title = title,
                    Description = description,
                    BlobFileName = blobFileName,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();

                // Етап 5: Логування інформації
                _logger.LogInformation("Завантажено новий фільм: {Title}", title);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Етап 5: Логування помилок Azure
                _logger.LogError(ex, "Помилка під час завантаження або збереження фільму: {Title}", title);
                ModelState.AddModelError("", "Сталася помилка при завантаженні файлу в хмару.");
                return View();
            }
        }

        // Плеєр / Деталі фільму (GET)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                // Етап 5: Штучна перевірка та Warning лог
                _logger.LogWarning("Спроба доступу до фільму, якого не існує в базі. ID: {MovieId}", id);
                return NotFound();
            }

            try
            {
                // Етап 3 & 4: Генерація тимчасового SAS-токену
                string sasUrl = _blobService.GenerateSasToken(movie.BlobFileName);
                ViewBag.SasVideoUrl = sasUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка генерації SAS токену для файлу: {BlobFileName}", movie.BlobFileName);
                return StatusCode(500, "Не вдалося отримати доступ до відеофайлу.");
            }

            return View(movie);
        }
    }
            
}
