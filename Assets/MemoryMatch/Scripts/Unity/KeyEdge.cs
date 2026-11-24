using UnityEngine.InputSystem;

namespace MemoryMatch
{
    /// <summary>
    /// キーの押し始めを、Update ごとの押下状態の差で検出する。
    /// wasPressedThisFrame は、Editor 側の更新で届く入力（unity-cli）を取りこぼすため使わない。
    /// </summary>
    public sealed class KeyEdge
    {
        private readonly Key _key;
        private bool _wasDown;

        public KeyEdge(Key key) => _key = key;

        /// <summary>毎フレーム1回だけ呼ぶ。</summary>
        public bool Poll(Keyboard keyboard)
        {
            bool down = keyboard[_key].isPressed;
            bool edge = down && !_wasDown;
            _wasDown = down;
            return edge;
        }
    }
}
