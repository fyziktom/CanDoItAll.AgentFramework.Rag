using CanDoItAll.AgentFramework.Rag.Driver.DependencyInjection;
using CanDoItAll.AgentFramework.Rag.Sandbox.Components;
using CanDoItAll.AgentFramework.Rag.Sandbox.Services;
using CanDoItAll.Components.BaseLib;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCanDoItAllBaseLib();
builder.Services.AddLocalHashingRagEmbeddingGenerator(options => options.Dimension = 384);
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<RagSandboxSimilarityCalculator>();
builder.Services.AddScoped<RagSandboxStore>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
