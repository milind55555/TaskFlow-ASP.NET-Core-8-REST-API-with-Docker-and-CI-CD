using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;

namespace TaskFlow.Api.Controllers;

[Authorize]
[Route("api/projects/{projectId:guid}/tasks")]
public class TasksController : ApiControllerBase
{
    private readonly ITaskService _tasks;
    public TasksController(ITaskService tasks) => _tasks = tasks;

    [HttpGet]
    public async Task<ActionResult<List<TaskResponse>>> List(Guid projectId, [FromQuery] TaskItemStatus? status)
    {
        var items = await _tasks.ListAsync(UserId, projectId, status);
        return items is null ? NotFound() : Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Get(Guid projectId, Guid id)
    {
        var t = await _tasks.GetAsync(UserId, projectId, id);
        return t is null ? NotFound() : Ok(t);
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(Guid projectId, TaskRequest request)
    {
        var t = await _tasks.CreateAsync(UserId, projectId, request);
        return t is null ? NotFound() : CreatedAtAction(nameof(Get), new { projectId, id = t.Id }, t);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Update(Guid projectId, Guid id, TaskRequest request)
    {
        var t = await _tasks.UpdateAsync(UserId, projectId, id, request);
        return t is null ? NotFound() : Ok(t);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid id) =>
        await _tasks.DeleteAsync(UserId, projectId, id) ? NoContent() : NotFound();
}
