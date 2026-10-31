using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ChatHaunt
{
    // Polls the loopback-only Node bridge while the scene is running.
    public sealed class ChatHauntLocalBridge : MonoBehaviour
    {
        [System.Serializable]
        private sealed class LightsMessage { public int id; public string command; }

        [System.Serializable]
        private sealed class LightsAck { public int id; public bool triggered; }

        public ChatHauntHorrorEvent horrorEvent;
        public ChatHauntHouseEvents houseEvents;
        public string serverUrl = "http://127.0.0.1:3177";
        public int LastMessageId { get; private set; }
        public bool LastTriggered { get; private set; }
        public bool PollConnected { get; private set; }
        public string LastError { get; private set; }

        private void OnEnable()
        {
            Application.runInBackground = true;
            if (houseEvents == null) houseEvents = FindFirstObjectByType<ChatHauntHouseEvents>();
            StartCoroutine(PollLoop());
        }

        private bool HouseLightsOn()
        {
            if (horrorEvent == null || horrorEvent.houseLights == null) return true;
            foreach (ChatHauntLightRig rig in horrorEvent.houseLights)
                if (rig != null && rig.IsBlackout) return false;
            return true;
        }

        private bool Trigger(string command)
        {
            if (command == "!lights" || command == "LIGHTS_HALLWAY_OFF")
            {
                int before = horrorEvent != null ? horrorEvent.TriggerCount : 0;
                if (horrorEvent != null) horrorEvent.TriggerEvent();
                return horrorEvent != null && horrorEvent.TriggerCount > before;
            }
            if (command == "HALLWAY_GHOST" || command == "LIVING_ROOM_GHOST")
                return houseEvents != null && houseEvents.TriggerGhost(command);
            if (command == "DOOR_SLAM")
                return houseEvents != null && houseEvents.Trigger(command);
            return false;
        }

        private IEnumerator PollLoop()
        {
            while (true)
            {
                using (var request = UnityWebRequest.Get(serverUrl + "/unity/poll?lightsOn=" + (HouseLightsOn() ? "true" : "false")))
                {
                    request.timeout = 2;
                    yield return request.SendWebRequest();
                    PollConnected = request.result == UnityWebRequest.Result.Success;
                    if (!PollConnected)
                    {
                        LastError = request.error;
                        yield return new WaitForSecondsRealtime(1f);
                        continue;
                    }

                    LastError = null;
                    LightsMessage message = null;
                    try { message = JsonUtility.FromJson<LightsMessage>(request.downloadHandler.text); }
                    catch (System.Exception error) { LastError = error.Message; }

                    if (message != null && message.id > 0)
                    {
                        LastMessageId = message.id;
                        LastTriggered = Trigger(message.command);
                        Debug.Log("ChatHaunt local " + message.command + " " + message.id + ": " + (LastTriggered ? "scare triggered" : "scare unavailable"));

                        string json = JsonUtility.ToJson(new LightsAck { id = message.id, triggered = LastTriggered });
                        byte[] body = Encoding.UTF8.GetBytes(json);
                        using (var ack = new UnityWebRequest(serverUrl + "/unity/ack", "POST"))
                        {
                            ack.uploadHandler = new UploadHandlerRaw(body);
                            ack.downloadHandler = new DownloadHandlerBuffer();
                            ack.SetRequestHeader("Content-Type", "application/json");
                            ack.timeout = 2;
                            yield return ack.SendWebRequest();
                            if (ack.result != UnityWebRequest.Result.Success) LastError = "Acknowledgement: " + ack.error;
                        }
                    }
                }
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }
    }
}
