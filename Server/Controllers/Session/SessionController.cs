using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Server.Models;

namespace Server.Controllers.Session
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ILogger<SessionController> _logger;
        private static readonly ConcurrentDictionary<Guid, bool> _captchaPendingFlags = new();
        private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> _sessionCancellationTokens = new();
        
        public SessionController(ILogger<SessionController> logger)
        {
            _logger = logger;
        }

        [HttpGet("ws")]
        public async Task Get(string fingerprint, string btcAddress, int redemptionToken)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            Guid sessionId = Guid.Empty;
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var (isValid, validatedSessionId) = await ValidateSessionAsync(ipAddress, fingerprint, redemptionToken);
                sessionId = validatedSessionId;
                if (isValid && sessionId != Guid.Empty)
                {
                    _logger.LogInformation($"Session started: {sessionId}");
                    
                    var cts = new CancellationTokenSource();
                    _sessionCancellationTokens.TryAdd(sessionId, cts);
                    
                    await SendMessageAsync(webSocket, "SESSION : " + sessionId);
                    StartSession(webSocket, sessionId, cts.Token);
                    
                    _logger.LogInformation("WebSocket opened");
                    await HandleTextInteraction(new Interaction
                    {
                        Type = "BtcAddress",
                        Data = btcAddress
                    }, sessionId);
                    
                    await GetMessageAsync(webSocket, sessionId);
                }
                else
                {
                    _logger.LogError("Invalid session");
                    await CloseAsync(webSocket, WebSocketCloseStatus.InternalServerError, "Invalid session");
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error");
                await CloseAsync(webSocket, WebSocketCloseStatus.InternalServerError, "Error", sessionId);
            }
            finally
            {
                if (sessionId != Guid.Empty)
                {
                    await CleanupSession(sessionId);
                }
            }
        }

        private async Task GetMessageAsync(WebSocket webSocket, Guid sessionId)
        {
            var buffer = new ArraySegment<byte>(new byte[64 * 1024]);
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseAsync(webSocket, WebSocketCloseStatus.NormalClosure, string.Empty, sessionId);
                        break;
                    }
                    if (buffer.Array == null)
                        return;
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        if (result.EndOfMessage)
                        {
                            var message = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, result.Count);
                            Interaction? interaction = null;
                            try
                            {
                                interaction = JsonSerializer.Deserialize<Interaction>(message);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to deserialize interaction");
                            }
                            if (interaction == null)
                                continue;
                            await HandleTextInteraction(interaction, sessionId);
                            _logger.LogInformation($"Message received: {message}");
                        }
                    }
                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        if (result.EndOfMessage)
                        {
                            // Binary message handling (if needed)
                        }
                    }
                }
                catch (WebSocketException)
                {
                    _logger.LogError("WebSocketException occurred, closing connection: " + sessionId);
                    break;
                }
            }
        }

        private async Task HandleTextInteraction(Interaction interaction, Guid sessionId)
        {
            if (string.IsNullOrWhiteSpace(interaction.Type) || string.IsNullOrWhiteSpace(interaction.Data))
                return;
            
            _logger.LogInformation($"Received interaction - Type: {interaction.Type}, Data: {interaction.Data}, SessionId: {sessionId}");
            
            // Check if this is a captcha response
            if (interaction.Type.Contains("Captcha", StringComparison.OrdinalIgnoreCase))
            {
                // Mark captcha as resolved to unfreeze the thread
                var wasUpdated = _captchaPendingFlags.AddOrUpdate(sessionId, false, (_, _) => false);
                _logger.LogInformation($"Captcha resolved for session: {sessionId}, flag updated to: {wasUpdated}");
                
                // Double check the flag was actually set
                var currentFlag = _captchaPendingFlags.GetValueOrDefault(sessionId, false);
                _logger.LogInformation($"Current captcha flag after update: {currentFlag}");
            }
            
            var callCenterService = Environment.GetEnvironmentVariable("CALLCENTERSERVICE");
            if (string.IsNullOrWhiteSpace(callCenterService))
            {
                _logger.LogError("CALLCENTERSERVICE environment variable not set");
                return;
            }
            
            using HttpClient httpClient = new();
            var body = JsonSerializer.Serialize(interaction);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{callCenterService}/Interaction/create?sessionId={sessionId}", content);
            if (!response.IsSuccessStatusCode)
                _logger.LogError($"Error creating interaction: {response.StatusCode} | {response.ReasonPhrase}");
            else
                _logger.LogInformation($"Interaction created: {interaction.Type}");
        }

        private async Task SendMessageAsync(WebSocket webSocket, string message)
        {
            if (webSocket.State != WebSocketState.Open)
                return;
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(messageBuffer);
            await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task<(bool isValid, Guid sessionId)> ValidateSessionAsync(string ipAddress, string fingerprint, int redemptionToken)
        {
            var callCenterService = Environment.GetEnvironmentVariable("CALLCENTERSERVICE");
            if (string.IsNullOrWhiteSpace(callCenterService))
            {
                _logger.LogError("CALLCENTERSERVICE environment variable not set");
                return (false, Guid.Empty);
            }

            using HttpClient httpClient = new();
            var url = $"{callCenterService}/Session/start?redemptionToken={redemptionToken}&sessionType=Web&sessionFingerprint={fingerprint}&sessionIpAddress={ipAddress}";
            var response = await httpClient.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error starting session: {response.StatusCode} | {response.ReasonPhrase} using {url}");
                return (false, Guid.Empty);
            }
                
            var sessionGuid = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(sessionGuid))
                return (false, Guid.Empty);
            var sessionId = Guid.TryParse(sessionGuid, out var guid) ? guid : Guid.Empty;
            return (sessionId != Guid.Empty, sessionId);
        }

        private void StartSession(WebSocket webSocket, Guid sessionId, CancellationToken cancellationToken)
        {
            var random = new Random();
            var randomNumber = random.Next(25, 45);
            
            _ = Task.Run(async () =>
            {
                try
                {
                    var stepSeparation = randomNumber / 3;
                    for (var i = 0; i < randomNumber; i++)
                    {
                        if (webSocket.State != WebSocketState.Open || cancellationToken.IsCancellationRequested)
                            break;
                        
                        if (i % stepSeparation == 0)
                            await SendMessageAsync(webSocket, "progress");
                        
                        // 30% chance to send captcha if none is currently pending
                        if (!_captchaPendingFlags.GetValueOrDefault(sessionId, false) && random.Next(100) < 30)
                        {
                            await SendMessageAsync(webSocket, "Captcha");
                            _captchaPendingFlags.AddOrUpdate(sessionId, true, (_, _) => true);
                            _logger.LogInformation($"Captcha sent for session: {sessionId}, waiting for response...");
                            
                            // Freeze the thread until captcha is resolved
                            while (_captchaPendingFlags.GetValueOrDefault(sessionId, false))
                            {
                                if (webSocket.State != WebSocketState.Open || cancellationToken.IsCancellationRequested)
                                    break;
                                
                                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                            }
                            
                            _logger.LogInformation($"Captcha resolved for session: {sessionId}, continuing session...");
                        }
                        
                        await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
                    }
                    
                    if (webSocket.State == WebSocketState.Open)
                        await CloseAsync(webSocket, WebSocketCloseStatus.NormalClosure, "Completed", sessionId);
                }
                catch (OperationCanceledException)
                {
                    // Session was cancelled, this is expected
                    _logger.LogInformation($"Session task cancelled for session: {sessionId}");
                }
            }, cancellationToken);
        }

        private async Task CleanupSession(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
                return;
                
            // Cancel the session task if it's still running
            if (_sessionCancellationTokens.TryRemove(sessionId, out var cts))
            {
                await cts.CancelAsync();
                cts.Dispose();
            }
            
            // Clean up the captcha flag
            _captchaPendingFlags.TryRemove(sessionId, out _);
            
            // End the session on the backend
            try
            {
                var callCenterService = Environment.GetEnvironmentVariable("CALLCENTERSERVICE");
                if (string.IsNullOrWhiteSpace(callCenterService))
                {
                    _logger.LogError("CALLCENTERSERVICE environment variable not set");
                    return;
                }
                
                using HttpClient httpClient = new();
                var response = await httpClient.PutAsync($"{callCenterService}/Session/end?sessionId={sessionId}", null);
                if (!response.IsSuccessStatusCode)
                    _logger.LogError("Error ending session: {statusCode} | {reasonPhrase}", response.StatusCode, await response.Content.ReadAsStringAsync() ?? string.Empty);
                else
                    _logger.LogInformation("Session ended: {session}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session for {sessionId}", sessionId);
            }
        }

        private async Task CloseAsync(WebSocket webSocket, WebSocketCloseStatus closeStatus,
            string statusDescription, Guid sessionId = default)
        {
            try
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                    await webSocket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
                
                _logger.LogInformation("WebSocket closed for session: {session}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing WebSocket for session: {sessionId}", sessionId);
            }
        }
    }
}