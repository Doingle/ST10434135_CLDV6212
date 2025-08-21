using ST10434135_CLDV6212.Services;

var builder = WebApplication.CreateBuilder(args);

#region Configuration and Services

// Add services to the container.
builder.Services.AddControllersWithViews();

//Added the Azure Table Storage service
builder.Services.AddSingleton<TableStorageService>();

//Added the Azure Blob Storage service
builder.Services.AddSingleton<BlobStorageService>();

//Added the Azure File Storage service
builder.Services.AddSingleton<FileShareService>();


#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
