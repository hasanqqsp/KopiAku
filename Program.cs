using DotNetEnv;
using MongoDB.Driver;
using Amazon.S3;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using KopiAku.Settings;
using KopiAku.Services;
using KopiAku.GraphQL;
using KopiAku.GraphQL.Users;
using KopiAku.GraphQL.Presences;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.Configure<JWTSettings>(
    builder.Configuration.GetSection("JWTSettings"));
builder.Services.Configure<B2Settings>(
    builder.Configuration.GetSection("B2Settings"));

builder.Services.AddSingleton<JWTService>();
builder.Services.AddSingleton(sp =>
{
    var Settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var client = new MongoClient(Settings.ConnectionString);
    return client.GetDatabase(Settings.DatabaseName);
});
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var b2Settings = sp.GetRequiredService<IOptions<B2Settings>>().Value;
    var credentials = new BasicAWSCredentials(b2Settings.AccessKey, b2Settings.SecretKey);
    var config = new AmazonS3Config
    {
        ServiceURL = b2Settings.BaseUrl,
        ForcePathStyle = true
    };
    return new AmazonS3Client(credentials, config);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JWTSettings").Get<JWTSettings>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings!.Issuer,
        ValidAudience = jwtSettings!.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SecretKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<UserQueries>()
    .AddTypeExtension<UserMutations>()
    .AddTypeExtension<PresenceQueries>()
    .AddTypeExtension<PresenceMutations>()
    .AddAuthorization()
    .AddMongoDbFiltering()
    .AddMongoDbSorting()
    .AddMongoDbProjections()
    .AddMongoDbPagingProviders();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.Run();
