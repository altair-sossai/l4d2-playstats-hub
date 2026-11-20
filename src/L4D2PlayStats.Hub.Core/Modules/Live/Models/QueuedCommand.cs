namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public record QueuedCommand(int LogId, ServerCommand Command, DateTime QueuedAtUtc);