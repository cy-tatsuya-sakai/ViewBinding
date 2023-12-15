using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor.Compilation;
using System;
using System.Runtime.CompilerServices;
using UnityEditor.Callbacks;
using UnityEngine.UI;
using TMPro;

namespace ViewBinding
{
    /// <summary>
    /// バインディングクラスを生成
    /// </summary>
    public static partial class ViewBindingEditor
    {
        // プロジェクトごとの設定
        // --------ここから--------
        private const string SRC_PREFAB_PATH = "Assets/UI/Layouts/Prefabs"; // UIプレハブの格納フォルダ
        private const string DST_SCRIPT_PATH = "Assets/UI/Layouts/Scripts"; // 生成したバインディングクラスの保存先
        private const string DST_SCRIPT_SUFFIX = "";                        // 生成したバインディングクラス名のサフィックス。何か付けたければ
        private static readonly Type[] BIND_TYPE_LIST = new Type[]          // バインドしたい型
        {
            typeof(CanvasGroup),
            typeof(Button),
            typeof(TextMeshProUGUI),
        };
        // --------ここまで--------

        // コード生成でpartialメソッドを実装するファイル名
        private static readonly string FILE_NAME_WRITE  = $"{nameof(ViewBindingEditor)}_Write.cs";
        private static readonly string FILE_NAME_ATTACH = $"{nameof(ViewBindingEditor)}_Attach.cs";

        // コード生成で実装するpartialメソッド
        static partial void WriteViewBinding(GameObject obj, string format, StringBuilder sb);  // バインディングクラスの各要素を書き出し
        static partial void RemoveChildViewBinding(GameObject obj);                             // バインディングクラスの余分な子オブジェクトを削除
        static partial void AttachViewBinding();                                                // バインディングクラスにアタッチ

        /// <summary>
        /// コード生成の状態
        /// </summary>
        private class Parameter : ScriptableSingleton<Parameter>
        {
            public enum State
            {
                None,
                Prepare,    // 前処理
                Generate,   // コード生成
                Attach,     // アタッチ
            }

            public State state = State.None;
        }

        /// <summary>
        /// バインディングクラスを生成
        /// </summary>
        [MenuItem("Tools/ViewBinding/Generate ViewBinding All")]
        public static void GenerateViewBindingAll()
        {
            var inst = Parameter.instance;
            inst.state = Parameter.State.Prepare;
            HandleGenerateViewBinding();
        }

        /// <summary>
        /// バインディングクラスの生成をハンドリング
        /// </summary>
        [DidReloadScripts]
        public static void HandleGenerateViewBinding()
        {
            var inst = Parameter.instance;
            if(inst.state == Parameter.State.None) { return; }

            switch(inst.state)
            {
                case Parameter.State.Prepare:
                    Debug.Log($"[Generate ViewBinding] Begin");

                    // バインディングクラス生成の前処理
                    PrepareGenerateViewBinding();
                    inst.state = Parameter.State.Generate;
                    break;
                case Parameter.State.Generate:
                    // バインディングクラスの生成
                    GenerateViewBinding();
                    inst.state = Parameter.State.Attach;
                    break;
                case Parameter.State.Attach:
                    // バインディングクラスのアタッチ
                    AttachViewBinding();
                    inst.state = Parameter.State.None;

                    // バインディングクラス以外の生成コードをここで削除
                    {
                        var pathWrite = GetCallerFilePath(FILE_NAME_WRITE);
                        var pathAttach = GetCallerFilePath(FILE_NAME_ATTACH);

                        // パスがフルパスなのでmetaも個別に消す…
                        File.Delete(pathWrite);
                        File.Delete(pathAttach);
                        File.Delete($"{pathWrite}.meta");
                        File.Delete($"{pathAttach}.meta");
                    }

                    Debug.Log($"[Generate ViewBinding] End");
                    break;
            }

            AssetDatabase.Refresh();
            if(inst.state != Parameter.State.None)
            {
                // コンパイルを実行
                CompilationPipeline.RequestScriptCompilation();
            }
        }

