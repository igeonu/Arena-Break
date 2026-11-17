using System;

namespace ArenaBreak.Core
{
    // 처치 수 집계만 맡는다. 무엇을 처치로 칠지는 사망을 이미 구독하고 있는 호출 쪽(WaveSpawner)이 정한다
    public class KillCounter
    {
        public event Action<int> Changed;

        public int Count { get; private set; }

        public void Add()
        {
            Count++;
            Changed?.Invoke(Count);
        }
    }
}
