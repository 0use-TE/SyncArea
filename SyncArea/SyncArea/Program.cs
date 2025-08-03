using Blazor.QrCodeGen;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;
using SyncArea.Components;
using SyncArea.Identity;
using SyncArea.Identity.Models;
using SyncArea.Models.Options;
using SyncArea.Pages.Account;
using SyncArea.Services;

var builder = WebApplication.CreateBuilder(args);
//当前是生产环境的话
if (builder.Environment.IsProduction())
{
    var port = builder.Configuration["Port"];

    builder.WebHost.ConfigureKestrel(x => x.ListenAnyIP(int.Parse(port ?? "")));
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// 配置 EF Core 和 SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString), ServiceLifetime.Scoped);
// 配置 Identity
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

// 配置 Cookie 认证（30天滑动过期）
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true; // 启用滑动过期
    options.LoginPath = "/Login"; // 登录路径
    options.LogoutPath = "/Account/Logout"; // 登出路径
    options.AccessDeniedPath = "/AccessDenied"; // 访问被拒绝路径
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WorkspaceMember", policy =>
        policy.Requirements.Add(new WorkspaceMemberRequirement()));
});
//业务服务
//注册电路
builder.Services.AddScoped<CircuitHandler, MyCircuit>();
builder.Services.AddHostedService<InitHostService>();
builder.Services.AddScoped<UserCRUDService>();
builder.Services.AddScoped<ModuleCreator>();
builder.Services.AddScoped<ImageBuildService>();

// 其他配置保持不变
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<WorkspaceService>();
builder.Services.AddScoped<WorkItemService>();
builder.Services.AddScoped<ShareService>();
//添加options
builder.Services.Configure<DefaultAdminModel>(builder.Configuration.GetSection("DefaultAdminModel"));
builder.Services.Configure<ImagesPathModel>(builder.Configuration.GetSection("ImagesPathModel"));

// Add MudBlazor services
builder.Services.AddMudServices(options => options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight);
builder.Services.AddScoped<MudBreakpointProvider>();

builder.Services.AddBlazoredLocalStorage();
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

var imagesPath = app.Services.GetService<IOptions<ImagesPathModel>>()?.Value.ImagePath;
if (string.IsNullOrEmpty(imagesPath))
{
    throw new InvalidOperationException("图片路径没有配置!!!");
}
if (!Directory.Exists(imagesPath))
    Directory.CreateDirectory(imagesPath);

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

//控制器
app.MapControllers();
//认证与授权服务
app.UseAuthentication();
app.UseAuthorization();

// **自动迁移代码**
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