        /// <summary>
        /// バインディングクラス生成の前処理
        /// </summary>
        private static void PrepareGenerateViewBinding()
        {
            // バインディングクラス書き出しコード
            const string TAG_METHOD_WRITE  = "#METHOD_WRITE#";
            const string TAG_METHOD_REMOVE = "#METHOD_REMOVE#";
            string CODE_TEMPLATE = $@"
using UnityEngine;
using System.Text;

namespace {nameof(ViewBinding)}
{{
public static partial class {nameof(ViewBindingEditor)}
{{

static partial void {nameof(WriteViewBinding)}(GameObject obj, string format, StringBuilder sb)
{{
{TAG_METHOD_WRITE}
}}

static partial void {nameof(RemoveChildViewBinding)}(GameObject obj)
{{
{TAG_METHOD_REMOVE}
}}

}}
}}
";

            // 書き出すクラス名のリスト
            List<string> viewNameList = GetViewClassNameList();
            List<string> classNameList = BIND_TYPE_LIST
                .Select(x => x.FullName)
                .Distinct()
                .Concat(viewNameList)
                .ToList();

            // 各要素の書き出しコード
            var sb1 = new StringBuilder();
            foreach(var className in classNameList)
            {
                sb1.AppendLine($"sb.AppendLine({nameof(GetWriteText)}<{className}>(obj, format));");
            }

            // 余分な子オブジェクト削除コード
            var sb2 = new StringBuilder();
            foreach(var className in viewNameList)
            {
                sb2.AppendLine($"{nameof(RemoveChildViewBinding)}<{className}>(obj);");
            }

            // コード書き出し
            var code = CODE_TEMPLATE
                .Replace(TAG_METHOD_WRITE, sb1.ToString().Trim())
                .Replace(TAG_METHOD_REMOVE, sb2.ToString().Trim());

            var path = GetCallerFilePath(FILE_NAME_WRITE);
            File.WriteAllText(path, code, Encoding.UTF8);
        }

        /// <summary>
        /// バインディングクラスを生成
        /// </summary>
        private static void GenerateViewBinding()
        {
            // バインディングクラスアタッチ用コード
            const string TAG_CLASS_NAME = "#CLASS_NAME#";
            const string TAG_PATH       = "#PATH#";
            const string TAG_METHOD     = "#METHOD#";
            string CODE_TEMPLATE = $@"
using UnityEditor;

namespace {nameof(ViewBinding)}
{{
public static partial class {nameof(ViewBindingEditor)}
{{

static partial void {nameof(AttachViewBinding)}()
{{
{TAG_METHOD}
}}

}}
}}
";
            string FORMAT_ATTACH_CODE = $@"
{{
var obj = PrefabUtility.LoadPrefabContents(""{TAG_PATH}"");
obj.GetOrAddComponent<{TAG_CLASS_NAME}>().Attach();
PrefabUtility.SaveAsPrefabAsset(obj, ""{TAG_PATH}"");
PrefabUtility.UnloadPrefabContents(obj);
}}";

            // UIプレハブを列挙
            var prefabPathList = PathUtility.GetFiles(SRC_PREFAB_PATH, "*.prefab", SearchOption.TopDirectoryOnly)
                .Select(x => x.Substring(x.IndexOf("Assets/")))
                .ToArray();

            // バインディングクラスを生成
            var sb = new StringBuilder();
            foreach(var path in prefabPathList)
            {
                var obj = PrefabUtility.LoadPrefabContents(path);
                RemoveChildViewBinding(obj);    // 余分な子オブジェクトを削除
                WriteViewBinding(obj);          // バインディングクラスを書き出し

                var str = FORMAT_ATTACH_CODE
                    .Replace(TAG_CLASS_NAME, GetViewClassName(obj))
                    .Replace(TAG_PATH, path);
                sb.AppendLine(str);

                PrefabUtility.UnloadPrefabContents(obj);
            }

            // アタッチ用コード書き出し
            {
                var code = CODE_TEMPLATE.Replace(TAG_METHOD, sb.ToString().Trim());
                var path = GetCallerFilePath(FILE_NAME_ATTACH);
                File.WriteAllText(path, code, Encoding.UTF8);
            }
        }

        /// <summary>
        /// バインディングクラスを生成
        /// </summary>
        private static void WriteViewBinding(GameObject obj)
        {
            // バインディングクラスのテンプレート
            const string TAG_CLASS_NAME      = "#CLASS_NAME#";
            const string TAG_SERIALIZE_FIELD = "#SERIALIZE_FIELD#";
            const string TAG_PROPERTY        = "#PROPERTY#";
            const string TAG_METHOD          = "#METHOD#";
            string CODE_TEMPLATE = $@"
using UnityEngine;

public class {TAG_CLASS_NAME} : MonoBehaviour
{{
{TAG_SERIALIZE_FIELD}

{TAG_PROPERTY}

#if UNITY_EDITOR
public void Attach()
{{
{TAG_METHOD}
}}
#endif
}}
";
            // 書き出す各要素。0: クラス名, 1: オブジェクト名, 2: パス
            const string FORMAT_SERIALIZE_FIELD = "[SerializeField] private {0} _{1};";
            const string FORMAT_PROPERTY        = "public {0} {1} => _{1};";
            const string FORMAT_ATTACH          = "_{1} = transform.Find(\"{2}\").GetComponent<{0}>();";

            // コード書き出し
            var className = GetViewClassName(obj);
            var code = CODE_TEMPLATE
                .Replace(TAG_CLASS_NAME, className)
                .Replace(TAG_SERIALIZE_FIELD, Write(obj, FORMAT_SERIALIZE_FIELD))
                .Replace(TAG_PROPERTY, Write(obj, FORMAT_PROPERTY))
                .Replace(TAG_METHOD, Write(obj, FORMAT_ATTACH));

            var path = PathUtility.Combine(DST_SCRIPT_PATH, $"{className}.cs");
            File.WriteAllText(path, code, Encoding.UTF8);

            static string Write(GameObject obj, string format)
            {
                var sb = new StringBuilder();
                WriteViewBinding(obj, format, sb);
                return sb.ToString().Trim();
            }
        }

