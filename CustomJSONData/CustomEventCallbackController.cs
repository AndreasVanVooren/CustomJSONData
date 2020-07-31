﻿using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomJSONData
{
    public class CustomEventCallbackController : MonoBehaviour
    {
        public static event Action<CustomEventCallbackController> customEventCallbackControllerInit;

        private void Start()
        {
            customEventCallbackControllerInit?.Invoke(this);
        }

        private void LateUpdate()
        {
            if (_beatmapObjectCallbackController.enabled && _beatmapData is CustomBeatmapData customBeatmapData)
            {
                for (int l = 0; l < _customEventCallbackData.Count; l++)
                {
                    CustomEventCallbackData customEventCallbackData = _customEventCallbackData[l];
                    while (customEventCallbackData.nextEventIndex < customBeatmapData.customEventData.Length)
                    {
                        CustomEventData customEventData = customBeatmapData.customEventData[customEventCallbackData.nextEventIndex];
                        if (customEventData.time - customEventCallbackData.aheadTime >= _audioTimeSource.songTime) break;
                        if (customEventData.time >= _spawningStartTime || customEventCallbackData.callIfBeforeStartTime) // skip events before song start
                        {
                            customEventCallbackData.callback(customEventData);
                        }
                        customEventCallbackData.nextEventIndex++;
                    }
                }
            }
        }

        public CustomEventCallbackData AddCustomEventCallback(CustomEventCallback callback, float aheadTime = 0, bool callIfBeforeStartTime = true)
        {
            CustomEventCallbackData customEventCallbackData = new CustomEventCallbackData(callback, aheadTime, callIfBeforeStartTime);
            _customEventCallbackData.Add(customEventCallbackData);
            return customEventCallbackData;
        }

        public void RemoveBeatmapEventCallback(CustomEventCallbackData callbackData)
        {
            _customEventCallbackData?.Remove(callbackData);
        }

        internal void SetNewBeatmapData(BeatmapData beatmapData)
        {
            foreach (CustomEventCallbackData customEventCallbackData in _customEventCallbackData)
            {
                customEventCallbackData.nextEventIndex = 0;
            }
        }

        private List<CustomEventCallbackData> _customEventCallbackData = new List<CustomEventCallbackData>();

        public class CustomEventCallbackData
        {
            public CustomEventCallback callback;
            public float aheadTime;
            public int nextEventIndex;
            public bool callIfBeforeStartTime;

            public CustomEventCallbackData(CustomEventCallback callback, float aheadTime, bool callIfBeforeStartTime)
            {
                this.callback = callback;
                this.aheadTime = aheadTime;
                this.callIfBeforeStartTime = callIfBeforeStartTime;
                nextEventIndex = 0;
            }
        }

        public delegate void CustomEventCallback(CustomEventData eventData);

        internal BeatmapObjectCallbackController _beatmapObjectCallbackController;
        private static readonly FieldAccessor<BeatmapObjectCallbackController, BeatmapData>.Accessor _beatmapDataAccessor = FieldAccessor<BeatmapObjectCallbackController, BeatmapData>.GetAccessor("_beatmapData");
        private static readonly FieldAccessor<BeatmapObjectCallbackController, IAudioTimeSource>.Accessor _audioTimeSourceAccessor = FieldAccessor<BeatmapObjectCallbackController, IAudioTimeSource>.GetAccessor("_audioTimeSource");
        private static readonly FieldAccessor<BeatmapObjectCallbackController, float>.Accessor _spawningStartTimeAccessor = FieldAccessor<BeatmapObjectCallbackController, float>.GetAccessor("_spawningStartTime");
        public BeatmapData _beatmapData { get => _beatmapDataAccessor(ref _beatmapObjectCallbackController); }
        public IAudioTimeSource _audioTimeSource { get => _audioTimeSourceAccessor(ref _beatmapObjectCallbackController); }
        public float _spawningStartTime { get => _spawningStartTimeAccessor(ref _beatmapObjectCallbackController); }
    }
}
