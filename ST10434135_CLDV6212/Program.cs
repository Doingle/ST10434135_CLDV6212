using ST10434135_CLDV6212.Services;

var builder = WebApplication.CreateBuilder(args);

//region showing all cofigured and connected services
#region Configuration and Services

// Add services to the container.
builder.Services.AddControllersWithViews();

//Added the Azure Queue service
//added before Table service as Order service uses both
builder.Services.AddSingleton<QueueService>();

//Added the Azure Table Storage service
builder.Services.AddSingleton<TableStorageService>();

//Added the Azure Blob Storage service
builder.Services.AddSingleton<BlobStorageService>();

//Added the Azure File Storage service
builder.Services.AddSingleton<FileShareService>();

///Added the Order service (seperate from TableStorageService for business logic)
builder.Services.AddScoped<OrderService>();

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
