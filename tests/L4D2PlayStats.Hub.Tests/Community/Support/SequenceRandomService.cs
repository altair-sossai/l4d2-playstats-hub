using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

namespace L4D2PlayStats.Hub.Tests.Community.Support;

public class SequenceRandomService(params int[] values) : ICommunityRandomService
{
    private int _calls;

    public int Next(int exclusiveMaximum)
    {
        var value = values.Length == 0 ? 0 : values[_calls % values.Length];

        _calls++;

        return value % exclusiveMaximum;
    }
}
