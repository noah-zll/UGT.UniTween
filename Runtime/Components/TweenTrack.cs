using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGT.UniTween.Components
{
    /// <summary>
    /// 一条动画轨道。包含一组串行执行的 TweenClip。
    /// 多条轨道在 TweenPlayer 下并行执行。
    /// </summary>
    [Serializable]
    public sealed class TweenTrack
    {
        [Tooltip("轨道名称")]
        public string TrackName;

        [Tooltip("动画目标对象（留空则使用 TweenPlayer 所在对象）")]
        public GameObject Target;

        [Tooltip("串行剪辑列表")]
        public List<TweenClip> Clips = new List<TweenClip>();
    }
}
