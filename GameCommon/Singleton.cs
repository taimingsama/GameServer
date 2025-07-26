namespace GameCommon;

public class Singleton<T> where T : class
{
    private class InternalSingleton
    {
        public static readonly T InternalInstance =
            Activator.CreateInstance(typeof(T), true) as T
            ?? throw new InvalidOperationException($"无法找到 {typeof(T).FullName} 的私有无参构造函数，单例创建失败");
    }

    public static T Instance => InternalSingleton.InternalInstance;
}