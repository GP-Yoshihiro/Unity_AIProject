using UnityEngine;
using UnityEngine.UI;
using CryptEscape.Game;

namespace CryptEscape.UI
{
    /// <summary>
    /// ダイアログシーン（PauseDialog / GameOverDialog / ClearDialog）のルート。
    /// 内容と選択は GameDirector が決め、ここは表示だけを担当する。
    /// </summary>
    public class DialogView : MonoBehaviour
    {
        [SerializeField] string dialogName;
        [SerializeField] Text titleText;
        [SerializeField] Text bodyText;
        [SerializeField] Text[] optionTexts;

        string[] options = new string[0];

        public string DialogName => dialogName;

        void Awake() => GameDirector.Register(this);
        void OnDestroy() => GameDirector.Unregister(this);

        public void SetContent(string title, string body, string[] newOptions)
        {
            titleText.text = title;
            bodyText.text = body;
            options = newOptions;
            for (int i = 0; i < optionTexts.Length; i++)
                optionTexts[i].gameObject.SetActive(i < options.Length);
        }

        public void SetSelection(int index)
        {
            for (int i = 0; i < options.Length && i < optionTexts.Length; i++)
            {
                bool selected = i == index;
                optionTexts[i].text = selected ? $"> {options[i]} <" : options[i];
                optionTexts[i].color = selected ? new Color(1f, 0.85f, 0.3f) : new Color(0.85f, 0.85f, 0.85f);
            }
        }
    }
}
