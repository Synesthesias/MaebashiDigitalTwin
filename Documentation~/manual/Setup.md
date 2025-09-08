# セットアップの手順

本書では、本リポジトリのセットアップ手順について記載しています。

## Unityのダウンロードとインストール
- Unity Hub を[こちら](https://unity3d.com/jp/get-unity/download)からインストールします。
- Unity Hub とは、Unityのお好きなバージョンをインストールして起動することのできるソフトウェアです。
- Unity Hubを起動し、左のサイドバーからインストール → 右上のボタンからエディターをインストール をクリックします。

![Unityのダウンロードとインストール](../resources/Install/unityHubMenu.png)

- Unity 2022.3.25f1 のバージョンを選択し、インストールを押します。
- 該当のバージョンが見つからない場合は、`アーカイブ` のタブを押して、`ダウンロードアーカイブ`のリンクより選択してください。

![Unityのダウンロードとインストール](../resources/Install/unityHubInstall.png)

## プロジェクトのセットアップ
- 本プロジェクトをgithubよりクローンしてください。
- 本プロジェクトでは `git lfs` と `git submodule` を使用しています。本プロジェクトをクローンできたら、以下のコマンドを実行してください。

```bash
git lfs install
git lfs pull

git submodule update --init --recursive
```

### 前橋データのダウンロード
- 本プロジェクトでは、前橋市の都市データを使用しています。
- 前橋市の都市データは、`下記のリンク先` の `Maebashi.zip` をダウンロードしてください。

[前橋市都市データダウンロードリンク](https://drive.google.com/drive/folders/1PozuAs8KcntlAoV_zBBQGcofI9qGAxei?usp=drive_link)

- ダウンロードしたデータを解凍後、本プロジェクトの `Assets/` 配下に配置してください。

![前橋データ](../resources/Install/maebashiData.png)

- Unityのプロジェクトからは以下のように確認できていれば問題ないです。

![前橋データ](../resources/Install/maebashiData_02.png)

## Unityプロジェクトの起動
- プロジェクトのセットアップが完了したら、`Unity Hub`を起動します。
- 左サイドバーの `プロジェクト` を押し、右上の `追加` ボタンから、`ディスクから加える`をクリックします。

![Unityプロジェクトを作成](../resources/Install/unityHubProjectAdd.png)

- クローンした本プロジェクトのフォルダを選択し、`開く` ボタンを押します。
- 追加されたプロジェクトをクリックします。
- Unityが起動します。

## 必要パッケージの追加
- 本プロジェクトでは、PLATEAUのUnity向けプラグインである `Maps-Toolkit-for-Unity` と `Cesium-for-Unity` を使用しています。
- これらのパッケージは、前橋データと同じ[ダウンロードリンク](https://drive.google.com/drive/folders/1PozuAs8KcntlAoV_zBBQGcofI9qGAxei?usp=drive_link)から `com.cesium.unity-1.7.1.tgz` と `com.unity.plateautoolkit.maps-1.0.2.tgz` をダウンロードしてください。

[プラグインダウンロード](https://drive.google.com/drive/folders/1PozuAs8KcntlAoV_zBBQGcofI9qGAxei?usp=drive_link)

![プラグインダウンロード](../resources/Install/packageInstall.png)

- Unityを起動して、上部メニューの `Window` → `Package Manager` をクリックします。
- Package Managerの左上の `+` ボタンを押し、`Add package from tarball...` をクリックします。

![パッケージの追加](../resources/Install/addPacakge.png)

- 先ほどDLした`com.cesium.unity-1.7.1.tgz`と`com.unity.plateautoolkit.maps-1.0.2.tgz` を指定します。
- エラーが出なければ成功です。

## 前橋シーンを起動

- Unityが起動したら、`Assets/Scenes/MainScene.unity`を開いてください。
- プレイボタンを押して、起動が確認できたらセットアップ完了です。

![Unityプロジェクトを起動](../resources/Install/unityPlay.png)
