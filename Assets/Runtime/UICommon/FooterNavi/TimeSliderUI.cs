using System;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Landscape2.Maebashi.Runtime
{
    public class TimeSliderUI
    {
        public UnityEvent<float> OnTimeChanged = new();

        public TimeSliderUI(VisualElement parent)
        {
            var timeSlider = parent.Q<Slider>("TimeSlider");
            if (timeSlider == null)
            {
                throw new InvalidOperationException("Slider with name 'UITimeSlider' not found in the UI hierarchy.");
            }

            timeSlider.RegisterValueChangedCallback(evt =>
            {
                OnTimeChanged?.Invoke(evt.newValue);
            });
        }
    }
} 