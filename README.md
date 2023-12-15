PathUtility.csは以下を使用させていただきました。  
https://gist.github.com/kankikuchi/601802c92757a2273aec26eb88ba979e

# ViewBinding
UIプレハブの各要素をバインドするクラスを生成するテスト。  
指定フォルダ以下のUIプレハブの各要素をバインドするクラスを生成して自動的にアタッチします。

![画面収録-2023-12-15-17 26 38_out](https://github.com/cy-tatsuya-sakai/ViewBinding/assets/33710684/1f55ed04-23b8-43d6-ad2b-b87ae80fc53a)

## 使い方メモ
* ViewBindingEditor.csとPathUtility.csの2ファイルを適当なEditorフォルダに配置する
  * https://github.com/cy-tatsuya-sakai/ViewBinding/tree/main/Assets/UI/Editor
* UIプレハブフォルダ、生成したスクリプトの出力先フォルダ、バインドしたい型をコードを書き換えて指定する
  * https://github.com/cy-tatsuya-sakai/ViewBinding/blob/main/Assets/UI/Editor/ViewBindingEditor.cs#L22-L33
* メニューから[Tools] > [ViewBinding] > [Generate ViewBinding All]を実行する
