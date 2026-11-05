using System.ComponentModel.DataAnnotations;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Dtos;

public record RegisterRequest([Required, EmailAddress] string Email, [Required, MinLength(8)] string Password);
public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public record AuthResponse(string Token, DateTime ExpiresAtUtc);

public record ProjectRequest([Required, MaxLength(200)] string Name, string? Description);
public record ProjectResponse(Guid Id, string Name, string? Description, DateTime CreatedAt);

public record TaskRequest(
    [Required, MaxLength(200)] string Title,
    string? Description,
    TaskItemStatus Status = TaskItemStatus.Todo,
    TaskPriority Priority = TaskPriority.Medium,
    DateTime? DueDate = null);

public record TaskResponse(
    Guid Id, Guid ProjectId, string Title, string? Description,
    TaskItemStatus Status, TaskPriority Priority, DateTime? DueDate, DateTime CreatedAt);
