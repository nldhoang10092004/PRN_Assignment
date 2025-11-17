using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.DAO
{
    public class WritingQuestionDAO
    {
        private readonly AiIeltsDbContext _context;

        public WritingQuestionDAO(AiIeltsDbContext context)
        {
            _context = context;
        }


        public async Task<WritingQuestion> SaveQuestionAsync(string content)
        {
            var question = new WritingQuestion
            {
                Content = content
            };

            _context.WritingQuestions.Add(question);
            await _context.SaveChangesAsync();

            // sau khi SaveChanges, question.Id đã có giá trị identity
            return question;
        }

        public async Task<WritingQuestion> SaveQuestionAsync(WritingQuestion question)
        {
            _context.WritingQuestions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }


        public async Task<WritingAnswer> SaveAnswerAsync(
            int questionId,
            string? content,
            decimal? grade,
            string? feedback)
        {
            var answer = new WritingAnswer
            {
                QuestionId = questionId,
                Content = content,
                Grade = grade,
                Feedback = feedback
            };

            _context.WritingAnswers.Add(answer);
            await _context.SaveChangesAsync();

            return answer;
        }

        // Overload nếu bạn có sẵn object WritingAnswer
        public async Task<WritingAnswer> SaveAnswerAsync(WritingAnswer answer)
        {
            _context.WritingAnswers.Add(answer);
            await _context.SaveChangesAsync();
            return answer;
        }

        // ====================================
        // 3️⃣ LẤY 5 LẦN LÀM BÀI GẦN NHẤT CHO 1 CÂU HỎI
        //    (không có CreatedAt nên sort theo Id)
        // ====================================
        /// <summary>
        /// Lấy 5 answer mới nhất (Id lớn nhất) cho 1 question.
        /// </summary>
        public async Task<List<WritingAnswer>> GetLastFiveAttemptsAsync()
        {
            return await _context.WritingAnswers
                .OrderByDescending(a => a.Id)          // Id càng lớn càng mới
                .Take(5)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<WritingAnswer?> GetLastAttemptAsync()
        {
            return await _context.WritingAnswers
                .OrderByDescending(a => a.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
