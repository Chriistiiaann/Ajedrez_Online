﻿using backEndAjedrez.Chess_Game;
using backEndAjedrez.ChessGame.Pieces;
using backEndAjedrez.Models.Database;
using backEndAjedrez.Models.Database.Repositories;
using backEndAjedrez.Models.Database.Repository;
using backEndAjedrez.Models.Interfaces;
using backEndAjedrez.Models.Mappers;
using backEndAjedrez.Services;
using backEndAjedrez.WebSockets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace backEndAjedrez;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<DataContext>();
        builder.Services.AddScoped<UserMapper>();

        builder.Services.AddScoped<IPasswordHasher, PasswordService>();
        builder.Services.AddScoped<IFriendRepository, FriendRepository>();
        builder.Services.AddSingleton<IMatchMaking, MatchMakingService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        builder.Services.AddSingleton<StatusService>();
        builder.Services.AddScoped<SmartSearchService>();
        builder.Services.AddSingleton<FriendService>();
        builder.Services.AddSingleton<MatchMakingService>();
        builder.Services.AddScoped<WebSocketService>();

        builder.Services.AddSingleton<WebSocketNetwork>();
        builder.Services.AddSingleton<GameMoveHandler>();
        builder.Services.AddSingleton<ChatHandler>();
        builder.Services.AddSingleton<GameBoardManager>();
        builder.Services.AddSingleton<Handler>();

        builder.Services.AddScoped<middleware>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            string key = Environment.GetEnvironmentVariable("JWT_KEY");
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("JWT_KEY variable de entorno no esta configurada.");
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            };
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
        });
        var app = builder.Build();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            DataContext dbContext = scope.ServiceProvider.GetService<DataContext>();
            dbContext.Database.EnsureCreated();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseAuthentication();
        app.UseWebSockets();
        app.UseMiddleware<middleware>();

        app.UseHttpsRedirection();
        app.UseRouting();  
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStaticFiles();

        app.UseCors("AllowFrontend");

        app.MapControllers();

        app.Run();
    }
}
