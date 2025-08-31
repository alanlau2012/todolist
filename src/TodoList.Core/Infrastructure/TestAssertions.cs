using TodoList.Core.Models;

namespace TodoList.Core.Infrastructure;

/// <summary>
/// 测试断言工具类
/// </summary>
public static class TestAssertions
{
    /// <summary>
    /// 断言条件为真
    /// </summary>
    public static void AssertTrue(bool condition, string message = "条件应该为真")
    {
        if (!condition)
        {
            throw new AssertionException(message);
        }
    }
    
    /// <summary>
    /// 断言条件为假
    /// </summary>
    public static void AssertFalse(bool condition, string message = "条件应该为假")
    {
        if (condition)
        {
            throw new AssertionException(message);
        }
    }
    
    /// <summary>
    /// 断言对象相等
    /// </summary>
    public static void AssertEqual<T>(T expected, T actual, string? message = null)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            var errorMessage = message ?? $"期望值: {expected}, 实际值: {actual}";
            throw new AssertionException(errorMessage);
        }
    }
    
    /// <summary>
    /// 断言对象不相等
    /// </summary>
    public static void AssertNotEqual<T>(T expected, T actual, string? message = null)
    {
        if (EqualityComparer<T>.Default.Equals(expected, actual))
        {
            var errorMessage = message ?? $"值不应该相等: {expected}";
            throw new AssertionException(errorMessage);
        }
    }
    
    /// <summary>
    /// 断言对象为空
    /// </summary>
    public static void AssertNull(object? obj, string message = "对象应该为空")
    {
        if (obj != null)
        {
            throw new AssertionException(message);
        }
    }
    
    /// <summary>
    /// 断言对象不为空
    /// </summary>
    public static void AssertNotNull(object? obj, string message = "对象不应该为空")
    {
        if (obj == null)
        {
            throw new AssertionException(message);
        }
    }
    
    /// <summary>
    /// 断言字符串为空或空白
    /// </summary>
    public static void AssertNullOrWhiteSpace(string? str, string message = "字符串应该为空或空白")
    {
        if (!string.IsNullOrWhiteSpace(str))
        {
            throw new AssertionException(message);
        }
    }
    
    /// <summary>
    /// 断言字符串不为空或空白
    /// </summary>
    public static void AssertNotNullOrWhiteSpace(string? str, string message = "字符串不应该为空或空白")
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new AssertionException(message);
        }
    }
    
    /// <summary>
    /// 断言集合包含指定元素
    /// </summary>
    public static void AssertContains<T>(IEnumerable<T> collection, T item, string? message = null)
    {
        if (!collection.Contains(item))
        {
            var errorMessage = message ?? $"集合应该包含元素: {item}";
            throw new AssertionException(errorMessage);
        }
    }
    
    /// <summary>
    /// 断言集合不包含指定元素
    /// </summary>
    public static void AssertDoesNotContain<T>(IEnumerable<T> collection, T item, string? message = null)
    {
        if (collection.Contains(item))
        {
            var errorMessage = message ?? $"集合不应该包含元素: {item}";
            throw new AssertionException(errorMessage);
        }
    }
    
    /// <summary>
    /// 断言集合为空
    /// </summary>
    public static void AssertEmpty<T>(IEnumerable<T> collection, string message = "集合应该为空")
    {
        if (collection.Any())
        {
            throw new AssertionException(message);
        }
    }
    
    /// <summary>
    /// 断言集合不为空
    /// </summary>
    public static void AssertNotEmpty<T>(IEnumerable<T> collection, string message = "集合不应该为空")
    {
        if (!collection.Any())
        {
            throw new AssertionException(message);
        }
    }
    
    /// <summary>
    /// 断言集合数量
    /// </summary>
    public static void AssertCount<T>(IEnumerable<T> collection, int expectedCount, string? message = null)
    {
        var actualCount = collection.Count();
        if (actualCount != expectedCount)
        {
            var errorMessage = message ?? $"集合数量应该为 {expectedCount}, 实际为 {actualCount}";
            throw new AssertionException(errorMessage);
        }
    }
    
    /// <summary>
    /// 断言异常被抛出
    /// </summary>
    public static T AssertThrows<T>(Action action, string? message = null) where T : Exception
    {
        try
        {
            action();
            var errorMessage = message ?? $"应该抛出异常类型: {typeof(T).Name}";
            throw new AssertionException(errorMessage);
        }
        catch (T ex)
        {
            return ex;
        }
        catch (Exception ex)
        {
            var errorMessage = message ?? $"期望异常类型: {typeof(T).Name}, 实际异常类型: {ex.GetType().Name}";
            throw new AssertionException(errorMessage);
        }
    }
    
    /// <summary>
    /// 断言异步操作抛出异常
    /// </summary>
    public static async Task<T> AssertThrowsAsync<T>(Func<Task> action, string? message = null) where T : Exception
    {
        try
        {
            await action();
            var errorMessage = message ?? $"应该抛出异常类型: {typeof(T).Name}";
            throw new AssertionException(errorMessage);
        }
        catch (T ex)
        {
            return ex;
        }
        catch (Exception ex)
        {
            var errorMessage = message ?? $"期望异常类型: {typeof(T).Name}, 实际异常类型: {ex.GetType().Name}";
            throw new AssertionException(errorMessage);
        }
    }
}

/// <summary>
/// 断言异常
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message)
    {
    }
    
    public AssertionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
