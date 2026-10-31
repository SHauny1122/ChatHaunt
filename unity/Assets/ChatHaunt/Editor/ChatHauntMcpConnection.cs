using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

namespace ChatHaunt.Editor
{
    public static class ChatHauntMcpConnection
    {
        [MenuItem("ChatHaunt/Connect This Editor To Unity MCP")]
        public static async void Connect()
        {
            bool connected = await new BridgeControlService().StartAsync();
            Debug.Log("CHAT_HAUNT_MCP_CONNECTED=" + connected);
        }
    }
}
