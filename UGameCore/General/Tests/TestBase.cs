using UnityEngine;

namespace UGameCore.Tests
{
    public abstract class TestBase
    {
        protected T GetSingleObject<T>()
            where T : Component
        {
            T[] objects = Object.FindObjectsOfType<T>();
            
            if (objects.Length == 0)
                throw new System.InvalidOperationException($"Object of type {typeof(T).Name} not found");

            if (objects.Length > 1)
                throw new System.InvalidOperationException($"Found multiple ({objects.Length}) objects of type {typeof(T).Name} in the scene. " +
                    $"Make sure there is only 1 object of this type in the scene.");

            return objects[0];
        }
    }
}
