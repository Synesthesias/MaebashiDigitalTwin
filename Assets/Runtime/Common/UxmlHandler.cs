using Landscape2.Runtime.UiCommon;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Landscape2.Maebashi.Runtime
{
    public struct UxmlStyleMargin
    {
        public float top;
        public float left;
        public float right;
        public float bottom;
    }
    
    public class UxmlHandler
    {
        private List<VisualElement> uxmls = new();
        public List<VisualElement> Uxmls => uxmls;
        
        public UxmlHandler()
        {
            // 共通のスタイルシートを読み込む
            var globalStyleSheet = Resources.Load<StyleSheet>("UICommon");
            
            // メニューのuxmlを生成
            foreach (var type in Enum.GetValues(typeof(SubComponents.SubMenuUxmlType)))
            {
                if (type is SubComponents.SubMenuUxmlType.Menu)
                {
                    continue;
                }
                var uxml = new UIDocumentFactory().CreateWithUxmlName(type.ToString());
                
                // 共通のスタイルシート適用
                if (!uxml.styleSheets.Contains(globalStyleSheet))
                {
                    uxml.styleSheets.Add(globalStyleSheet);
                }
                uxmls.Add(uxml);
            };
            
            HideAll();
        }
        
        public void Show(SubComponents.SubMenuUxmlType type)
        {
            uxmls[(int)type].Show();
            
            // NOTE:一部のtypeではdisplayで判定しているので、追加
            uxmls[(int)type].style.display = DisplayStyle.Flex;
            
        }

        public void HideAll()
        {
            foreach (var uxml in uxmls)
            {
                uxml.Hide();
                
                // NOTE:一部のtypeではdisplayで判定しているので、追加
                uxml.style.display = DisplayStyle.None;
            }
        }
        
        public VisualElement GetUxml(SubComponents.SubMenuUxmlType type)
        {
            if (type == SubComponents.SubMenuUxmlType.Menu)
            {
                throw new ArgumentException("Invalid type: SubMenuUxmlType.Menu is not allowed.");
            }
            return uxmls[(int)type];
        }

        public bool IsVisible(SubComponents.SubMenuUxmlType type)
        {
            if (type == SubComponents.SubMenuUxmlType.Menu)
            {
                throw new ArgumentException("Invalid type: SubMenuUxmlType.Menu is not allowed.");
            }
            return GetUxml(type).IsVisible();
        }
        
        /// <summary>
        /// uxmlのマージンを調整
        /// </summary>
        /// <param name="type"></param>
        /// <param name="targetElementName"></param>
        /// <param name="margin">上下左右のマージンをpixel単位で指定</param>
        public void AdjustMargin(SubComponents.SubMenuUxmlType type, string targetElementName, UxmlStyleMargin margin)
        {
            var uxml = GetUxml(type);
            var targetElement = uxml.Q(targetElementName);
            if (targetElement == null)
            {
                Debug.LogError($"Target element '{targetElementName}' not found in UXML of type '{type}'.");
                return;
            }
            targetElement.style.marginTop = margin.top;
            targetElement.style.marginLeft = margin.left;
            targetElement.style.marginRight = margin.right;
            targetElement.style.marginBottom = margin.bottom;
        }
    }
}