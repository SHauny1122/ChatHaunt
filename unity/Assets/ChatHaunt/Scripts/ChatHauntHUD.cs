using UnityEngine;

namespace ChatHaunt
{
    public sealed class ChatHauntHUD : MonoBehaviour
    {
        public ChatHauntPlayer player;
        GUIStyle style;

        void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = 18 };
                style.normal.textColor = new Color(0.85f, 0.8f, 0.7f);
            }
            GUI.Label(new Rect(20, 20, 700, 30), "WASD move   Mouse look   E interact   F flashlight   Esc cursor", style);
            if (player != null && player.Focused != null)
                GUI.Label(new Rect(Screen.width / 2f - 100, Screen.height * 0.7f, 300, 30), player.Focused.Prompt, style);
            GUI.Label(new Rect(Screen.width / 2f - 4, Screen.height / 2f - 15, 30, 30), "+", style);
        }
    }
}
