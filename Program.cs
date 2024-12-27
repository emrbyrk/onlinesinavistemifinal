using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using onlinesinavsistemifinal.Data;
using onlinesinavsistemifinal.Models;

public class Program
{

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Veritaban� ba�lant�s�n� yap�land�r
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Identity servislerini ekle
        builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false; // E-posta do�rulama gereksinimini kald�r
            options.Password.RequireDigit = false; // �ifre politikalar�n� d�zenle
            options.Password.RequiredLength = 5;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddRoles<IdentityRole>() // Roller i�in destek
        .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Rolleri, admin kullan�c�y� ve sorular� olu�tur
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                await CreateRolesAndAdminUser(services);
                await SeedQuestions(services); // Sorular� ekleme
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }
        }

        // HTTP istek hatt�n� yap�land�r
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

        app.UseAuthentication(); // Kimlik do�rulama middleware
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        await app.RunAsync();
    }

    // Rolleri ve admin kullan�c�y� olu�turma fonksiyonu
    private static async Task CreateRolesAndAdminUser(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Rolleri tan�mla
        string[] roles = { "��renci", "��retmen" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Admin kullan�c� olu�tur
        var adminEmail = "emr.bayrak19@gmail.com";
        var adminPassword = "z78sfr";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true // Admin i�in e-posta do�rulama gerekmiyor
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "��retmen");
            }
        }
    }

    // Sorular� ekleme fonksiyonu
    private static async Task SeedQuestions(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // E�er veritaban�nda s�nav yoksa bir tane olu�tur
        if (!context.Exams.Any())
        {
            var exam = new Exam
            {
                Title = "HTML ve CSS Test S�nav�",
                DurationInMinutes = 10
            };
            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            // Sorular� ekle
            var questions = new List<Question>
            {
                new Question
                {
                    Content = "A�a��dakilerden hangisi en b�y�k ba�l�k etiketidir?",
                    OptionA = "h1",
                    OptionB = "h2",
                    OptionC = "h3",
                    OptionD = "h4",
                    CorrectAnswer = "A",
                    ExamId = exam.Id
                },
                new Question
                {
                    Content = "Bir HTML belgesinde CSS dosyas�n� ba�lamak i�in hangi etiket kullan�l�r?",
                    OptionA = "<css>",
                    OptionB = "<script>",
                    OptionC = "<link>",
                    OptionD = "<style>",
                    CorrectAnswer = "C",
                    ExamId = exam.Id
                },
                // Di�er sorular� buraya ekleyebilirsiniz
            };

            context.Questions.AddRange(questions);
            await context.SaveChangesAsync();
        }
    }
}




