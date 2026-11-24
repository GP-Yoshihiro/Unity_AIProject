using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemoryMatch.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MemoryMatch
{
    /// <summary>入力をルールへ渡し、結果を表示する。公開プロパティは CLI での状態取得に使う。</summary>
    public sealed class MemoryGame : MonoBehaviour
    {
        private const float CpuStepSeconds = 0.8f;
        private const float CpuShowMissSeconds = 1.5f;

        private const float BoardCenterY = 5f;
        private static readonly Vector2 BoardSize = new Vector2(1240f, 560f);

        [SerializeField] private bool useFixedSeed;
        [SerializeField] private int fixedSeed = 1;
        [SerializeField] private CardView[] cards;
        [SerializeField] private RectTransform cursorFrame;
        [SerializeField] private Text matchedText;
        [SerializeField] private Text missesText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text comboText;
        [SerializeField] private Text playersText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultText;
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Text menuText;

        private readonly KeyEdge _left = new KeyEdge(Key.LeftArrow);
        private readonly KeyEdge _right = new KeyEdge(Key.RightArrow);
        private readonly KeyEdge _up = new KeyEdge(Key.UpArrow);
        private readonly KeyEdge _down = new KeyEdge(Key.DownArrow);
        private readonly KeyEdge _space = new KeyEdge(Key.Space);
        private readonly KeyEdge _restart = new KeyEdge(Key.R);
        private readonly KeyEdge _escape = new KeyEdge(Key.Escape);

        private readonly MenuState _menu = new MenuState();
        private bool[] _wasVisible = new bool[0];
        private DeckShuffler _shuffler;
        private CpuPlayer _cpu;
        private MemoryLogic _logic; // null の間はモード選択
        private float _cpuTimer;

        public string Phase => _logic == null ? "Menu" : _logic.Phase.ToString();
        public string Mode => _menu.Mode.ToString();
        public int PlayerCount => _menu.PlayerCount;
        public int CurrentPlayer => _logic?.CurrentPlayer ?? 0;
        public int Matched => _logic?.Matched ?? 0;
        public int Misses => _logic?.Misses ?? 0;
        public int Score => _logic?.Score ?? 0;
        public int Combo => _logic?.Combo ?? 0;
        public int Cursor => _logic?.Cursor ?? 0;
        public int Columns => _logic?.Spec.Columns ?? 0;
        public int Rows => _logic?.Spec.Rows ?? 0;
        public int MissLimit => _logic?.Spec.MissLimit ?? 0;
        public string Difficulty => _menu.Mode == GameMode.Solo ? _menu.DifficultyName : "";
        public string Scores => _logic == null ? "" : string.Join(" ", Players().Select(p => _logic.ScoreOf(p)));
        public string Result => DescribeResult();

        /// <summary>各プレイヤーの取得ペア数。</summary>
        public string Pairs => _logic == null ? "" : string.Join(" ", Players().Select(p => _logic.PairsOf(p)));

        /// <summary>全カードの持ち主の番号。未取得は「-」。</summary>
        public string Owners => _logic == null ? "" : string.Join(" ", Enumerable.Range(0, _logic.Spec.CardCount).Select(i => _logic.OwnerAt(i) < 0 ? "-" : _logic.OwnerAt(i).ToString()));

        /// <summary>全カードの表裏。裏は「--」、表は絵柄の文字、成立済みは「*」付き。裏の絵柄は出さない。</summary>
        public string Cards => DescribeCards();

        private bool IsCpuTurn => _logic != null && !IsOver && _menu.IsCpu(_logic.CurrentPlayer);
        private bool IsOver => _logic.Phase == GamePhase.Cleared || _logic.Phase == GamePhase.GameOver;

        private void Awake()
        {
            Application.runInBackground = true;
            int seed = useFixedSeed ? fixedSeed : Guid.NewGuid().GetHashCode();
            _shuffler = new DeckShuffler(seed);
            _cpu = new CpuPlayer(seed);
            Render();
        }

        private void Update()
        {
            bool changed = HandleInput();
            if (IsCpuTurn) changed |= StepCpu();
            if (changed) Render();
        }

        private bool HandleInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return false;

            bool left = _left.Poll(kb), right = _right.Poll(kb), up = _up.Poll(kb), down = _down.Poll(kb);
            bool space = _space.Poll(kb), restart = _restart.Poll(kb), escape = _escape.Poll(kb);
            int dx = right ? 1 : left ? -1 : 0;
            int dy = dx != 0 ? 0 : down ? 1 : up ? -1 : 0;

            if (_logic == null)
            {
                if (space) StartGame();
                else if (dx != 0 || dy != 0) _menu.Move(dx, dy);
                else return false;
                return true;
            }

            if (escape) _logic = null;
            else if (restart) StartGame();
            else if (IsCpuTurn) return false;
            else if (dx != 0 || dy != 0) _logic.MoveCursor(dx, dy);
            else if (space) PressSpace();
            else return false;
            return true;
        }

        private void StartGame()
        {
            var deck = _shuffler.Next(_menu.Spec);
            if (_logic == null)
            {
                _logic = new MemoryLogic(deck, _menu.PlayerCount, _menu.Spec);
                LayOutBoard();
            }
            else _logic.Restart(deck);
            _cpu.Reset();
            _wasVisible = new bool[_logic.Spec.CardCount];
            _cpuTimer = 0f;
        }

        private void PressSpace()
        {
            _logic.PressSpace();
            _cpuTimer = 0f;
            for (int i = 0; i < _wasVisible.Length; i++)
            {
                bool visible = _logic.FaceAt(i) != CardFace.Down;
                if (visible && !_wasVisible[i]) _cpu.Observe(i, _logic.VisibleKeyAt(i));
                _wasVisible[i] = visible;
            }
        }

        private bool StepCpu()
        {
            _cpuTimer += Time.deltaTime;
            bool showingMiss = _logic.Phase == GamePhase.ShowingMiss;
            if (_cpuTimer < (showingMiss ? CpuShowMissSeconds : CpuStepSeconds)) return false;

            if (!showingMiss)
            {
                int[] selectable = Enumerable.Range(0, _logic.Spec.CardCount).Where(i => _logic.FaceAt(i) == CardFace.Down).ToArray();
                int choice;
                if (_logic.Phase == GamePhase.PickFirst) choice = _cpu.ChooseFirst(selectable);
                else
                {
                    int first = Enumerable.Range(0, _logic.Spec.CardCount).First(i => _logic.FaceAt(i) == CardFace.Up);
                    choice = _cpu.ChooseSecond(selectable, _logic.VisibleKeyAt(first));
                }
                _logic.MoveCursor(choice % _logic.Spec.Columns - _logic.Cursor % _logic.Spec.Columns,
                                  choice / _logic.Spec.Columns - _logic.Cursor / _logic.Spec.Columns);
            }
            PressSpace();
            return true;
        }

        private void Render()
        {
            bool inMenu = _logic == null;
            menuPanel.SetActive(inMenu);
            if (inMenu)
            {
                menuText.text = DescribeMenu();
                resultPanel.SetActive(false);
                return;
            }

            for (int i = 0; i < _logic.Spec.CardCount; i++)
            {
                if (_logic.FaceAt(i) == CardFace.Down) cards[i].ShowBack();
                else cards[i].ShowFront(_logic.VisibleSymbolAt(i), _logic.Spec.IsTrump, _logic.FaceAt(i) == CardFace.Matched, OwnerLabel(i));
            }
            cursorFrame.anchoredPosition = ((RectTransform)cards[_logic.Cursor].transform).anchoredPosition;

            bool versus = _logic.IsVersus;
            foreach (var solo in new[] { matchedText, missesText, scoreText, comboText }) solo.gameObject.SetActive(!versus);
            playersText.gameObject.SetActive(versus);
            matchedText.text = $"PAIRS {_logic.Matched}/{_logic.Spec.PairCount}";
            missesText.text = $"MISS {_logic.Misses}/{_logic.Spec.MissLimit}";
            scoreText.text = $"SCORE {_logic.Score}";
            comboText.text = $"COMBO {_logic.Combo}";
            playersText.text = string.Join("     ", Players().Select(p => $"{(p == _logic.CurrentPlayer && !IsOver ? "> " : "")}{NameOf(p)}  {_logic.ScoreOf(p)} pt ({_logic.PairsOf(p)})"));
            resultText.text = DescribeResult();
            resultPanel.SetActive(IsOver);
            if (IsOver) PlaceResult(versus);
        }

        /// <summary>盤面の列・行に合わせてカードを並べ、使わないカードを隠す。</summary>
        private void LayOutBoard()
        {
            var spec = _logic.Spec;
            var cell = new Vector2(Mathf.Min(150f, BoardSize.x / spec.Columns), Mathf.Min(180f, BoardSize.y / spec.Rows));
            var size = cell * 0.88f;
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].gameObject.SetActive(i < spec.CardCount);
                if (i >= spec.CardCount) continue;
                float x = (i % spec.Columns - (spec.Columns - 1) / 2f) * cell.x;
                float y = ((spec.Rows - 1) / 2f - i / spec.Columns) * cell.y + BoardCenterY;
                cards[i].Place(new Vector2(x, y), size);
            }
            cursorFrame.sizeDelta = size + Vector2.one * 12f;
        }

        /// <summary>1人用の結果は、最後の2枚を隠さないよう盤面の下に出す。対戦の順位は中央に出す。</summary>
        private void PlaceResult(bool versus)
        {
            var panel = (RectTransform)resultPanel.transform;
            panel.anchoredPosition = versus ? Vector2.zero : new Vector2(0f, -320f);
            panel.sizeDelta = versus ? new Vector2(620f, 330f) : new Vector2(620f, 64f);
            if (!versus) resultText.text += "   (R: Restart)";
            resultText.fontSize = versus ? 54 : 38;
        }

        private IEnumerable<int> Players() => Enumerable.Range(0, _logic.PlayerCount);

        private string NameOf(int player) => (_menu.IsCpu(player) ? "CPU" : "P") + (player + 1);

        private string OwnerLabel(int index) => _logic.IsVersus && _logic.OwnerAt(index) >= 0 ? NameOf(_logic.OwnerAt(index)) : "";

        private string DescribeMenu()
        {
            string Row(GameMode mode, string label) => (_menu.Mode == mode ? "> " : "   ") + label;
            string option = _menu.Mode == GameMode.Solo
                ? $"< {_menu.DifficultyName}  {_menu.Spec.CardCount} CARDS >"
                : $"< {_menu.PlayerCount} PLAYERS >  TRUMP {_menu.Spec.CardCount} CARDS";
            return $"MEMORY MATCH\n\n{Row(GameMode.Solo, "SOLO")}\n{Row(GameMode.VsCpu, "VS CPU")}\n{Row(GameMode.VsHuman, "VS PLAYERS")}\n\n{option}";
        }

        private string DescribeResult()
        {
            if (_logic == null || !IsOver) return "";
            if (_logic.Phase == GamePhase.GameOver) return "GAME OVER";
            if (!_logic.IsVersus) return "CLEAR!";
            return string.Join("\n", Players().OrderBy(p => _logic.RankOf(p)).Select(p => $"{_logic.RankOf(p)}. {NameOf(p)}   {_logic.ScoreOf(p)}"));
        }

        /// <summary>1人用は絵柄の文字（A〜）、トランプはスート（S/H/D/C）と数字。</summary>
        private string CardCode(int card) => _logic.Spec.IsTrump
            ? "SHDC"[card / 13].ToString() + (card % 13 + 1)
            : ((char)('A' + card)).ToString();

        private string DescribeCards()
        {
            if (_logic == null) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < _logic.Spec.CardCount; i++)
            {
                if (i > 0) sb.Append(' ');
                var face = _logic.FaceAt(i);
                if (face == CardFace.Down) sb.Append("--");
                else sb.Append(CardCode(_logic.VisibleSymbolAt(i))).Append(face == CardFace.Matched ? "*" : "");
            }
            return sb.ToString();
        }
    }
}
