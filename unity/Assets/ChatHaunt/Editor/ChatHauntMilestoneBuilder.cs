using System;
using System.IO;
using ChatHaunt;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ChatHaunt.Editor
{
    public static class ChatHauntMilestoneBuilder
    {
        const string Root = "Assets/ChatHaunt";
        const string ScenePath = Root + "/Scenes/ChatHauntMilestone.unity";

        [MenuItem("ChatHaunt/Build First Milestone Scene")]
        public static void Build()
        {
            Directory.CreateDirectory(Root + "/Scenes");
            Directory.CreateDirectory(Root + "/Materials");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ChatHauntMilestone";

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.025f, 0.035f, 0.045f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.035f;
            RenderSettings.fogColor = new Color(0.035f, 0.045f, 0.055f);

            Material plaster = MakeMaterial("Damp Plaster", new Color(0.12f, 0.16f, 0.18f));
            Material floor = MakeMaterial("Dark Timber", new Color(0.13f, 0.085f, 0.07f));
            Material trim = MakeMaterial("Weathered Trim", new Color(0.06f, 0.045f, 0.04f));
            Material doorMat = MakeMaterial("Door Wood", new Color(0.22f, 0.12f, 0.09f));
            Material black = MakeMaterial("Silhouette Black", new Color(0.002f, 0.002f, 0.004f));

            GameObject environment = new GameObject("Short Hallway");
            Box("Floor", new Vector3(0, -0.15f, 0), new Vector3(4.4f, 0.3f, 12f), floor, environment.transform);
            Box("Ceiling", new Vector3(0, 3f, 0), new Vector3(4.4f, 0.2f, 12f), plaster, environment.transform);
            Box("Left wall", new Vector3(-2.2f, 1.5f, 0), new Vector3(0.2f, 3f, 12f), plaster, environment.transform);
            Box("Right wall", new Vector3(2.2f, 1.5f, 0), new Vector3(0.2f, 3f, 12f), plaster, environment.transform);
            Box("Near wall", new Vector3(0, 1.5f, -6f), new Vector3(4.4f, 3f, 0.2f), plaster, environment.transform);
            Box("End wall left", new Vector3(-1.5f, 1.5f, 6f), new Vector3(1.4f, 3f, 0.2f), plaster, environment.transform);
            Box("End wall right", new Vector3(1.5f, 1.5f, 6f), new Vector3(1.4f, 3f, 0.2f), plaster, environment.transform);
            Box("Door lintel", new Vector3(0, 2.7f, 6f), new Vector3(1.6f, 0.6f, 0.2f), trim, environment.transform);
            Box("Door threshold", new Vector3(0, 0.06f, 6f), new Vector3(1.7f, 0.12f, 0.36f), trim, environment.transform);
            Box("Dark landing floor", new Vector3(0, -0.15f, 7f), new Vector3(4.4f, 0.3f, 2.2f), floor, environment.transform);
            Box("Dark landing left wall", new Vector3(-2.2f, 1.5f, 7f), new Vector3(0.2f, 3f, 2.2f), plaster, environment.transform);
            Box("Dark landing right wall", new Vector3(2.2f, 1.5f, 7f), new Vector3(0.2f, 3f, 2.2f), plaster, environment.transform);
            Box("Dark landing back wall", new Vector3(0, 1.5f, 8.1f), new Vector3(4.4f, 3f, 0.2f), trim, environment.transform);
            for (int i = 0; i < 5; i++)
            {
                float z = -4.7f + i * 2.3f;
                Box("Left wall rib " + (i + 1), new Vector3(-2.06f, 1.5f, z), new Vector3(0.12f, 2.8f, 0.13f), trim, environment.transform);
                Box("Right wall rib " + (i + 1), new Vector3(2.06f, 1.5f, z), new Vector3(0.12f, 2.8f, 0.13f), trim, environment.transform);
                Box("Floor seam " + (i + 1), new Vector3(0, 0.008f, z), new Vector3(4.0f, 0.015f, 0.025f), trim, environment.transform);
            }

            GameObject doorPivot = new GameObject("Interactable Door");
            doorPivot.transform.SetPositionAndRotation(new Vector3(-0.78f, 1.22f, 5.89f), Quaternion.identity);
            GameObject doorSlab = Box("Door slab", new Vector3(0.78f, 0, 0), new Vector3(1.55f, 2.42f, 0.1f), doorMat, doorPivot.transform, true);
            Box("Door handle", new Vector3(0.55f, -0.04f, -0.105f), new Vector3(0.1f, 0.22f, 0.07f), trim, doorSlab.transform, true);
            ChatHauntDoor door = doorPivot.AddComponent<ChatHauntDoor>();
            door.movingPart = doorSlab.transform;
            ChatHauntAudio doorCue = doorPivot.AddComponent<ChatHauntAudio>();
            doorCue.tone = ChatHauntAudio.Tone.Creak;
            doorCue.volume = 0.28f;
            door.audioCue = doorCue;

            GameObject roomLamp = new GameObject("Room Light");
            roomLamp.transform.position = new Vector3(0, 2.55f, -0.2f);
            Light lamp = roomLamp.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(1f, 0.67f, 0.39f);
            lamp.intensity = 3.8f;
            lamp.range = 8.5f;
            lamp.shadows = LightShadows.Soft;
            Box("Lamp housing", new Vector3(0, 2.85f, -0.2f), new Vector3(0.4f, 0.12f, 0.4f), trim, environment.transform);

            GameObject lightRigObject = new GameObject("Room Light Rig");
            ChatHauntLightRig lightRig = lightRigObject.AddComponent<ChatHauntLightRig>();
            lightRig.roomLights = new[] { lamp };

            GameObject playerObject = new GameObject("First Person Player");
            playerObject.transform.position = new Vector3(0, 0.08f, -4.8f);
            CharacterController motor = playerObject.AddComponent<CharacterController>();
            motor.height = 1.8f;
            motor.radius = 0.27f;
            motor.center = new Vector3(0, 0.9f, 0);
            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.transform.SetParent(playerObject.transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 1.62f, 0);
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 68f;
            camera.farClipPlane = 60f;
            camera.backgroundColor = RenderSettings.fogColor;
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();
            ChatHauntPlayer player = playerObject.AddComponent<ChatHauntPlayer>();
            player.view = camera;
            GameObject flashlightObject = new GameObject("Flashlight Beam");
            flashlightObject.transform.SetParent(cameraObject.transform, false);
            flashlightObject.transform.localPosition = new Vector3(0, -0.08f, 0.2f);
            Light beam = flashlightObject.AddComponent<Light>();
            beam.type = LightType.Spot;
            beam.spotAngle = 48f;
            beam.range = 11f;
            beam.intensity = 5f;
            beam.color = new Color(0.72f, 0.85f, 1f);
            beam.enabled = false;
            ChatHauntFlashlight flashlight = cameraObject.AddComponent<ChatHauntFlashlight>();
            flashlight.beam = beam;

            GameObject ambienceObject = new GameObject("Placeholder Ambient Drone");
            ChatHauntAudio ambience = ambienceObject.AddComponent<ChatHauntAudio>();
            ambience.tone = ChatHauntAudio.Tone.Ambience;
            ambience.volume = 0.12f;

            GameObject silhouette = new GameObject("Dark Silhouette");
            silhouette.transform.position = new Vector3(0, 0, 2.7f);
            Box("Torso", new Vector3(0, 1.15f, 0), new Vector3(0.56f, 1.2f, 0.24f), black, silhouette.transform, true);
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(silhouette.transform, false);
            head.transform.localPosition = new Vector3(0, 2.0f, 0);
            head.transform.localScale = new Vector3(0.46f, 0.5f, 0.43f);
            head.GetComponent<Renderer>().sharedMaterial = black;
            UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());
            Box("Left arm", new Vector3(-0.42f, 1.13f, 0), new Vector3(0.2f, 1.25f, 0.2f), black, silhouette.transform, true);
            Box("Right arm", new Vector3(0.42f, 1.13f, 0), new Vector3(0.2f, 1.25f, 0.2f), black, silhouette.transform, true);
            silhouette.SetActive(false);

            GameObject scareObject = new GameObject("Blackout And Silhouette Event");
            scareObject.transform.position = silhouette.transform.position;
            ChatHauntAudio sting = scareObject.AddComponent<ChatHauntAudio>();
            sting.tone = ChatHauntAudio.Tone.Sting;
            sting.volume = 0.36f;
            ChatHauntHorrorEvent scare = scareObject.AddComponent<ChatHauntHorrorEvent>();
            scare.lightRig = lightRig;
            scare.silhouette = silhouette;
            scare.cue = sting;
            scare.oneShot = false;
            ChatHauntLocalBridge localBridge = scareObject.AddComponent<ChatHauntLocalBridge>();
            localBridge.horrorEvent = scare;

            GameObject triggerObject = new GameObject("Walk Into Scare Trigger");
            triggerObject.transform.position = new Vector3(0, 1.2f, -1.35f);
            BoxCollider triggerCollider = triggerObject.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(3.8f, 2.4f, 0.3f);
            ChatHauntEventTrigger trigger = triggerObject.AddComponent<ChatHauntEventTrigger>();
            trigger.horrorEvent = scare;

            GameObject hudObject = new GameObject("Controls And Interaction HUD");
            ChatHauntHUD hud = hudObject.AddComponent<ChatHauntHUD>();
            hud.player = player;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.productName = "ChatHaunt";
            AssetDatabase.SaveAssets();
            Validate();
            Debug.Log("CHAT_HAUNT_MILESTONE_BUILT " + ScenePath);
        }

        [MenuItem("ChatHaunt/Validate First Milestone Scene")]
        public static void Validate()
        {
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == ScenePath, "milestone scene is active");
            ChatHauntPlayer player = UnityEngine.Object.FindFirstObjectByType<ChatHauntPlayer>();
            ChatHauntDoor door = UnityEngine.Object.FindFirstObjectByType<ChatHauntDoor>();
            ChatHauntLightRig lights = UnityEngine.Object.FindFirstObjectByType<ChatHauntLightRig>();
            ChatHauntFlashlight flashlight = UnityEngine.Object.FindFirstObjectByType<ChatHauntFlashlight>();
            ChatHauntHorrorEvent scare = UnityEngine.Object.FindFirstObjectByType<ChatHauntHorrorEvent>();
            ChatHauntEventTrigger trigger = UnityEngine.Object.FindFirstObjectByType<ChatHauntEventTrigger>();
            Require(player && player.view && player.GetComponent<CharacterController>(), "player, camera and motor");
            Require(door && door.movingPart && door.audioCue, "door and spatial audio");
            Require(lights && lights.roomLights.Length >= 1 && lights.roomLights[0], "room lighting");
            Require(flashlight && flashlight.beam, "flashlight");
            Require(scare && scare.lightRig == lights && scare.silhouette && scare.cue, "horror event wiring");
            Require(trigger && trigger.horrorEvent == scare && trigger.GetComponent<Collider>().isTrigger, "walk-in trigger");
            Require(UnityEngine.Object.FindFirstObjectByType<ChatHauntAudio>() != null, "placeholder audio");
            Debug.Log("CHAT_HAUNT_MILESTONE_SCENE_VALIDATED");
        }

        static void Require(bool condition, string label)
        {
            if (!condition) throw new Exception("ChatHaunt validation failed: " + label);
        }

        static Material MakeMaterial(string name, Color color)
        {
            string path = Root + "/Materials/" + name.Replace(' ', '_') + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader) { name = name, color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, Transform parent, bool local = false)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            if (local) box.transform.localPosition = position;
            else box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            if (name == "Door handle" || name == "Lamp housing" || name.StartsWith("Floor seam"))
                UnityEngine.Object.DestroyImmediate(box.GetComponent<Collider>());
            return box;
        }
    }
}
