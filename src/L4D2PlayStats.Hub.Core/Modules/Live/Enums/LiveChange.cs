namespace L4D2PlayStats.Hub.Core.Modules.Live.Enums;

[Flags]
public enum LiveChange
{
    None = 0,
    Header = 1,
    Players = 2,
    Feed = 4
}