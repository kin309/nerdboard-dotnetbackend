using System.Security.Claims;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

[Authorize]
public class TicTacToeHub : Hub
{
    private readonly RoomService _roomService;
    private readonly FirebaseAuth _firebaseAuth;
    private readonly ILogger<TicTacToeHub> _logger;
    private readonly Dictionary<string, TicTacToeLogic> _activeGames = new();

    public TicTacToeHub(RoomService roomService, ILogger<TicTacToeHub> logger)
    {
        _roomService = roomService;
        _firebaseAuth = FirebaseAuth.DefaultInstance;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var user = Context.User;
            var connectionId = Context.ConnectionId;

            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = user.FindFirst(ClaimTypes.Name)?.Value;

                _logger.LogInformation("[CONNECT] User {Username} ({UserId}) connected with connection {ConnectionId}",
                    username, userId, connectionId);

                await Groups.AddToGroupAsync(connectionId, $"User-{userId}");
            }
            else
            {
                _logger.LogWarning("[CONNECT] Unauthenticated connection attempt: {ConnectionId}", connectionId);
                Context.Abort();
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CONNECT] Error during connection for {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[DISCONNECT] Unauthenticated user disconnected: {ConnectionId}", connectionId);
                return;
            }

            if (exception != null)
            {
                _logger.LogError(exception, "[DISCONNECT] User {UserId} disconnected with error", userId);
            }
            else
            {
                _logger.LogInformation("[DISCONNECT] User {UserId} disconnected normally", userId);
            }

            // Clean up any games the user was in
            foreach (var roomId in _activeGames.Keys.Where(k =>
                _roomService.GetUsersInRoom(k).Any(u => u.Id == userId)))
            {
                await HandlePlayerDisconnect(roomId, userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DISCONNECT] Error during disconnection for user {UserId}", userId);
            throw;
        }
    }

    public async Task JoinGame(string roomId)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[JOIN] Unauthenticated join attempt for game {RoomId}", roomId);
                return;
            }

            _logger.LogDebug("[JOIN] User {UserId} joining game {RoomId}", userId, roomId);

            // Get user info from Firebase
            UserRecord userRecord = await _firebaseAuth.GetUserAsync(userId);
            var user = new User
            {
                Id = userRecord.Uid,
                Username = userRecord.DisplayName ?? "Anonymous"
            };

            // Add to room if not already present
            if (!_roomService.GetUsersInRoom(roomId).Any(u => u.Id == userId))
            {
                _roomService.AddUserToRoom(roomId, user.Id, user.Username);
                await Groups.AddToGroupAsync(connectionId, roomId);
                await NotifyRoomUpdate(roomId);
            }

            // Initialize game if not already active
            if (!_activeGames.ContainsKey(roomId))
            {
                var players = _roomService.GetUsersInRoom(roomId)
                    .Select(u => new TicTacToePlayer(u.Username, u.Id))
                    .ToList();

                if (players.Count >= 2) // Only start game if at least 2 players
                {
                    var gameLogic = new TicTacToeLogic(players);
                    gameLogic.AddListener(async state => await OnGameStateChanged(roomId, state));
                    _activeGames[roomId] = gameLogic;

                    _logger.LogInformation("[GAME] Starting new TicTacToe game in room {RoomId}", roomId);
                    await Clients.Group(roomId).SendAsync("GameStarted", gameLogic.GetGameState());
                }
            }
            else
            {
                // Send current game state to joining player
                await Clients.Caller.SendAsync("GameStateUpdated", _activeGames[roomId].GetGameState());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[JOIN] Error joining game {RoomId} for user {UserId}", roomId, userId);
            throw;
        }
    }

    public async Task MakeMove(string roomId, int row, int col)
    {
        var userId = Context.UserIdentifier;

        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[MOVE] Unauthenticated move attempt in room {RoomId}", roomId);
                return;
            }

            if (!_activeGames.TryGetValue(roomId, out var gameLogic))
            {
                _logger.LogWarning("[MOVE] Game not found in room {RoomId}", roomId);
                return;
            }

            var player = gameLogic.GetPlayers().FirstOrDefault(p => p.Id == userId);
            if (player == null)
            {
                _logger.LogWarning("[MOVE] Player {UserId} not in game {RoomId}", userId, roomId);
                return;
            }

            if (gameLogic.IsPlayerTurn(player) && !gameLogic.HasEnded())
            {
                gameLogic.Play([row, col]);
                _logger.LogInformation("[MOVE] Player {UserId} made move at ({Row},{Col}) in room {RoomId}",
                    userId, row, col, roomId);
            }
            else
            {
                _logger.LogWarning("[MOVE] Invalid move attempt by {UserId} in room {RoomId}", userId, roomId);
                await Clients.Caller.SendAsync("InvalidMove", "It's not your turn or the game has ended");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MOVE] Error processing move in room {RoomId} by user {UserId}", roomId, userId);
            throw;
        }
    }

    public async Task LeaveGame(string roomId)
    {
        var userId = Context.UserIdentifier;

        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[LEAVE] Unauthenticated leave attempt for game {RoomId}", roomId);
                return;
            }

            _logger.LogDebug("[LEAVE] User {UserId} leaving game {RoomId}", userId, roomId);

            await HandlePlayerDisconnect(roomId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LEAVE] Error leaving game {RoomId} for user {UserId}", roomId, userId);
            throw;
        }
    }

    private async Task HandlePlayerDisconnect(string roomId, string userId)
    {
        // Remove from room
        _roomService.RemoveUserFromRoom(roomId, userId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

        // Handle game cleanup if needed
        if (_activeGames.TryGetValue(roomId, out var game))
        {
            var players = _roomService.GetUsersInRoom(roomId);
            if (players.Count < 2) // End game if not enough players
            {
                _activeGames.Remove(roomId);
                await Clients.Group(roomId).SendAsync("GameEnded", "Game ended due to player disconnect");
                _logger.LogInformation("[GAME] Ending game in room {RoomId} due to player disconnect", roomId);
            }
        }

        await NotifyRoomUpdate(roomId);
    }

    private async Task OnGameStateChanged(string roomId, TicTacToeState gameState)
    {
        try
        {
            await Clients.Group(roomId).SendAsync("GameStateUpdated", gameState);

            if (gameState.PlayerWin != null || gameState.Draw)
            {
                _logger.LogInformation("[GAME] Game ended in room {RoomId}", roomId);
                await Clients.Group(roomId).SendAsync("GameEnded", gameState);
                _activeGames.Remove(roomId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GAME] Error sending game state update for room {RoomId}", roomId);
        }
    }

    private async Task NotifyRoomUpdate(string roomId)
    {
        var roomInfo = new
        {
            RoomId = roomId,
            Users = _roomService.GetUsersInRoom(roomId),
            GameActive = _activeGames.ContainsKey(roomId)
        };

        await Clients.Group(roomId).SendAsync("RoomUpdated", roomInfo);
    }
}