using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MessagesController> _logger;
    private readonly IMessageLogic _messageLogic;

    public MessagesController(IMessageRepository repository, ILogger<MessagesController> logger, IMessageLogic messageLogic)
    {
        _repository = repository;
        _logger = logger;
        _messageLogic = messageLogic;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll(Guid organizationId)
    {
        var messages = await _messageLogic.GetAllMessagesAsync(organizationId);
        return Ok(messages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
        var message = await _messageLogic.GetMessageAsync(organizationId, id);

        if (message == null)
            return NotFound();

        return Ok(message);
    }

    [HttpPost]
    public async Task<ActionResult<Message>> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        var result = await _messageLogic.CreateMessageAsync(organizationId, request);

        return result switch
        {
            Created<Message> created =>
                CreatedAtAction(
                    nameof(GetById),
                    new { organizationId, id = created.Value.Id },
                    created.Value),

            Conflict conflict =>
                Conflict(conflict.Message),

            ValidationError validation =>
                BadRequest(validation.Errors),

            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        var result = await _messageLogic.UpdateMessageAsync(organizationId, id, request);

        return result switch
        {
            Updated =>
                NoContent(),

            NotFound notFound =>
                NotFound(notFound.Message),

            ValidationError validation =>
                BadRequest(validation.Errors),

            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid organizationId, Guid id)
    {
        var result = await _messageLogic.DeleteMessageAsync(organizationId, id);

        return result switch
        {
            Deleted =>
                NoContent(),

            NotFound notFound =>
                NotFound(notFound.Message),

            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
