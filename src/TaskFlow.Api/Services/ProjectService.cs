using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Models;

namespace TaskFlow.Api.Services;

public interface IProjectService
{
    Task<List<ProjectResponse>> ListAsync(Guid userId);
    Task<ProjectResponse?> GetAsync(Guid userId, Guid id);
    Task<ProjectResponse> CreateAsync(Guid userId, ProjectRequest request);
    Task<ProjectResponse?> UpdateAsync(Guid userId, Guid id, ProjectRequest request);
    Task<bool> DeleteAsync(Guid userId, Guid id);
}

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    public ProjectService(AppDbContext db) => _db = db;

    public async Task<List<ProjectResponse>> ListAsync(Guid userId)
    {
        var items = await _db.Projects.Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.CreatedAt).ToListAsync();
        return items.Select(ToResponse).ToList();
    }

    public async Task<ProjectResponse?> GetAsync(Guid userId, Guid id)
    {
        var p = await _db.Projects.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId);
        return p is null ? null : ToResponse(p);
    }

    public async Task<ProjectResponse> CreateAsync(Guid userId, ProjectRequest request)
    {
        var p = new Project { Name = request.Name.Trim(), Description = request.Description, OwnerId = userId };
        _db.Projects.Add(p);
        await _db.SaveChangesAsync();
        return ToResponse(p);
    }

    public async Task<ProjectResponse?> UpdateAsync(Guid userId, Guid id, ProjectRequest request)
    {
        var p = await _db.Projects.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId);
        if (p is null) return null;
        p.Name = request.Name.Trim();
        p.Description = request.Description;
        await _db.SaveChangesAsync();
        return ToResponse(p);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id)
    {
        var p = await _db.Projects.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId);
        if (p is null) return false;
        _db.Projects.Remove(p);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ProjectResponse ToResponse(Project p) => new(p.Id, p.Name, p.Description, p.CreatedAt);
}
