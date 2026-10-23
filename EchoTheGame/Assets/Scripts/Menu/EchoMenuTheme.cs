using TMPro;
using UnityEngine;

namespace Project.Echo.Menu
{
    [CreateAssetMenu(menuName = "Echo/UI Theme")]
    public class EchoMenuTheme : ScriptableObject
    {
        public Color abyss = new Color32(2, 12, 19, 255);
        public Color panel = new Color32(5, 24, 33, 250);
        public Color seafoam = new Color32(155, 233, 226, 255);
        public Color muted = new Color32(147, 185, 194, 255);
        public Color amber = new Color32(244, 185, 79, 255);
        public TMP_FontAsset font;
        public Material lightTextMaterial;
        public Sprite sonar;
        public Sprite submarine;
        public Sprite currency;
        public Sprite arrow;
        public Sprite signal;
        public Sprite roundedButton;
    }
}
