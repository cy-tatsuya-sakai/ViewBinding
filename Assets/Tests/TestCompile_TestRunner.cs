using System.Collections;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// TestRunnerを使用してコード生成→コンパイル→コード実行
/// </summary>
public static partial class TestCompile_TestRunner
{
    private const string FILE_PATH = "Assets/Tests/__Test.cs";

    // 出力するコードの雛形
    private static readonly string CODE_TEMPLATE = $@"
public static partial class {nameof(TestCompile_TestRunner)}
{{
    static partial void TestLog()
    {{
        UnityEngine.Debug.Log(""#LOG_MESSAGE#"");
    }}
}}";

    static partial void TestLog();  // コード生成で実装するメソッド

    [UnityTest, Order(1)]
    [Timeout(3600000)]
    public static IEnumerator Test_001()
    {
        // 何らかコード生成
        var code = CODE_TEMPLATE.Replace("#LOG_MESSAGE#", "コード生成その1");
        File.WriteAllText(FILE_PATH, code, Encoding.UTF8);

        AssetDatabase.Refresh();
        yield return new RecompileScripts(false, true);
    }

    [UnityTest, Order(2)]
    [Timeout(3600000)]
    public static IEnumerator Test_002()
    {
        // 生成したコード実行
        TestLog();

        // 更にコード生成
        var code = CODE_TEMPLATE.Replace("#LOG_MESSAGE#", "コード生成その2");
        File.WriteAllText(FILE_PATH, code, Encoding.UTF8);

        AssetDatabase.Refresh();
        yield return new RecompileScripts(false, true);
    }

    [Test, Order(3)]
    [Timeout(3600000)]
    public static void Test_003()
    {
        // 更に生成したコード実行
        TestLog();
    }

    [MenuItem("Test/TestCompile_TestRunner")]
    public static void BeginTest()
    {
        var filter = new Filter
        {
            testMode       = TestMode.EditMode,
            testNames      = null,
            groupNames     = new string[] { "TestCompile_TestRunner" },
            categoryNames  = null,
            assemblyNames  = null,
            targetPlatform = null,
        };

        var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
        testRunnerApi.Execute(new ExecutionSettings(filter));
    }
}