        /// <summary>
        /// バインディングクラスに書き出す各要素のコードを返す
        /// </summary>
        /// <param name="obj">UIプレハブのインスタンス</param>
        /// <param name="format">書き出す文字列のフォーマット。0: クラス名, 1: オブジェクト名, 2: パス</param>
        private static string GetWriteText<T>(GameObject obj, string format) where T : Component
        {
            var className = typeof(T).FullName;
            var children = obj.GetComponentsInChildrenWithoutSelf<T>();
            var sb = new StringBuilder();

            foreach(var child in children)
            {
                var childName = child.name.ToCamel();
                var childPath = GetChildPath(child.transform, obj.transform);
                var str = string.Format(format, className, childName, childPath);
                sb.AppendLine(str);
            }

            return sb.ToString().Trim();

            // 子オブジェクトの相対パスを取得
            static string GetChildPath(Transform curr, Transform root)
            {
                var path = curr.name;
                while(curr.parent != root)
                {
                    curr = curr.parent;
                    path = $"{curr.name}/{path}";
                }

                return path;
            }
        }

        /// <summary>
        /// バインディングクラスのファイル名
        /// </summary>
        private static string GetViewClassName(GameObject asset)
        {
            return $"{asset.name}{DST_SCRIPT_SUFFIX}";
        }

        /// <summary>
        /// 生成したバインディングクラス名のリストを取得
        /// </summary>
        private static List<string> GetViewClassNameList()
        {
            return PathUtility.GetFiles(DST_SCRIPT_PATH, "*.cs", SearchOption.TopDirectoryOnly)
                .Select(x => PathUtility.GetFileNameWithoutExtension(x))
                .ToList();
        }

        /// <summary>
        /// 子のバインディングクラスを削除する
        /// </summary>
        private static void RemoveChildViewBinding<T>(GameObject obj) where T : Component
        {
            var children = obj.GetComponentsInChildrenWithoutSelf<T>();
            foreach(var child in children)
            {
                for(int i = child.transform.childCount - 1; i >= 0; i--)
                {
                    GameObject.DestroyImmediate(child.transform.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        /// 自身を除いてGetComponentsInChildrenを実行
        /// </summary>
        private static T[] GetComponentsInChildrenWithoutSelf<T>(this GameObject self) where T : Component
        {
            return self.GetComponentsInChildren<T>(true).Where(c => self != c.gameObject).ToArray();
        }

        /// <summary>
        /// このソースコードと同じパスのファイル名
        /// </summary>
        private static string GetCallerFilePath(string fileName, [CallerFilePath] string filePath = "")
        {
            return PathUtility.Combine(PathUtility.GetDirectoryName(filePath), fileName);
        }

        /// <summary>
        /// コンポーネントをアタッチして取得
        /// </summary>
        private static T GetOrAddComponent<T>(this GameObject self) where T : Component
        {
            return self.GetComponent<T>() ?? self.AddComponent<T>();
        }

        /// <summary>
        /// スネークケースに変換
        /// </summary>
        private static string ToSnake(this string str)
        {
            // 出力するのがC#スクリプトなので、ビルドが通るような文字列に加工する
            str = str.Replace(" ", "_");                    // 半角スペースはアンダーバーに変更
            str = Regex.Replace(str, "[^_a-zA-Z0-9]", "");    // 英数字アンダーバー以外は削除
            str = Regex.Replace(str, "[a-z][A-Z]", m => m.Groups[0].Value[0] + "_" + m.Groups[0].Value[1]).ToLower();
            return str;
        }

        /// <summary>
        /// キャメルケースに変換
        /// </summary>
        private static string ToCamel(this string str)
        {
            str = str.ToSnake();
            str = Regex.Replace(str, "_[a-z]", m => "" + char.ToUpper(m.Groups[0].Value[1]));
            return str;
        }

        /// <summary>
        /// パスカルケースに変換
        /// </summary>
        private static string ToPascal(this string str)
        {
            str = str.ToCamel();
            var charArray = str.ToCharArray();
            charArray[0] = char.ToUpper(charArray[0]);
            return new string(charArray);
        }
    }
}
