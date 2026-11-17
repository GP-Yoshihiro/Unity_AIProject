using UnityEngine;
using UnityEngine.UI;
using CryptEscape.Game;

namespace CryptEscape.UI
{
    /// <summary>Title シーンのルート。</summary>
    public class TitleView : MonoBehaviour
    {
        [SerializeField] Text bestText;

        void Awake() => GameDirector.Register(this);
        void OnDestroy() => GameDirector.Unregister(this);

        public void SetBest(string text) => bestText.text = text;
    }
}
