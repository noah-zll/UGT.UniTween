using System.Collections.Generic;
using UnityEngine;

namespace UGT.UniTween.Components
{
    /// <summary>
    /// TweenPlayer 对象池，用于批量复用动画实例。
    /// </summary>
    public sealed class TweenPlayerPool
    {
        private readonly TweenPlayer _prefab;
        private readonly Transform _parent;
        private readonly Stack<TweenPlayer> _available = new Stack<TweenPlayer>();
        private readonly List<TweenPlayer> _active = new List<TweenPlayer>();

        public int ActiveCount => _active.Count;
        public int AvailableCount => _available.Count;

        public TweenPlayerPool(TweenPlayer prefab, Transform parent, int initialCapacity)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < initialCapacity; i++)
            {
                CreateAndPush();
            }
        }

        /// <summary>
        /// 从池中取出一个 TweenPlayer 并播放。
        /// </summary>
        public TweenPlayer Spawn()
        {
            TweenPlayer player;
            if (_available.Count > 0)
            {
                player = _available.Pop();
            }
            else
            {
                player = CreateNew();
            }

            player.transform.SetParent(_parent, false);
            player.gameObject.SetActive(true);
            player.OnComplete.AddListener(() => OnComplete(player));
            _active.Add(player);
            player.Play();
            return player;
        }

        /// <summary>
        /// 回收一个 TweenPlayer 到池中。
        /// </summary>
        public void Recycle(TweenPlayer player)
        {
            if (player == null) return;

            player.Stop();
            player.OnComplete.RemoveAllListeners();
            player.gameObject.SetActive(false);
            _active.Remove(player);
            _available.Push(player);
        }

        /// <summary>
        /// 回收所有活跃实例。
        /// </summary>
        public void RecycleAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Recycle(_active[i]);
            }
        }

        /// <summary>
        /// 销毁池中所有实例。
        /// </summary>
        public void DestroyAll()
        {
            RecycleAll();
            while (_available.Count > 0)
            {
                var player = _available.Pop();
                if (player != null)
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(player.gameObject);
                    }
                    else
                    {
                        Object.DestroyImmediate(player.gameObject);
                    }
                }
            }
        }

        private void CreateAndPush()
        {
            var player = CreateNew();
            player.gameObject.SetActive(false);
            _available.Push(player);
        }

        private TweenPlayer CreateNew()
        {
            return Object.Instantiate(_prefab);
        }

        private void OnComplete(TweenPlayer player)
        {
            Recycle(player);
        }
    }
}
