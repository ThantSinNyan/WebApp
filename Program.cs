var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
})
.AddCookie("Cookies")
.AddGoogle(options =>
{
    options.ClientId = "406147987450-ncj5etlt0ssanfs3rst9kbki04ba5edg.apps.googleusercontent.com";
    options.ClientSecret = "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";
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();