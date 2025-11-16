using Microsoft.EntityFrameworkCore;
using Repository.Models;

public class TestAiIeltsDbContext : AiIeltsDbContext
{
    public TestAiIeltsDbContext(DbContextOptions<AiIeltsDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Nếu chưa có options, dùng InMemory database
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("TestDB");
        }
    }
}
