using backEndAjedrez.Chess_Game;
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

        //Board board = new Board();
        //Console.WriteLine("¡Bienvenido al ajedrez! Turnos alternados entre blancas y negras.");
        //bool turnoBlancas = true;

        //while (true)
        //{
        //    board.PrintBoard();
        //    Console.WriteLine($"Turno de las {(turnoBlancas ? "Blancas" : "Negras")}");
        //    Console.WriteLine("¿El rey blanco está en jaque? " + (board.EstaEnJaque("White") ? "Sí" : "No"));
        //    Console.WriteLine("¿El rey negro está en jaque? " + (board.EstaEnJaque("Black") ? "Sí" : "No"));

        //    string colorActual = turnoBlancas ? "White" : "Black";
        //    string colorOponente = turnoBlancas ? "Black" : "White";

        //    if (board.EstaEnJaqueMate(colorOponente))
        //    {
        //        Console.WriteLine($"¡Jaque mate! Las {(turnoBlancas ? "Blancas" : "Negras")} ganan.");
        //        break;
        //    }

        //    Console.WriteLine("Ingrese el movimiento en formato 'x1 y1 x2 y2' (o 'exit' para salir):");
        //    string input = Console.ReadLine();
        //    if (input.ToLower() == "exit") break;

        //    string[] parts = input.Split(' ');
        //    if (parts.Length != 4 ||
        //        !int.TryParse(parts[0], out int startX) ||
        //        !int.TryParse(parts[1], out int startY) ||
        //        !int.TryParse(parts[2], out int endX) ||
        //        !int.TryParse(parts[3], out int endY))
        //    {
        //        Console.WriteLine("Formato inválido. Intente de nuevo.");
        //        continue;
        //    }

        //    Piece piece = board.GetPiece(startX, startY);
        //    if (piece == null || piece.Color != colorActual)
        //    {
        //        Console.WriteLine($"Solo puedes mover piezas {(turnoBlancas ? "blancas" : "negras")} en este turno.");
        //        continue;
        //    }

        //    if (!piece.IsValidMove(startX, startY, endX, endY, board))
        //    {
        //        Console.WriteLine("Movimiento inválido según las reglas de la pieza.");
        //        continue;
        //    }

        //    Piece piezaDestinoOriginal = board.GetPiece(endX, endY);
        //    Piece piezaOrigen = board.GetPiece(startX, startY);
        //    board.MovePiece(startX, startY, endX, endY);

        //    if (board.EstaEnJaque(colorActual))
        //    {
        //        Console.WriteLine($"Movimiento inválido: deja a tu rey {colorActual} en jaque.");
        //        board.Grid[startX, startY] = piezaOrigen;
        //        board.Grid[endX, endY] = piezaDestinoOriginal;
        //        continue;
        //    }

        //    if (board.EstaEnJaque(colorOponente))
        //    {
        //        Console.WriteLine($"¡Bien hecho! El rey {colorOponente} está en jaque.");
        //        if (board.EstaEnJaqueMate(colorOponente))
        //        {
        //            Console.WriteLine($"¡Jaque mate! Las {(turnoBlancas ? "Blancas" : "Negras")} ganan.");
        //            break;
        //        }
        //    }

        //    turnoBlancas = !turnoBlancas;
        //}

        //Console.WriteLine("Juego terminado.");


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
