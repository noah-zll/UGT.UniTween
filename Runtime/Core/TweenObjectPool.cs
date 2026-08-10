using System.Collections.Generic;

namespace UGT.UniTween.Core
{
    /// <summary>
    /// Tween 对象池，复用 Tween 实例以减少 GC。
    /// </summary>
    internal sealed class TweenObjectPool
    {
        private readonly Stack<Tween> _pool;
        private readonly int _initialCapacity;

        public int PoolSize => _pool.Count;

        public TweenObjectPool(int initialCapacity)
        {
            _initialCapacity = initialCapacity;
            _pool = new Stack<Tween>(initialCapacity);

            for (int i = 0; i < initialCapacity; i++)
            {
                _pool.Push(new Tween());
            }
        }

        public Tween Get()
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }

            return new Tween();
        }

        public void Return(Tween tween)
        {
            if (tween == null) return;

            tween.Reset();
            _pool.Push(tween);
        }
    }
}
