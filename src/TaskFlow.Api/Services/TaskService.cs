using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public interface ITaskService
{
    Task<List<TaskResponse>?> ListAsync(Guid userId, Guid projectId, TaskItemStatus? status);
    Task<TaskResponse?> GetAsync(Guid userId, Guid projectId, Guid id);
    Task<TaskResponse?> CreateAsync(Guid userId, Guid projectId, TaskRequest request);
    Task<TaskResponse?> UpdateAsync(Guid userId, Guid projectId, Guid id, TaskRequest request);
    Task<bool> DeleteAsync(Guid userId, Guid projectId, Guid id);
}

public class TaskService : ITaskService
{
    private readonly AppDbContext _db;
    public TaskService(AppDbContext db) => _db = db;

    private Task<bool> OwnsProject(Guid userId, Guid projectId) =>
        _db.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId);

    public async Task<List<TaskResponse>?> ListAsync(Guid userId, Guid projectId, TaskItemStatus? status)
    {
        if (!await OwnsProject(userId, projectId)) return null;
        var query = _db.Tasks.Where(t => t.ProjectId == projectId);
        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        var items = await query.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate).ToListAsync();
        return items.Select(ToResponse).ToList();
    }

    public async Task<TaskResponse?> GetAsync(Guid userId, Guid projectId, Guid id)
    {
        if (!await OwnsProject(userId, projectId)) return null;
        var t = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.ProjectId == projectId);
        return t is null ? null : ToResponse(t);
    }

    public async Task<TaskResponse?> CreateAsync(Guid userId, Guid projectId, TaskRequest request)
    {
        if (!await OwnsProject(userId, projectId)) return null;
        var t = new TaskItem
        {
            ProjectId = projectId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate
        };
        _db.Tasks.Add(t);
        await _db.SaveChangesAsync();
        return ToResponse(t);
    }

    public async Task<TaskResponse?> UpdateAsync(Guid userId, Guid projectId, Guid id, TaskRequest request)
    {
        if (!await OwnsProject(userId, projectId)) return null;
        var t = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.ProjectId == projectId);
        if (t is null) return null;
        t.Title = request.Title.Trim();
        t.Description = request.Description;
        t.Status = request.Status;
        t.Priority = request.Priority;
        t.DueDate = request.DueDate;
        await _db.SaveChangesAsync();
        return ToResponse(t);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid projectId, Guid id)
    {
        if (!await OwnsProject(userId, projectId)) return false;
        var t = await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.ProjectId == projectId);
        if (t is null) return false;
        _db.Tasks.Remove(t);
        await _db.SaveChangesAsync();
        return true;
    }

    private static TaskResponse ToResponse(TaskItem t) =>
        new(t.Id, t.ProjectId, t.Title, t.Description, t.Status, t.Priority, t.DueDate, t.CreatedAt);
}
