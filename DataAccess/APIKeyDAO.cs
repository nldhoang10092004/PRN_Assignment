using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Threading.Tasks;

namespace Repository.DAO
{
    public class APIKeyDAO
    {
        private readonly AiIeltsDbContext _context;

        public APIKeyDAO(AiIeltsDbContext context)
        {
            _context = context;
        }

        public async Task<Apikey?> GetApiKeyAsync(int userId)
        {
            return await _context.Apikeys
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<Apikey?> InsertApiKeyAsync(int userId, string deepgram, string chatgpt)
        {
            var exists = await _context.Apikeys.FindAsync(userId);
            if (exists != null)
            {
                return null;
            }

            var newKey = new Apikey
            {
                UserId = userId,
                DeepgramKey = deepgram,
                ChatGptkey = chatgpt
            };

            _context.Apikeys.Add(newKey);
            await _context.SaveChangesAsync();

            return newKey;
        }
        public async Task<bool> UpdateApiKeyAsync(int userId, string deepgram, string chatgpt)
        {
            var key = await _context.Apikeys.FirstOrDefaultAsync(x => x.UserId == userId);
            if (key == null) return false;

            key.DeepgramKey = deepgram;
            key.ChatGptkey = chatgpt;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Apikey> SaveApiKeyAsync(int userId, string deepgram, string chatgpt)
        {
            var key = await _context.Apikeys.FirstOrDefaultAsync(x => x.UserId == userId);

            if (key == null)
            {
                key = new Apikey
                {
                    UserId = userId,
                    DeepgramKey = deepgram,
                    ChatGptkey = chatgpt
                };

                _context.Apikeys.Add(key);
                await _context.SaveChangesAsync();
                return key;
            }

            key.DeepgramKey = deepgram;
            key.ChatGptkey = chatgpt;
            await _context.SaveChangesAsync();

            return key;
        }

        public async Task<bool> DeleteApiKeyAsync(int userId)
        {
            var key = await _context.Apikeys.FirstOrDefaultAsync(x => x.UserId == userId);
            if (key == null) return false;

            _context.Apikeys.Remove(key);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
