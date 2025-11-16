using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.DAO
{
    public class SpeakingQuestionDAO
    {
        private readonly AiIeltsDbContext _context;

        public SpeakingQuestionDAO(AiIeltsDbContext context)
        {
            _context = context;
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
    }
}