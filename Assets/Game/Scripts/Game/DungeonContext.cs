using UnityEngine;
using CryptEscape.Enemies;
using CryptEscape.Player;
using CryptEscape.World;

namespace CryptEscape.Game
{
    /// <summary>
    /// Dungeon シーンのルートに置き、迷宮内の参照をまとめる。DungeonBuilder が配線する。
    /// 読み込まれたら GameDirector に登録する（シーンをまたぐ参照は Inspector で持てないため）。
    /// </summary>
    public class DungeonContext : MonoBehaviour
    {
        public PlayerMotor player;
        public CameraFollow cameraFollow;
        public Transform startPoint;
        public SavePoint[] savePoints;
        public Coin[] coins;
        public Potion[] potions;
        public EnemyBase[] enemies;
        public BossController boss;
        public GameObject bossDoor;
        public GameObject exitGate;

        void Awake() => GameDirector.Register(this);
        void OnDestroy() => GameDirector.Unregister(this);

        public Vector3 RespawnPosition(int savePointId)
        {
            if (savePointId >= 0 && savePointId < savePoints.Length)
                return savePoints[savePointId].RespawnPosition;
            return startPoint.position;
        }
    }
}
