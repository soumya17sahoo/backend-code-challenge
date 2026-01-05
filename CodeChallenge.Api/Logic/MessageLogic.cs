using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic;

public class MessageLogic : IMessageLogic
{
    private readonly IMessageRepository _repository;

    public MessageLogic(IMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
    {
        return await _repository.GetAllByOrganizationAsync(organizationId);
    }

    public async Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
    {
        return await _repository.GetByIdAsync(organizationId, id);
    }

    public async Task<Result> CreateMessageAsync(
        Guid organizationId,
        CreateMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return new ValidationError(new Dictionary<string, string[]>
            {
                { "Title", new[] { "Title is required." } }
            });
        }

        var existing = await _repository.GetByTitleAsync(organizationId, request.Title);
        if (existing != null)
        {
            return new Conflict($"Message with title '{request.Title}' already exists.");
        }

        var message = new Message
        {
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content,
            IsActive = true
        };

        var created = await _repository.CreateAsync(message);
        return new Created<Message>(created);
    }

    public async Task<Result> UpdateMessageAsync(
        Guid organizationId,
        Guid id,
        UpdateMessageRequest request)
    {
        var message = await _repository.GetByIdAsync(organizationId, id);
        if (message == null)
        {
            return new NotFound("Message not found.");
        }

        message.Title = request.Title;
        message.Content = request.Content;
        message.IsActive = request.IsActive;

        var updated = await _repository.UpdateAsync(message);
        if (updated == null)
        {
            return new NotFound("Message not found.");
        }

        return new Updated();
    }

    public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
    {
        var deleted = await _repository.DeleteAsync(organizationId, id);
        if (!deleted)
        {
            return new NotFound("Message not found.");
        }

        return new Deleted();
    }
}
