using System;

namespace UGameCore
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientRpcAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ClientTargetRpcAttribute : Attribute
    {
    }
}
