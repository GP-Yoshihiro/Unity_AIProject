namespace CryptEscape.Game
{
    /// <summary>
    /// 押下状態の立ち上がり（押した瞬間）を検出する。
    /// unity-cli の input_keyboard で押したキーは wasPressedThisFrame が立たないことがあるため、
    /// 前フレームの押下状態と比べて判定する（research R4）。
    /// </summary>
    public struct ButtonEdge
    {
        bool wasDown;

        public bool Update(bool isDown)
        {
            bool pressed = isDown && !wasDown;
            wasDown = isDown;
            return pressed;
        }
    }
}
