using Microsoft.EntityFrameworkCore;
using UX.ControlPlane.Data;
using UX.ControlPlane.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UxDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<TelemetryService>();
builder.Services.AddScoped<OrgTreeService>();
builder.Services.AddScoped<ConfigResolverService>();
builder.Services.AddScoped<ConfigPublisherService>();

builder.Services.AddHttpContextAccessor();

// HttpClient for Blazor Server components calling same app APIs
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();

    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001";
    if (!baseUrl.EndsWith("/")) baseUrl += "/";
    httpClient.BaseAddress = new Uri(baseUrl);

    return httpClient;
});

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UxDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
