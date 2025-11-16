using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Collections.Generic;
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

        // ====================================
        // 1. Lấy tất cả câu hỏi
        // ====================================
        public async Task<List<WritingQuestion>> GetAllAsync()
        {
            return await _context.WritingQuestions
                .AsNoTracking()
                .ToListAsync();
        }

        // ====================================
        // 2. Lấy câu hỏi theo ID
        // ====================================
        public async Task<WritingQuestion?> GetByIdAsync(int questionId)
        {
            return await _context.WritingQuestions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == questionId);
        }

        // ====================================
        // 3. Lấy câu hỏi kèm bài làm của học sinh (nếu muốn)
        // ====================================
        public async Task<WritingQuestion?> GetByIdWithAnswersAsync(int questionId)
        {
            return await _context.WritingQuestions
                .Include(q => q.WritingAnswers)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == questionId);
        }
    }
}
