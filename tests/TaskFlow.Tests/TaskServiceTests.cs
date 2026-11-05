using TaskFlow.Api.Dtos;
using TaskFlow.Api.Models;
using TaskFlow.Api.Services;

namespace TaskFlow.Tests;

public class TaskServiceTests
{
    [Fact]
    public async Task Create_task_and_filter_by_status()
    {
        var db = TestDb.Create();
        var projects = new ProjectService(db);
        var tasks = new TaskService(db);
        var user = Guid.NewGuid();
        var project = await projects.CreateAsync(user, new ProjectRequest("Sprint 1", null));

        await tasks.CreateAsync(user, project.Id, new TaskRequest("Write API", null, TaskItemStatus.Todo, TaskPriority.High));
        await tasks.CreateAsync(user, project.Id, new TaskRequest("Write docs", null, TaskItemStatus.Done, TaskPriority.Low));

        var done = await tasks.ListAsync(user, project.Id, TaskItemStatus.Done);

        Assert.NotNull(done);
        Assert.Single(done!);
        Assert.Equal("Write docs", done![0].Title);
    }

    [Fact]
    public async Task Cannot_add_task_to_someone_elses_project()
    {
        var db = TestDb.Create();
        var projects = new ProjectService(db);
        var tasks = new TaskService(db);
        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();
        var project = await projects.CreateAsync(owner, new ProjectRequest("Secret", null));

        var result = await tasks.CreateAsync(intruder, project.Id, new TaskRequest("Hack", null));

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_changes_status()
    {
        var db = TestDb.Create();
        var projects = new ProjectService(db);
        var tasks = new TaskService(db);
        var user = Guid.NewGuid();
        var project = await projects.CreateAsync(user, new ProjectRequest("P", null));
        var task = (await tasks.CreateAsync(user, project.Id, new TaskRequest("T", null)))!;

        var updated = await tasks.UpdateAsync(user, project.Id, task.Id, new TaskRequest("T", null, TaskItemStatus.InProgress));

        Assert.Equal(TaskItemStatus.InProgress, updated!.Status);
    }
}
