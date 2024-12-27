using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using onlinesinavsistemifinal.Data;
using onlinesinavsistemifinal.Models;

namespace onlinesinavsistemifinal.Models
{
    [Authorize]
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection ile _context alanına değer atanıyor
        public ExamController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // 1. Sınavları Listeleme
        [Authorize(Roles = "Öğrenci,Öğretmen")]
        public IActionResult Index()
        {
            var exams = _context.Exams.ToList();
            return View(exams);
        }

        // 2. Yeni Sınav Oluşturma (Sadece Öğretmenler)
        [Authorize(Roles = "Öğretmen")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Öğretmen")]
        public IActionResult Create(Exam exam)
        {
            if (ModelState.IsValid)
            {
                _context.Exams.Add(exam);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(exam);
        }

        // 3. Soru Ekleme (Sadece Öğretmenler)
        [Authorize(Roles = "Öğretmen")]
        public IActionResult AddQuestion(int examId)
        {
            var exam = _context.Exams.Find(examId);
            if (exam == null) return NotFound();

            ViewBag.ExamId = examId;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Öğretmen")]
        public IActionResult AddQuestion(Question question)
        {
            if (ModelState.IsValid)
            {
                _context.Questions.Add(question);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ExamId = question.ExamId;
            return View(question);
        }

        // 4. Sınav Çözme (Sadece Öğrenciler)
        [Authorize(Roles = "Öğrenci")]
        public IActionResult TakeExam(int examId)
        {
            var exam = _context.Exams
                .Include(e => e.Questions)
                .FirstOrDefault(e => e.Id == examId);

            if (exam == null) return NotFound();
            return View(exam);
        }

        [HttpPost]
        [Authorize(Roles = "Öğrenci")]
        public IActionResult SubmitExam(int examId, Dictionary<int, string> answers)
        {
            var exam = _context.Exams
                .Include(e => e.Questions)
                .FirstOrDefault(e => e.Id == examId);

            if (exam == null) return NotFound();

            int score = 0;
            foreach (var question in exam.Questions)
            {
                if (answers.ContainsKey(question.Id) && answers[question.Id] == question.CorrectAnswer)
                {
                    score++;
                }
            }

            ViewBag.Score = score;
            ViewBag.TotalQuestions = exam.Questions.Count;

            return View("Result");
        }

        // 5. Sonuç Gösterme
        [Authorize(Roles = "Öğrenci")]
        public IActionResult Result()
        {
            return View();
        }
    }
    // Exam sınıfı
    public class Exam
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime Date { get; set; }
        public int DurationInMinutes { get; set; } // Özellik eklendi

        // İlişkili sorular
        public ICollection<Question> Questions { get; set; } = new List<Question>()!;
    }



}
