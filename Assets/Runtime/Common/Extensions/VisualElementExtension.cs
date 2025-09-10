using UnityEngine.UIElements;

namespace Landscape2.Maebashi.Runtime
{
    /// <summary>
    /// Visual Element用のエクステンション
    /// </summary>
    public static class VisualElementExtension
    {
        public static void Show(this VisualElement element)
        {
            RemoveClassSchedule(element, UICommonConstants.UI_Hide_Class);
        }

        public static void Hide(this VisualElement element)
        {
            AddClassSchedule(element, UICommonConstants.UI_Hide_Class);
        }

        public static void ToggleShow(this VisualElement element)
        {
            if (IsVisible(element))
            {
                AddClassSchedule(element, UICommonConstants.UI_Hide_Class);
            }
            else
            {
                RemoveClassSchedule(element, UICommonConstants.UI_Hide_Class);
            }
        }
        
        public static bool IsVisible(this VisualElement element)
        {
            return !element.ClassListContains(UICommonConstants.UI_Hide_Class);
        }
        
        private static void AddClassSchedule(this VisualElement element, string className)
        {
            // 次フレームで更新
            element.schedule.Execute(() =>
            {
                element.AddToClassList(className);
            });
        }
        
        private static void RemoveClassSchedule(this VisualElement element, string className)
        {
            // 次フレームで更新
            element.schedule.Execute(() =>
            {
                element.RemoveFromClassList(className);
            });
        }
    }
}