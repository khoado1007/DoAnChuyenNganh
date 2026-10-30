using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
namespace ITManagement.IntegrationTests;

public class AssetTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private async Task<HttpClient> Admin()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "Admin@12345" });
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.GetProperty("data").GetProperty("accessToken").GetString());
        return client;
    }
    private static async Task<JsonElement> Data(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }
    [Fact]
    public async Task Asset_details_search_update_and_terminal_retirement_persist()
    {
        using var admin = await Admin();
        var code = "ASSET-" + Guid.NewGuid().ToString("N");
        var created = await Data(await admin.PostAsJsonAsync("/api/assets", new { assetCode = code, name = "Workstation", type = "Laptop", serialNumber = "SERIAL-1", location = "Office" }));
        var id = created.GetProperty("id").GetGuid();
        var detail = await Data(await admin.GetAsync($"/api/assets/{id}"));
        Assert.Equal("Laptop", detail.GetProperty("type").GetString());
        Assert.Equal("SERIAL-1", detail.GetProperty("serialNumber").GetString());
        object Update(string status) => new { name = "Updated workstation", type = "Laptop", status, serialNumber = "SERIAL-2", manufacturer = "Test", model = "Model", location = "Lab", notes = "Managed" };
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PutAsJsonAsync($"/api/assets/{id}", Update("Assigned"))).StatusCode);
        await Data(await admin.PutAsJsonAsync($"/api/assets/{id}", Update("UnderMaintenance")));
        var page = await Data(await admin.GetAsync($"/api/assets/search?search={code}&status=UnderMaintenance"));
        Assert.Equal(1, page.GetProperty("totalCount").GetInt32());
        Assert.Equal("Lab", page.GetProperty("items")[0].GetProperty("location").GetString());
        await Data(await admin.PutAsJsonAsync($"/api/assets/{id}", Update("Retired")));
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PutAsJsonAsync($"/api/assets/{id}", Update("Available"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await admin.PostAsJsonAsync("/api/agents/enrollment-tokens", new { assetId = id, mode = "real" })).StatusCode);
        Assert.Equal("Retired", (await Data(await admin.GetAsync($"/api/assets/{id}"))).GetProperty("status").GetString());
    }
    [Fact]
    public async Task Asset_input_validation_and_unique_code_are_enforced()
    {
        using var admin = await Admin();
        var code = "ASSET-" + Guid.NewGuid().ToString("N");
        await Data(await admin.PostAsJsonAsync("/api/assets", new { assetCode = code, name = "PC" }));
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsJsonAsync("/api/assets", new { assetCode = code, name = "Duplicate" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.GetAsync("/api/assets/search?page=0&pageSize=10000")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PutAsJsonAsync($"/api/assets/{Guid.NewGuid()}", new { name = "", type = "Desktop", status = "Available" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/api/assets/{Guid.NewGuid()}")).StatusCode);
        using var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/assets/search")).StatusCode);
    }
}
