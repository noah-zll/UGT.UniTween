using System;
using UnityEngine;
using UnityEngine.Events;

namespace UGT.UniTween.Components
{
    /// <summary>
    /// TweenPlayer 事件。
    /// </summary>
    [Serializable]
    public sealed class TweenEvent
    {
        [Tooltip("在序列进度的何时触发（0..1）")]
        [Range(0f, 1f)]
        public float TriggerTime;

        [Tooltip("触发的事件")]
        public UnityEvent OnTrigger;

        [Tooltip("是否已触发（避免重复）")]
        [NonSerialized]
        internal bool HasTriggered;
    }
}
