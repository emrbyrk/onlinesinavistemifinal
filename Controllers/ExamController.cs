using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using onlinesinavsistemifinal.Data;
using onlinesinavsistemifinal.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace onlinesinavsistemifinal.Controllers
{
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExamController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tüm sınavları listele (Öğrenciler için)
        [Authorize(Roles = "Öğrenci,Öğretmen")]
        public ActionResult Index()
        {
            var exams = _context.Exams.ToList();
            return View(exams);
        }

        // Yeni sınav oluşturma (Öğretmenler için)
        [Authorize(Roles = "Öğretmen")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Öğretmen")]
        public ActionResult Create(Exam exam)
        {
            if (ModelState.IsValid)
            {
                _context.Exams.Add(exam);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(exam);
        }

        // Sınava girme (Öğrenciler için)
        [Authorize(Roles = "Öğrenci")]
        public ActionResult TakeExam(int id)
        {
            var exam = _context.Exams.Include(e => e.Questions).FirstOrDefault(e => e.Id == id);
            if (exam == null) return NotFound();
            return View(exam);
        }

        // Sınavı gönderme ve sonuçları kaydetme
        [HttpPost]
        [Authorize(Roles = "Öğrenci")]
        public ActionResult SubmitExam(int examId, Dictionary<int, string> answers)
        {
            var exam = _context.Exams.Include(e => e.Questions).FirstOrDefault(e => e.Id == examId);
            if (exam == null) return NotFound();

            int score = 0;
            foreach (var question in exam.Questions)
            {
                if (answers.ContainsKey(question.Id) && answers[question.Id] == question.CorrectAnswer)
                {
                    score++;
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return BadRequest("Kullanıcı kimliği bulunamadı.");

            var result = new ExamResult
            {
                UserId = userId,
                ExamId = examId,
                Score = score
            };

            _context.ExamResults.Add(result);
            _context.SaveChanges();

            return RedirectToAction("Result", new { id = result.Id });
        }

        // Sınav sonuçlarını gösterme
        public ActionResult Result(int id)
        {
            var result = _context.ExamResults
                .Include(r => r.Exam)
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == id);

            if (result == null) return NotFound();

            return View(result);
        }
    }
}



