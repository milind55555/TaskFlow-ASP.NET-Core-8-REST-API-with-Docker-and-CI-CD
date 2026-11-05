using TaskFlow.Api.Dtos;
using TaskFlow.Api.Services;

namespace TaskFlow.Tests;

public class ProjectServiceTests
{
    [Fact]
    public async Task Create_then_list_returns_the_project()
    {
        var svc = new ProjectService(TestDb.Create());
        var user = Guid.NewGuid();

        await svc.CreateAsync(user, new ProjectRequest("Website", "Redesign"));
        var list = await svc.ListAsync(user);

        Assert.Single(list);
        Assert.Equal("Website", list[0].Name);
    }

    [Fact]
    public async Task Users_cannot_see_each_others_projects()
    {
        var svc = new ProjectService(TestDb.Create());
        var owner = Guid.NewGuid();
        var other = Guid.NewGuid();

        var created = await svc.CreateAsync(owner, new ProjectRequest("Private", null));

        Assert.Null(await svc.GetAsync(other, created.Id));
        Assert.Empty(await svc.ListAsync(other));
    }

    [Fact]
    public async Task Delete_removes_the_project()
    {
        var svc = new ProjectService(TestDb.Create());
        var user = Guid.NewGuid();
        var created = await svc.CreateAsync(user, new ProjectRequest("Temp", null));

        Assert.True(await svc.DeleteAsync(user, created.Id));
        Assert.Null(await svc.GetAsync(user, created.Id));
    }
}
