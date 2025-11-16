using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.DAO
{
    public class SpeakingAnswerDAO
    {
        private readonly AiIeltsDbContext _context;

        public SpeakingAnswerDAO(AiIeltsDbContext context)
        {
            _context = context;
        }

        // ====================================
        // 1. Submit new answer
        // ====================================
        public async Task<SpeakingAnswer> SubmitAnswerAsync(SpeakingAnswer answer)
        {
            _context.SpeakingAnswers.Add(answer);
            await _context.SaveChangesAsync();
            return answer;
        }

        // ====================================
        // 2. Get all answers
        // ====================================
        public async Task<List<SpeakingAnswer>> GetAllAsync()
        {
            return await _context.SpeakingAnswers
                .Include(a => a.Question)
                .AsNoTracking()
                .ToListAsync();
        }

        // ====================================
        // 3. Get answers by QuestionId
        // ====================================
        public async Task<List<SpeakingAnswer>> GetByQuestionIdAsync(int questionId)
        {
            return await _context.SpeakingAnswers
                .Include(a => a.Question)
                .AsNoTracking()
                .Where(a => a.QuestionId == questionId)
                .ToListAsync();
        }

        // ====================================
        // 4. Get grade / feedback
        // ====================================
        public async Task<(decimal? Grade, string? Feedback)?> GetGradeAsync(int answerId)
        {
            var answer = await _context.SpeakingAnswers
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == answerId);

            if (answer == null) return null;

            return (answer.Grade, answer.Feedback);
        }

        // ====================================
        // 5. Delete answer
        // ====================================
        public async Task<bool> DeleteAsync(int answerId)
        {
            var answer = await _context.SpeakingAnswers.FindAsync(answerId);
            if (answer == null) return false;

            _context.SpeakingAnswers.Remove(answer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
