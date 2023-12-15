#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;

/// <summary>
/// [DidReloadScripts]属性メソッドでコンパイルをフックしてコード生成→コンパイル→コード実行
/// </summary>
public static partial class TestCompile_DidReloadScripts
{
    private const string FILE_PATH = "Assets/Tests002/__Test.cs";

    // 出力するコードの雛形
    private static readonly string CODE_TEMPLATE = $@"
public static partial class {nameof(TestCompile_DidReloadScripts)}
{{
    static partial void TestLog()
    {{
        UnityEngine.Debug.Log(""#LOG_MESSAGE#"");
    }}
}}";

    static partial void TestLog();  // コード生成で実装するメソッド

    /// <summary>
    /// 状態
    /// </summary>
    private class State : ScriptableSingleton<State>
    {
        public int state = 0;
    }

    /// <summary>
    /// コード生成開始
    /// </summary>
    [MenuItem("Test/TestCompile_DidReloadScripts")]
    public static void BeginTest()
    {
        State.instance.state = 1;
        OnDidReloadScripts();
    }

    /// <summary>
    /// スクリプトのリロード時に呼ばれる。コード生成と実行の順番を制御する
    /// </summary>
    [DidReloadScripts]
    public static void OnDidReloadScripts()
    {
        var inst = State.instance;
        if(inst.state == 0) { return; }

        switch(inst.state)
        {
            case 1:
            {
                // 何らかコード生成
                var code = CODE_TEMPLATE.Replace("#LOG_MESSAGE#", "コード生成その1");
                File.WriteAllText(FILE_PATH, code, Encoding.UTF8);

                AssetDatabase.Refresh();
                CompilationPipeline.RequestScriptCompilation();

                inst.state++;
            }
            break;
            case 2:
            {
                // 生成したコード実行
                TestLog();

                // 何らかコード生成
                var code = CODE_TEMPLATE.Replace("#LOG_MESSAGE#", "コード生成その2");
                File.WriteAllText(FILE_PATH, code, Encoding.UTF8);

                AssetDatabase.Refresh();
                CompilationPipeline.RequestScriptCompilation();

                inst.state++;
            }
            break;
            case 3:
            {
                // 生成したコード実行
                TestLog();

                inst.state = 0;
            }
            break;
        }
    }
}
#endif
