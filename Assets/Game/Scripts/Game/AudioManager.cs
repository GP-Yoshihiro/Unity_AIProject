using System;
using System.Collections.Generic;
using UnityEngine;

namespace CryptEscape.Game
{
    /// <summary>
    /// 効果音と BGM を名前で鳴らす。素材がない名前は鳴らさないが、履歴には残す。
    /// 音は画像で確認できないため、直近の履歴を unity-cli から読めるようにする（research R9）。
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public const string Attack = "Attack", Hit = "Hit", Hurt = "Hurt", Coin = "Coin", Heal = "Heal",
            Save = "Save", DoorOpen = "DoorOpen", BossTell = "BossTell", BossDefeat = "BossDefeat",
            Clear = "Clear", GameOver = "GameOver";
        public const string BgmExplore = "Explore", BgmBoss = "Boss", BgmTitle = "Title";

        [Serializable]
        public struct NamedClip
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
        }

        [SerializeField] AudioSource sfxSource;
        [SerializeField] AudioSource bgmSource;
        [SerializeField] List<NamedClip> sounds = new List<NamedClip>();
        [SerializeField] List<NamedClip> music = new List<NamedClip>();

        const int HistorySize = 8;
        readonly Queue<string> history = new Queue<string>();

        public string CurrentBgm { get; private set; } = "";
        public string HistoryText => string.Join("|", history);

        public void Play(string soundName)
        {
            history.Enqueue(soundName);
            while (history.Count > HistorySize) history.Dequeue();

            if (TryFind(sounds, soundName, out var entry) && entry.clip != null && sfxSource != null)
                sfxSource.PlayOneShot(entry.clip, entry.volume > 0f ? entry.volume : 1f);
        }

        public void PlayBgm(string musicName)
        {
            if (CurrentBgm == musicName) return;
            CurrentBgm = musicName;
            if (bgmSource == null) return;

            if (TryFind(music, musicName, out var entry) && entry.clip != null)
            {
                bgmSource.clip = entry.clip;
                bgmSource.volume = entry.volume > 0f ? entry.volume : 0.6f;
                bgmSource.loop = true;
                bgmSource.Play();
            }
            else
            {
                bgmSource.Stop();
            }
        }

        static bool TryFind(List<NamedClip> list, string name, out NamedClip found)
        {
            foreach (var c in list)
            {
                if (c.name == name)
                {
                    found = c;
                    return true;
                }
            }
            found = default;
            return false;
        }
    }
}
