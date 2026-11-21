using System.Reflection;

namespace L4D2PlayStats.Hub.Tests.Support;

public class CancellingProxy : DispatchProxy
{
    public const string Message = "The test cancelled this service on purpose";

    public static T Create<T>() where T : class
    {
        return Create<T, CancellingProxy>();
    }

    protected override object Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        throw new OperationCanceledException(Message);
    }
}
