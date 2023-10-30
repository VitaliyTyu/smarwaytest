using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using SmartWay.WebApi.Data;
using SmartWay.WebApi.Extensions;
using SmartWay.WebApi.Interfaces;
using SmartWay.WebApi.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks().Services.AddHttpContextAccessor();
builder.Services.AddLazyCache();

builder.Services.AddDAL(builder.Configuration);
builder.Services.AddScoped<IFilesService, FilesService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Test01", Version = "v1" });

    opt.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."

    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme,
                }
            },
            new string[] {}
        }
    });
});

builder.Services
    .AddIdentityCore<IdentityUser>(opt =>
    {
        opt.Password.RequireDigit = false;
        opt.Password.RequiredLength = 1;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<IdentityUser>>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Token:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:Secret"])),
        };
    });            
            
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
}
else
{
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c => {  
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");  
});     

app.UseRouting();

app.UseHealthChecks("/health");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
