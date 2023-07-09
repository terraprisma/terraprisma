using System;

namespace Terraprisma.Patcher;

/// <summary>
///     Test class.
/// </summary>
public class TestDocClass {
    /// <summary>
    ///     Test.
    /// </summary>
    public event Action? TestEventAction;

    /// <summary>
    ///     Test.
    /// </summary>
    public event Action<int>? TestEventActionInt;

    /// <summary>
    ///     Test.
    /// </summary>
    public event Action<int, string>? TestEventActionIntString;

    /// <summary>
    ///     Test.
    /// </summary>
    /// <returns>Returns.</returns>
    /// <remarks>Remarks.</remarks>
    public TestDocClass() { }

    /// <summary>
    ///     Test.
    /// </summary>
    /// <param name="a">a</param>
    /// <returns>Returns.</returns>
    /// <remarks>Remarks.</remarks>
    public TestDocClass(string a) { }

    /// <summary>
    ///     Test.
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <returns>Returns.</returns>
    /// <remarks>Remarks.</remarks>
    public TestDocClass(string a, int b) { }

    /// <summary>
    ///     Test.
    /// </summary>
    /// <returns>Returns.</returns>
    /// <remarks>Remarks.</remarks>
    public void TestMethod() { }

    /// <summary>
    ///     Test.
    /// </summary>
    /// <param name="a">a</param>
    /// <returns>Returns.</returns>
    /// <remarks>Remarks.</remarks>
    public void TestMethod(string a) { }

    /// <summary>
    ///     Test.
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <returns>Returns.</returns>
    /// <remarks>Remarks.</remarks>
    public void TestMethod(string a, int b) { }

    /// <summary>
    ///     Test.
    /// </summary>
    /// <typeparam name="T">t</typeparam>
    /// <returns>Returns.</returns>
    /// <remarks>Remarks.</remarks>
    public void TestMethod<T>() { }

    /// <summary>
    ///     Test.
    /// </summary>
    /// <param name="a">a</param>
    /// <typeparam name="T">t</typeparam>
    /// <returns>Returns.</returns>
    /// <remarks>Remarks.</remarks>
    public void TestMethod<T>(T a) { }

    /// <summary>
    ///     Test.
    /// </summary>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <typeparam name="T">t</typeparam>
    /// <returns>Returns.</returns>
    /// <remarks>Remarks.</remarks>
    public void TestMethod<T>(T a, string b) { }

    /// <summary>
    ///     Test.
    /// </summary>
    /// <returns>Returns.</returns>
    /// <remarks>Remarks.</remarks>
    public int TestProperty { get; set; }
}
