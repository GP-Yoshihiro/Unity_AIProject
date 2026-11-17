using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BallCollect
{
    /// <summary>
    /// ゲーム全体の進行。回収数とCLEARの表示、落下判定、Rキーでの再開を担当する。
    /// 公開フィールドは unity-cli の get_component_values で状態を取得するためのもの。
    /// </summary>
    public class BallCollectGame : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] PlayerController player;
        [SerializeField] CollectibleItem[] items;
        [SerializeField] Text countText;
        [SerializeField] Text clearText;

        [Header("Settings")]
        [SerializeField] Vector3 playerStartPosition = new Vector3(0f, 0.5f, -7f);
        [SerializeField] float fallThresholdY = -5f;

        [Header("State (read-only)")]
        public int collectedCount;
        public int totalItems;
        public bool isCleared;

        BallCollectRules rules;
        bool restartKeyWasDown;

        void Awake()
        {
            rules = new BallCollectRules(items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                items[i].ItemId = i;
                items[i].Bind(this);
            }
        }

        void Start()
        {
            ResetGame();
        }

        void Update()
        {
            if (RestartKeyPressed())
            {
                ResetGame();
                return;
            }

            if (player.transform.position.y < fallThresholdY)
            {
                ResetGame();
            }
        }

        // wasPressedThisFrame は外部から状態を書き換えた入力（unity-cli の input_keyboard）で
        // 立たないことがあるため、押下状態の立ち上がりを自前で検出する。
        bool RestartKeyPressed()
        {
            var keyboard = Keyboard.current;
            bool isDown = keyboard != null && keyboard.rKey.isPressed;
            bool pressed = isDown && !restartKeyWasDown;
            restartKeyWasDown = isDown;
            return pressed;
        }

        public void Collect(CollectibleItem item)
        {
            if (!rules.TryCollect(item.ItemId)) return;

            item.gameObject.SetActive(false);
            SyncState();
        }

        /// <summary>球の位置と速度、回収数、アイテム、UIをすべて初期状態に戻す。</summary>
        public void ResetGame()
        {
            rules.Reset();
            foreach (var item in items) item.gameObject.SetActive(true);
            player.ResetTo(playerStartPosition);
            SyncState();
        }

        void SyncState()
        {
            collectedCount = rules.CollectedCount;
            totalItems = rules.TotalItems;
            isCleared = rules.IsCleared;

            countText.text = $"{collectedCount}/{totalItems}";
            clearText.gameObject.SetActive(isCleared);
        }
    }
}
