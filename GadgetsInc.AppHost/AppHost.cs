var builder = DistributedApplication.CreateBuilder(args);

// Setup AI services to be used in applications
var ollama = builder.AddOllama("ollama")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithOpenWebUI();
//
// llama3.2
var chat = ollama.AddModel("chat", "llama3.2");


// gpt-oss model
// var chat = ollama.AddModel("chat", "gpt-oss");



var apiService = builder.AddProject<Projects.GadgetsInc_ApiService>("apiservice")
    .WithReference(chat)
    .WithHttpHealthCheck("/health")
    .WaitFor(chat);

builder.AddProject<Projects.GadgetsInc_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();