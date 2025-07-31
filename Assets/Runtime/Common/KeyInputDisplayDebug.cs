using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Landscape2.Runtime;

namespace Landscape2.Maebashi.Runtime.Common
{
    /// <summary>
    /// デバッグ用のキー入力表示コンポーネント
    /// </summary>
    public class KeyInputDisplayDebug : ISubComponent
    {
        private VisualElement rootElement;
        private VisualElement keyDisplayContainer;
        private List<KeyDisplayItem> activeKeyDisplays = new List<KeyDisplayItem>();
        private List<KeyDisplayItem> expiredKeyDisplays = new List<KeyDisplayItem>(); // 表示期間が終了したキーアイテム
        private const int MAX_DISPLAY_COUNT = 5;
        private const float DISPLAY_DURATION = 2.0f;

        private readonly System.Array cachedKeyCodes;
        
        public KeyInputDisplayDebug(VisualElement parentRoot)
        {
            rootElement = parentRoot;
            cachedKeyCodes = System.Enum.GetValues(typeof(KeyCode));
            CreateUI();
        }
        
        private class KeyDisplayItem
        {
            public Label label;
            public float startTime;
            public string keyName;
            
            public KeyDisplayItem(Label label, string keyName)
            {
                this.label = label;
                this.keyName = keyName;
                this.startTime = Time.time;
            }
        }

        private void CreateUI()
        {
            // キー表示用のコンテナを作成
            keyDisplayContainer = new VisualElement
            {
                name = "key-display-container"
            };
            
            // スタイル設定 - 左上のロゴ部分に配置
            keyDisplayContainer.style.position = Position.Absolute;
            keyDisplayContainer.style.top = 10;
            keyDisplayContainer.style.left = 10;
            keyDisplayContainer.style.flexDirection = FlexDirection.Column;
            keyDisplayContainer.style.alignItems = Align.FlexStart;
            
            // GlobalNaviUIのrootElementに追加
            rootElement.Add(keyDisplayContainer);
        }

        public void Update(float deltaTime)
        {
            CheckKeyInput();
            UpdateDisplayedKeys();
        }

        private void CheckKeyInput()
        {
            // 主要なキーをチェック
            foreach (KeyCode keyCode in cachedKeyCodes)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    ShowKeyInput(keyCode.ToString());
                }
            }
        }

        private void ShowKeyInput(string keyName)
        {
            // 既に表示されているキーは更新しない
            if (activeKeyDisplays.Any(item => item.keyName == keyName))
                return;

            // 最大表示数を超える場合は古いものを削除
            if (activeKeyDisplays.Count >= MAX_DISPLAY_COUNT)
            {
                var oldestItem = activeKeyDisplays.OrderBy(item => item.startTime).First();
                RemoveKeyDisplay(oldestItem);
            }

            // 新しいキー表示を作成
            var label = new Label(keyName)
            {
                name = $"key-display-{keyName}"
            };
            
            // スタイル設定 - より大きな表示
            label.style.backgroundColor = new Color(0, 0, 0, 0.8f);
            label.style.color = Color.white;
            label.style.paddingBottom = new StyleLength(10);
            label.style.paddingTop = new StyleLength(10);
            label.style.paddingRight = new StyleLength(15);
            label.style.paddingLeft = new StyleLength(15);
            label.style.marginBottom = 5;
            label.style.borderTopLeftRadius = 5;
            label.style.borderBottomLeftRadius = 5;
            label.style.borderTopRightRadius = 5;
            label.style.borderBottomRightRadius = 5;
            label.style.fontSize = 18;
            label.style.minWidth = 50;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;

            keyDisplayContainer.Add(label);
            
            var keyDisplayItem = new KeyDisplayItem(label, keyName);
            activeKeyDisplays.Add(keyDisplayItem);
        }

        private void UpdateDisplayedKeys()
        {
            // 表示期間が終了したキーアイテムのリストをクリア
            expiredKeyDisplays.Clear();
            
            foreach (var item in activeKeyDisplays)
            {
                float elapsed = Time.time - item.startTime;
                
                if (elapsed >= DISPLAY_DURATION)
                {
                    expiredKeyDisplays.Add(item);
                }
                else if (elapsed > DISPLAY_DURATION * 0.7f)
                {
                    // フェードアウト効果
                    float fadeProgress = (elapsed - DISPLAY_DURATION * 0.7f) / (DISPLAY_DURATION * 0.3f);
                    float alpha = 1.0f - fadeProgress;
                    item.label.style.opacity = alpha;
                }
            }
            
            foreach (var item in expiredKeyDisplays)
            {
                RemoveKeyDisplay(item);
            }
        }

        private void RemoveKeyDisplay(KeyDisplayItem item)
        {
            if (item.label.parent != null)
            {
                keyDisplayContainer.Remove(item.label);
            }
            activeKeyDisplays.Remove(item);
        }

        public void OnEnable() { }
        public void LateUpdate(float deltaTime) { }
        public void OnDisable() 
        {
            // クリーンアップ
            activeKeyDisplays.Clear();
            if (keyDisplayContainer?.parent != null)
            {
                keyDisplayContainer.parent.Remove(keyDisplayContainer);
            }
        }
        public void Start() { }
    }
}