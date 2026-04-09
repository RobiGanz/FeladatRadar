using Blazored.LocalStorage;
using FeladatRadar.frontend.Components;
using FeladatRadar.frontend.Services;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://api.feladatradar.hu/")
});
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddBlazoredLocalStorage();

// FocusTimerService: singleton, hogy navigáció között is megmaradjon az állapot
builder.Services.AddSingleton<FocusTimerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
