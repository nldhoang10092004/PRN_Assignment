using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.DAO
{
    public class SpeakingQuestionDAO
    {
        private readonly AiIeltsDbContext _context;

        public SpeakingQuestionDAO()
        {
            _context = new AiIeltsDbContext();
        }

        // ====================================
        // 1. Lấy tất cả câu hỏi
        // ====================================
        public async Task<List<SpeakingQuestion>> GetAllAsync()
        {
            return await _context.SpeakingQuestions
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SpeakingQuestion> SaveQuestionAsync(string content)
        {
            var question = new SpeakingQuestion
            {
                Content = content
            };

            _context.SpeakingQuestions.Add(question);
            await _context.SaveChangesAsync();

            // sau khi SaveChanges, question.Id đã có giá trị identity
            return question;
        }

        public async Task<SpeakingAnswer> SaveAnswerAsync(int questionId, string transcript, decimal Grade, string feedback)
        {
            var answer = new SpeakingAnswer
            {
                QuestionId = questionId,
                Transcript = transcript,
                Grade = Grade,
                Feedback = feedback
            };

            _context.SpeakingAnswers.Add(answer);
            await _context.SaveChangesAsync();

            return answer;
        }

        public async Task<SpeakingAnswer?> GetLastAttemptAsync()
        {
            return await _context.SpeakingAnswers
                .OrderByDescending(a => a.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }


        public async Task<List<SpeakingAnswer>> GetLastFiveAttemptsAsync()
        {
            return await _context.SpeakingAnswers
                .OrderByDescending(a => a.Id)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}