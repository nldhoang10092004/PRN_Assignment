using Repository.DAO;
using Repository.Models;
using Service;
using System.Threading.Tasks;

namespace Business
{
    public class AccountBusiness
    {
        private readonly AccountDAO _accDao;

        public AccountBusiness(AiIeltsDbContext context)
        {
            _accDao = new AccountDAO(context);
        }

        /* =======================================================
         *  LOGIN BUSINESS
         * =======================================================*/
        public async Task<Account?> LoginAsync(string username, string password)
        {
            // Lấy account theo username
            var account = await _accDao.GetByUsernameAsync(username);
            if (account == null) return null;

            bool ok = PasswordHasher.Verify(password, account.HashPass);
            return ok ? account : null;
        }

        /* =======================================================
         *  REGISTER BUSINESS
         * =======================================================*/
        public async Task<(bool success, string message)> RegisterAsync(
            string username, string email, string password)
        {
            if (await _accDao.UsernameExistsAsync(username))
                return (false, "Username already exists!");

            if (await _accDao.EmailExistsAsync(email))
                return (false, "Email already exists!");

            // Hash mật khẩu
            string hashed = PasswordHasher.Hash(password);

            var acc = new Account
            {
                Username = username,
                Email = email,
                HashPass = hashed
            };

            await _accDao.CreateAsync(acc);

            // tạo UserDetail rỗng
            await _accDao.UpdateUserDetailAsync(new UserDetail
            {
                UserId = acc.UserId
            });

            return (true, "Registered successfully!");
        }

        /* =======================================================
         *  UPDATE PROFILE
         * =======================================================*/
        public async Task<bool> UpdateProfileAsync(UserDetail detail)
        {
            return await _accDao.UpdateUserDetailAsync(detail);
        }

        /* =======================================================
         *  GET ACCOUNT INFO
         * =======================================================*/
        public async Task<Account?> GetAccountAsync(int id)
        {
            return await _accDao.GetByIdAsync(id);
        }
    }
}
