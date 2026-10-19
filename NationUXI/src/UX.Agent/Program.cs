using System.CommandLine;
using UX.Agent.Cli;

return await RootCommandFactory.Create().InvokeAsync(args);
