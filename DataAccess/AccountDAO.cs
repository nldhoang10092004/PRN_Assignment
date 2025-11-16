using Microsoft.EntityFrameworkCore;
using Repository.Models;
using System.Threading.Tasks;

namespace Repository.DAO
{
    public class AccountDAO
    {
        private readonly AiIeltsDbContext _context;

        public AccountDAO(AiIeltsDbContext context)
        {
            _context = context;
        }

        /* ========================================
         *  GET ACCOUNT BY ID
         * ======================================== */
        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _context.Accounts
                .Include(a => a.UserDetail)  // include user detail
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == id);
        }

        /* ========================================
         *  LOGIN
         * ======================================== */
        public async Task<Account?> LoginAsync(string username, string hashPassword)
        {
            return await _context.Accounts
                .Include(a => a.UserDetail)
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.Username == username &&
                    a.HashPass == hashPassword);
        }

        /* ========================================
         *  CHECK DUPLICATES
         * ======================================== */
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Accounts.AnyAsync(a => a.Email == email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Accounts.AnyAsync(a => a.Username == username);
        }

        /* ========================================
         *  REGISTER ACCOUNT
         * ======================================== */
        public async Task<Account> RegisterAsync(Account account)
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        /* ========================================
         *  UPDATE ACCOUNT
         * ======================================== */
        public async Task<bool> UpdateAsync(Account account)
        {
            var exists = await _context.Accounts.FirstOrDefaultAsync(x => x.UserId == account.UserId);

            if (exists == null) return false;

            _context.Entry(exists).CurrentValues.SetValues(account);
            await _context.SaveChangesAsync();

            return true;
        }

        /* ========================================
         *  UPDATE USER DETAIL
         * ======================================== */
        public async Task<bool> UpdateUserDetailAsync(UserDetail detail)
        {
            var existing = await _context.UserDetails
                .FirstOrDefaultAsync(x => x.UserId == detail.UserId);

            if (existing == null)
            {
                _context.UserDetails.Add(detail);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(detail);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /* ========================================
         *  DELETE ACCOUNT + USER DETAIL
         * ======================================== */
        public async Task<bool> DeleteAsync(int userId)
        {
            var acc = await _context.Accounts
                .Include(a => a.UserDetail)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (acc == null) return false;

            if (acc.UserDetail != null)
                _context.UserDetails.Remove(acc.UserDetail);

            _context.Accounts.Remove(acc);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
