//  PathUtility.cs
//  http://kan-kikuchi.hatenablog.com/entry/PathUtility
//
//  Created by kan.kikuchi on 2019.12.23

using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

/// <summary>
/// Windowsの区切り文字(\)が紛れ込まないように、色々なパスを扱うためのクラス
/// </summary>
public static class PathUtility
{
    //=================================================================================
    //区切り文字
    //=================================================================================

    /// <summary>
    /// Windowsで使われる事がある区切り文字をMacやUnity用の区切り文字に置き換える
    /// </summary>
    public static string ReplaceDelimiter(string targetPath)
    {
        return targetPath.Replace("\\", "/");
    }

    //=================================================================================
    //呼び出し元のパスを取得
    //=================================================================================

    /// <summary>
    /// 呼び出し元のパスを取得
    /// </summary>
    public static string GetSelfPath([CallerFilePath] string sourceFilePath = "")
    {
        return ReplaceDelimiter(sourceFilePath);
    }

    /// <summary>
    /// 呼び出し元のパス(Assetsから)を取得
    /// </summary>
    public static string GetSelfPathFromAssets([CallerFilePath] string sourceFilePath = "")
    {
        return (new Uri(Application.dataPath)).MakeRelativeUri(new Uri(sourceFilePath)).ToString();
    }

    //=================================================================================
    //System.IO.Pathのメソッド
    //=================================================================================

    /// <summary>
    /// 拡張子を変更する
    /// </summary>
    public static string ChangeExtension(string path, string extension)
    {
        return ReplaceDelimiter(Path.ChangeExtension(path, extension));
    }

    /// <summary>
    /// path1とpath2を結合したパスを生成
    /// </summary>
    public static string Combine(string path1, string path2)
    {
        return ReplaceDelimiter(Path.Combine(path1, path2));
    }

    /// <summary>
    /// path1~path3を結合したパスを生成
    /// </summary>
    public static string Combine(string path1, string path2, string path3)
    {
        return ReplaceDelimiter(Path.Combine(path1, path2, path3));
    }

    /// <summary>
    /// path1~path4を結合したパスを生成
    /// </summary>
    public static string Combine(string path1, string path2, string path3, string path4)
    {
        return ReplaceDelimiter(Path.Combine(path1, path2, path3, path4));
    }

    /// <summary>
    ///paths全てを結合したパスを生成
    /// </summary>
    public static string Combine(params string[] paths)
    {
        return ReplaceDelimiter(Path.Combine(paths));
    }

    /// <summary>
    /// ディレクトリの名前を取得
    /// </summary>
    public static string GetDirectoryName(string path)
    {
        return ReplaceDelimiter(Path.GetDirectoryName(path));
    }

    /// <summary>
    /// 拡張子を取得
    /// </summary>
    public static string GetExtension(string path)
    {
        return ReplaceDelimiter(Path.GetExtension(path));
    }

    /// <summary>
    /// 拡張子ありのファイル名を取得
    /// </summary>
    public static string GetFileName(string path)
    {
        return ReplaceDelimiter(Path.GetFileName(path));
    }

    /// <summary>
    /// 拡張子なしのファイル名を取得
    /// </summary>
    public static string GetFileNameWithoutExtension(string path)
    {
        return ReplaceDelimiter(Path.GetFileNameWithoutExtension(path));
    }

    /// <summary>
    /// 相対パスから絶対パスを取得
    /// </summary>
    public static string GetFullPath(string path)
    {
        return ReplaceDelimiter(Path.GetFullPath(path));
    }

    /// <summary>
    /// ファイル名に使用できない文字を全て取得
    /// </summary>
    public static char[] GetInvalidFileNameChars()
    {
        return Path.GetInvalidFileNameChars();
    }

    /// <summary>
    /// パスに使用できない文字を全て取得
    /// </summary>
    public static char[] GetInvalidPathChars()
    {
        return Path.GetInvalidPathChars();
    }

    /// <summary>
    /// 指定のパスからルートへのパスだけを取得
    /// </summary>
    public static string GetPathRoot(string path)
    {
        return ReplaceDelimiter(Path.GetPathRoot(path));
    }

    /// <summary>
    /// ランダムなファイル名を取得
    /// </summary>
    /// <returns></returns>
    public static string GetRandomFileName()
    {
        return ReplaceDelimiter(Path.GetRandomFileName());
    }

    /// <summary>
    /// 一時ファイルのパスをランダムに作成
    /// </summary>
    public static string GetTempFileName()
    {
        return ReplaceDelimiter(Path.GetTempFileName());
    }

    /// <summary>
    /// 一時ファイルがあるディレクトリへのパスを取得
    /// </summary>
    public static string GetTempPath()
    {
        return ReplaceDelimiter(Path.GetTempPath());
    }

    /// <summary>
    /// 拡張子が付いているか
    /// </summary>
    public static bool HasExtension(string path)
    {
        return Path.HasExtension(path);
    }

    /// <summary>
    /// パスのルートか
    /// </summary>
    public static bool IsPathRooted(string path)
    {
        return Path.IsPathRooted(path);
    }

    //=================================================================================
    //System.IO.Directoryのメソッド
    //=================================================================================

    /// <summary>
    /// パスで指定したディレクトリに含まれるディレクトリのパスを全て取得
    /// </summary>
    public static string[] GetDirectories(string path)
    {
        return Directory.GetDirectories(path).Select(ReplaceDelimiter).ToArray();
    }

    /// <summary>
    /// パスで指定したディレクトリに含まれるディレクトリのパスを全て取得
    /// </summary>
    public static string[] GetDirectories(string path, string searchPattern)
    {
        return Directory.GetDirectories(path, searchPattern).Select(ReplaceDelimiter).ToArray();
    }

    /// <summary>
    /// パスで指定したディレクトリに含まれるディレクトリのパスを全て取得
    /// </summary>
    public static string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.GetDirectories(path, searchPattern, searchOption).Select(ReplaceDelimiter).ToArray();
    }

    /// <summary>
    /// パスで指定したディレクトリに含まれるファイルのパスを全て取得
    /// </summary>
    public static string[] GetFiles(string path)
    {
        return Directory.GetFiles(path).Select(ReplaceDelimiter).ToArray();
    }

    /// <summary>
    /// パスで指定したディレクトリに含まれるファイルのパスを全て取得
    /// </summary>
    public static string[] GetFiles(string path, string searchPattern)
    {
        return Directory.GetFiles(path, searchPattern).Select(ReplaceDelimiter).ToArray();
    }

    /// <summary>
    /// パスで指定したディレクトリに含まれるファイルのパスを全て取得
    /// </summary>
    public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.GetFiles(path, searchPattern, searchOption).Select(ReplaceDelimiter).ToArray();
    }

    /// <summary>
    /// パスで指定したディレクトリに含まれるディレクトリとファイルのパスを全て取得
    /// </summary>
    public static string[] GetFileSystemEntries(string path)
    {
        return Directory.GetFileSystemEntries(path).Select(ReplaceDelimiter).ToArray();
    }

    /// <summary>
    /// パスで指定したディレクトリに含まれるディレクトリとファイルのパスを全て取得
    /// </summary>
    public static string[] GetFileSystemEntries(string path, string searchPattern)
    {
        return Directory.GetFileSystemEntries(path, searchPattern).Select(ReplaceDelimiter).ToArray();
    }

    /// <summary>
    /// パスで指定したディレクトリに含まれるディレクトリとファイルのパスを全て取得
    /// </summary>
    public static string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.GetFileSystemEntries(path, searchPattern, searchOption).Select(ReplaceDelimiter).ToArray();
    }
}
