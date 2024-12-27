using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using onlinesinavsistemifinal.Data;
using onlinesinavsistemifinal.Models;

public class Program
{

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Veritabaný baðlantýsýný yapýlandýr
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Identity servislerini ekle
        builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false; // E-posta doðrulama gereksinimini kaldýr
            options.Password.RequireDigit = false; // Þifre politikalarýný düzenle
            options.Password.RequiredLength = 5;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddRoles<IdentityRole>() // Roller için destek
        .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Rolleri, admin kullanýcýyý ve sorularý oluþtur
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                await CreateRolesAndAdminUser(services);
                await SeedQuestions(services); // Sorularý ekleme
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }
        }

        // HTTP istek hattýný yapýlandýr
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication(); // Kimlik doðrulama middleware
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        await app.RunAsync();
    }

    // Rolleri ve admin kullanýcýyý oluþturma fonksiyonu
    private static async Task CreateRolesAndAdminUser(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Rolleri tanýmla
        string[] roles = { "Öðrenci", "Öðretmen" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Admin kullanýcý oluþtur
        var adminEmail = "emr.bayrak19@gmail.com";
        var adminPassword = "z78sfr";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true // Admin için e-posta doðrulama gerekmiyor
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Öðretmen");
            }
        }
    }

    // Sorularý ekleme fonksiyonu
    private static async Task SeedQuestions(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Eðer veritabanýnda sýnav yoksa bir tane oluþtur
        if (!context.Exams.Any())
        {
            var exam = new Exam
            {
                Title = "HTML ve CSS Test Sýnavý",
                DurationInMinutes = 10
            };
            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            // Sorularý ekle
            var questions = new List<Question>
            {
                new Question
                {
                    Content = "Aþaðýdakilerden hangisi en büyük baþlýk etiketidir?",
                    OptionA = "h1",
                    OptionB = "h2",
                    OptionC = "h3",
                    OptionD = "h4",
                    CorrectAnswer = "A",
                    ExamId = exam.Id
                },
                new Question
                {
                    Content = "Bir HTML belgesinde CSS dosyasýný baðlamak için hangi etiket kullanýlýr?",
                    OptionA = "<css>",
                    OptionB = "<script>",
                    OptionC = "<link>",
                    OptionD = "<style>",
                    CorrectAnswer = "C",
                    ExamId = exam.Id
                },
                // Diðer sorularý buraya ekleyebilirsiniz
            };

            context.Questions.AddRange(questions);
            await context.SaveChangesAsync();
        }
    }
}




