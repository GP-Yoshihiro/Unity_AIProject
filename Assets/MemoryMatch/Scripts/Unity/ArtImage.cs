using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatch
{
    /// <summary>Image に TableArt の絵を当てる。</summary>
    [RequireComponent(typeof(Image))]
    public sealed class ArtImage : MonoBehaviour
    {
        [SerializeField] private ArtKind kind;

        private void Awake() => Apply(GetComponent<Image>(), kind);

        public static void Apply(Image image, ArtKind kind)
        {
            image.sprite = TableArt.Get(kind);
            image.type = kind == ArtKind.Panel ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = false;
        }
    }
}
