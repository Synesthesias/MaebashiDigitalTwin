# 前橋市 - 景観まちづくり支援ツール 開発用 Unity プロジェクト

## 環境

- Unity 2022.3.24f1

## 開発環境の構築

- 次のコマンドを実行します。 `git lfs install`
- このリポジトリを git clone します。
- 次のコマンドを実行します。
  - `git lfs pull`
  - `git submodule update --init --recursive`

# Linter とコードスタイル

- Linter を利用しているので、それに沿ったコードスタイルで書きましょう。
- Linter の動作確認済み：Visual Studio、および Rider
- お手元で Linter が動くことを確認してください。
  - 確認方法：
  - 試しにフィールドに `string _a = "a";` と書いてみます。
  - private を付けること、およびフィールド名に"\_"を付けないことを提案されたら成功です。
- Visual Studio で動かない場合、Visual Studio Installer から「Visual Studio 拡張機能の開発」がインストールされていることを確認してください。
- コードスタイルは .editorconfig に記載されています。アナライザーは Microsoft.Unity.Analyzers です。
