using Microsoft.EntityFrameworkCore;
using Repository.DAO;
using Repository.Models;
using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // =================================
        // Khởi tạo DbContext với InMemory DB
        // =================================
        var options = new DbContextOptionsBuilder<AiIeltsDbContext>().Options;
        var context = new TestAiIeltsDbContext(options);

        var accountDAO = new AccountDAO(context);
        var apiKeyDAO = new APIKeyDAO(context);

        Console.WriteLine("=== TEST ACCOUNT DAO ===\n");

        // =====================================
        // 1. REGISTER NEW ACCOUNT
        // =====================================
        Console.WriteLine(">>> Testing Register");

        var newAcc = new Account
        {
            Username = "console_user",
            Email = "console@example.com",
            HashPass = "123456", // hash demo
            CreatedAt = DateTime.UtcNow
        };

        if (await accountDAO.UsernameExistsAsync(newAcc.Username))
        {
            Console.WriteLine("❌ Username exists!");
        }
        else if (await accountDAO.EmailExistsAsync(newAcc.Email))
        {
            Console.WriteLine("❌ Email exists!");
        }
        else
        {
            newAcc = await accountDAO.RegisterAsync(newAcc);
            Console.WriteLine($"✔ Registered OK! UserId = {newAcc.UserId}");
        }

        // =====================================
        // 2. LOGIN
        // =====================================
        Console.WriteLine("\n>>> Testing Login");

        var loginAcc = await accountDAO.LoginAsync("console_user", "123456");
        Console.WriteLine(loginAcc != null ? "✔ Login thành công" : "❌ Login thất bại");

        // =====================================
        // 3. UPDATE USER DETAIL
        // =====================================
        Console.WriteLine("\n>>> Testing Update UserDetail");

        var detail = new UserDetail
        {
            UserId = newAcc.UserId,
            FullName = "Console Testing User",
            Address = "HCM City",
            AvatarUrl = "https://avatar.com/test.jpg",
            Dob = new DateOnly(2000, 1, 1)
        };

        await accountDAO.UpdateUserDetailAsync(detail);
        Console.WriteLine("✔ Update UserDetail OK");

        // =====================================
        // 4. SAVE API KEY
        // =====================================
        Console.WriteLine("\n>>> Testing APIKey Save");

        var savedKey = await apiKeyDAO.SaveApiKeyAsync(
            newAcc.UserId,
            "deepgram_test_key",
            "chatgpt_test_key"
        );

        Console.WriteLine("✔ API Key Saved");

        // =====================================
        // 5. GET ACCOUNT BY ID
        // =====================================
        Console.WriteLine("\n>>> Testing GetById");

        var foundAcc = await accountDAO.GetByIdAsync(newAcc.UserId);
        Console.WriteLine(foundAcc != null ? $"✔ Found account: {foundAcc.Username}" : "❌ Not found");

        // =====================================
        // 6. GET ALL ACCOUNTS
        // =====================================
        Console.WriteLine("\n>>> Testing GetAll");

        var allAccounts = await accountDAO.GetAllAsync();
        Console.WriteLine($"✔ Total Accounts: {allAccounts.Count}");

        // =====================================
        // 7. DELETE ACCOUNT
        // =====================================
        Console.WriteLine("\n>>> Testing Delete");

        var deleted = await accountDAO.DeleteAsync(newAcc.UserId);
        Console.WriteLine(deleted ? "✔ Deleted OK" : "❌ Delete failed");

        Console.WriteLine("\n=== TEST FINISHED ===");
    }
}
