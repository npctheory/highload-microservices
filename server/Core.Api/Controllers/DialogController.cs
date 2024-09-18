using Grpc.Net.Client;
using Dialogs.Api.Grpc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Core.Api.DTO;
using Grpc.Core;
using System.Text.Json;

namespace Core.Api.Controllers;

[ApiController]
public class DialogController : ControllerBase
{
    private readonly DialogService.DialogServiceClient _dialogServiceClient;

    public DialogController(DialogService.DialogServiceClient dialogServiceClient)
    {
        _dialogServiceClient = dialogServiceClient;
    }

    [Authorize]
    [HttpGet("dialog/list")]
    public async Task<IActionResult> ListDialogs()
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        try
        {
            var response = await _dialogServiceClient.ListDialogsAsync(new ListDialogsRequest { UserId = userId });
            var dialogs = response.Agents.Select(agent => new AgentDTO(agent.AgentId)).ToList();
            return Ok(dialogs);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            return Unauthorized();
        }
    }

    [Authorize]
    [HttpGet("dialog/{agentId}/list")]
    public async Task<IActionResult> ListMessages([FromRoute] string agentId)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        try
        {
            var response = await _dialogServiceClient.ListMessagesAsync(new ListMessagesRequest { UserId = userId, AgentId = agentId });
            var messages = response.Messages.Select(message => new DialogMessageDTO
            {
                Id = Guid.Parse(message.Id),
                Text = message.Text,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                IsRead = message.IsRead,
                Timestamp = DateTime.Parse(message.Timestamp)
            }).ToList();
            return Ok(messages);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            return Unauthorized();
        }
    }

    [Authorize]
    [HttpPost("dialog/{receiverId}/send")]
    public async Task<IActionResult> SendMessage([FromRoute] string receiverId, [FromBody] JsonElement jsonElement)
    {
        var senderId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string text = jsonElement.GetProperty("text").GetString();

        try
        {
            var response = await _dialogServiceClient.SendMessageAsync(new SendMessageRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Text = text
            });
            var message = response.Message;
            var dialogMessageDto = new DialogMessageDTO
            {
                Id = Guid.Parse(message.Id),
                Text = message.Text,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                IsRead = message.IsRead,
                Timestamp = DateTime.Parse(message.Timestamp)
            };
            return Ok(dialogMessageDto);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            return Unauthorized();
        }
    }
}