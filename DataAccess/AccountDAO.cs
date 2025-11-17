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
        //get by id 
        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _context.Accounts
                .Include(a => a.UserDetail)
                .Include(a => a.Apikey)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == id);
        }

        //Get by username
        public async Task<Account?> GetByUsernameAsync(string username)
        {
            return await _context.Accounts
                .Include(a => a.UserDetail)
                .Include(a => a.Apikey)
                .FirstOrDefaultAsync(a => a.Username == username);
        }

        //Check duplicate
        public async Task<bool> EmailExistsAsync(string email)
            => await _context.Accounts.AnyAsync(a => a.Email == email);

        public async Task<bool> UsernameExistsAsync(string username)
            => await _context.Accounts.AnyAsync(a => a.Username == username);

        //Register
        public async Task<Account> CreateAsync(Account account)
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return account;
        }

        //Update Account Info
        public async Task<bool> UpdateAccountAsync(Account model)
        {
            var acc = await _context.Accounts.FirstOrDefaultAsync(x => x.UserId == model.UserId);
            if (acc == null) return false;

            acc.Email = model.Email;
            acc.Username = model.Username;
            acc.HashPass = model.HashPass;

            await _context.SaveChangesAsync();
            return true;
        }

        //Update Information
        public async Task<bool> UpdateUserDetailAsync(UserDetail detail)
        {
            var exist = await _context.UserDetails.FirstOrDefaultAsync(x => x.UserId == detail.UserId);

            if (exist == null)
            {
                _context.UserDetails.Add(detail);
            }
            else
            {
                exist.FullName = detail.FullName;
                exist.Address = detail.Address;
                exist.Dob = detail.Dob;
                exist.AvatarUrl = detail.AvatarUrl;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        //Delete
        public async Task<bool> DeleteAsync(int userId)
        {
            var acc = await _context.Accounts
                .Include(a => a.UserDetail)
                .Include(a => a.Apikey)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (acc == null) return false;

            if (acc.UserDetail != null)
                _context.UserDetails.Remove(acc.UserDetail);

            if (acc.Apikey != null)
                _context.Apikeys.Remove(acc.Apikey);

            _context.Accounts.Remove(acc);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
