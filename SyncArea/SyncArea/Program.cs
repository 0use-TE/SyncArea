using Blazor.QrCodeGen;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MudBlazor;
using MudBlazor.Services;
using SyncArea.Components;
using SyncArea.Identity;
using SyncArea.Identity.Models;
using SyncArea.Models.Options;
using SyncArea.Pages.Account;
using SyncArea.Services;

var builder = WebApplication.CreateBuilder(args);
var port = builder.Configuration["Port"];

builder.WebHost.ConfigureKestrel(x => x.ListenAnyIP(int.Parse(port ?? "")));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// ���� EF Core �� SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString), ServiceLifetime.Scoped);
// ���� Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthorizationHandler, WorkspaceMemberHandler>();

// ���� Cookie ��֤��30�컬�����ڣ�
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true; // ���û�������
    options.LoginPath = "/Login"; // ��¼·��
    options.LogoutPath = "/Account/Logout"; // �ǳ�·��
    options.AccessDeniedPath = "/AccessDenied"; // ���ʱ��ܾ�·��
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WorkspaceMember", policy =>
        policy.Requirements.Add(new WorkspaceMemberRequirement()));
});
//ҵ�����
//ע���·
builder.Services.AddScoped<CircuitHandler, MyCircuit>();
builder.Services.AddHostedService<InitHostService>();
builder.Services.AddScoped<UserCRUDService>();
builder.Services.AddScoped<ModuleCreator>();

// �������ñ��ֲ���
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<WorkspaceService>();
builder.Services.AddScoped<WorkItemService>();
builder.Services.AddScoped<ShareService>();
//���options
builder.Services.Configure<DefaultAdminModel>(builder.Configuration.GetSection("DefaultAdminModel"));

// Add MudBlazor services
builder.Services.AddMudServices(options => options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight);
builder.Services.AddRazorPages();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();

//webApi
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}

//app.UseHttpsRedirection();

var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images");
if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images"
});


app.UseAntiforgery();

app.MapRazorPages().WithStaticAssets();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

//������
app.MapControllers();
//��֤����Ȩ����
app.UseAuthentication();
app.UseAuthorization();

// **�Զ�Ǩ�ƴ���**
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
