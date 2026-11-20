using System.Reflection;

namespace L4D2PlayStats.Hub.Tests.Support;

public class ThrowingProxy : DispatchProxy
{
    public const string Message = "The test broke this service on purpose";

    public static T Create<T>() where T : class
    {
        return Create<T, ThrowingProxy>();
    }

    protected override object Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        throw new InvalidOperationException(Message);
    }
}