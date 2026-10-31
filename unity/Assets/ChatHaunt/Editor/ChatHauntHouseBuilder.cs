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
    public static class ChatHauntHouseBuilder
    {
        const string Root = "Assets/ChatHaunt";
        const string ScenePath = Root + "/Scenes/ChatHauntHouse.unity";
        static Material wall, timber, trim, doorWood, tatami, stone, fabric, metal;

        [MenuItem("ChatHaunt/Build Compact House")]
        public static void Build()
        {
            Directory.CreateDirectory(Root + "/Scenes");
            Directory.CreateDirectory(Root + "/Materials");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ChatHauntHouse";
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;
            RenderSettings.ambientIntensity = 0f;
            RenderSettings.reflectionIntensity = 0f;
            RenderSettings.ambientProbe = new SphericalHarmonicsL2();
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.026f;
            RenderSettings.fogColor = Color.black;

            wall = ReferenceMat("HallwayV3/M_JH_DirtyPlaster_V3");
            timber = ReferenceMat("HallwayV3/M_JH_AgedTimber_V3");
            trim = ReferenceMat("HallwayV3/M_JH_AgedTimber_V3");
            doorWood = ReferenceMat("HallwayV3/M_JH_DamagedShoji_V3");
            tatami = ReferenceMat("BedroomV1/M_JH_AgedTatami");
            stone = Mat("House Stone", new Color(0.09f, 0.12f, 0.13f));
            fabric = ReferenceMat("BedroomV1/M_JH_StainedFabric");
            metal = Mat("House Metal", new Color(0.14f, 0.17f, 0.18f));

            GameObject structure = new GameObject("House Shell - New Geometry");
            GameObject bedroom = new GameObject("Bedroom");
            GameObject hallway = new GameObject("Hallway");
            GameObject living = new GameObject("Living Room");
            GameObject kitchen = new GameObject("Kitchen");
            GameObject genkan = new GameObject("Genkan Entrance");

            Box("Bedroom floor", V(-4,-.15f,4), V(4,.3f,4), timber, structure.transform);
            Box("Hallway floor", V(-1,-.15f,.5f), V(2,.3f,11), timber, structure.transform);
            Box("Living floor", V(2.5f,-.15f,1.5f), V(5,.3f,5), timber, structure.transform);
            Box("Kitchen floor", V(2.5f,-.15f,-3), V(5,.3f,4), timber, structure.transform);
            Box("Genkan stone floor", V(-.5f,-.19f,-6.5f), V(3,.3f,3), stone, structure.transform);

            // Compact version of the reference's central hall and room openings.
            Wall("Bedroom west", -6, 4, .16f, 4, structure.transform);
            WallZ("Bedroom north", -4, 6, 4, structure.transform);
            WallZ("Bedroom south", -4, 2, 4, structure.transform);
            Wall("Bedroom door south jamb", -2, 2.65f, .16f, 1.3f, structure.transform);
            Wall("Bedroom door north jamb", -2, 5.35f, .16f, 1.3f, structure.transform);
            Lintel("Bedroom lintel", -2, 4, true, 1.4f, structure.transform);
            Wall("Hall west", -2, -1.5f, .16f, 7, structure.transform);
            WallZ("Hall north", -1, 6, 2, structure.transform);
            Wall("Hall east south", 0, -2.5f, .16f, 5, structure.transform);
            Wall("Hall east north", 0, 4, .16f, 4, structure.transform);
            Lintel("Living lintel", 0, 1, true, 2, structure.transform);
            WallZ("Living north", 2.5f, 4, 5, structure.transform);
            Wall("Living and kitchen east", 5, -.5f, .16f, 9, structure.transform);
            WallZ("Living kitchen west pier", 1, -1, 2, structure.transform);
            WallZ("Living kitchen east pier", 4.5f, -1, 1, structure.transform);
            Lintel("Kitchen opening lintel", 3, -1, false, 2, structure.transform);
            WallZ("Kitchen south", 3, -5, 4, structure.transform);
            Wall("Genkan west", -2, -6.5f, .16f, 3, structure.transform);
            Wall("Genkan east", 1, -6.5f, .16f, 3, structure.transform);
            WallZ("Genkan front", -.5f, -8, 3, structure.transform);
            Lintel("Genkan hall lintel", -1, -5, false, 2, structure.transform);

            foreach (var room in new[] { bedroom, hallway, living, kitchen, genkan })
            {
                Bounds b = room == bedroom ? new Bounds(V(-4,1.4f,4),V(4,2.8f,4)) :
                    room == hallway ? new Bounds(V(-1,1.4f,.5f),V(2,2.8f,11)) :
                    room == living ? new Bounds(V(2.5f,1.4f,1.5f),V(5,2.8f,5)) :
                    room == kitchen ? new Bounds(V(2.5f,1.4f,-3),V(5,2.8f,4)) :
                    new Bounds(V(-.5f,1.4f,-6.5f),V(3,2.8f,3));
                Box(room.name + " ceiling", V(b.center.x,2.86f,b.center.z), V(b.size.x,.12f,b.size.z), trim, room.transform);
            }

            // Simple furniture and trim establish distinct spaces; all meshes are new primitives.
            for (int x = 0; x < 2; x++) for (int z = 0; z < 2; z++)
                Decor("Tatami mat", V(-4.9f + x*1.8f,.015f,3 + z*1.8f), V(1.7f,.025f,1.7f), tatami, bedroom.transform);
            Box("Futon", V(-4.7f,.12f,5), V(1.6f,.22f,1.3f), fabric, bedroom.transform);
            Decor("Pillow", V(-4.7f,.25f,5.38f), V(.6f,.1f,.25f), wall, bedroom.transform);
            Box("Bedroom shelf", V(-5.55f,1.2f,3.1f), V(.55f,.08f,1.4f), doorWood, bedroom.transform);
            Decor("Empty frame", V(-5.83f,1.64f,3.1f), V(.04f,.66f,.48f), trim, bedroom.transform);

            for (int i = 0; i < 6; i++)
                Decor("Hall floor seam", V(-1,.012f,-4.2f+i*1.7f), V(1.85f,.012f,.025f), trim, hallway.transform);
            for (int i = 0; i < 5; i++)
                Decor("Hall timber rib", V(-1.89f,1.4f,-3.7f+i*1.7f), V(.09f,2.75f,.12f), trim, hallway.transform);
            Decor("Hall wall frame", V(-1.89f,1.65f,-.3f), V(.06f,.75f,.55f), doorWood, hallway.transform);

            Box("Low table", V(3.4f,.45f,2.35f), V(1.7f,.18f,1.05f), doorWood, living.transform);
            Box("Floor cushion", V(3.4f,.11f,.95f), V(.8f,.22f,.7f), fabric, living.transform);
            Box("Living cabinet", V(4.45f,.55f,3.25f), V(.8f,1.1f,.65f), doorWood, living.transform);
            Decor("Window panel", V(4.89f,1.7f,1.4f), V(.025f,1.1f,1.3f), stone, living.transform);

            Box("Kitchen counter", V(4.35f,.53f,-3), V(1.0f,1.05f,2.6f), doorWood, kitchen.transform);
            Decor("Sink", V(4.35f,1.08f,-3.6f), V(.72f,.06f,.65f), metal, kitchen.transform);
            Box("Kitchen cabinet", V(2.7f,.52f,-4.55f), V(1.45f,1.04f,.66f), doorWood, kitchen.transform);
            Decor("Kitchen shelf", V(4.7f,1.95f,-2.1f), V(.46f,.08f,1.45f), doorWood, kitchen.transform);

            Box("Shoe bench", V(-1.62f,.23f,-7.15f), V(.5f,.45f,1.2f), doorWood, genkan.transform);
            Decor("Genkan threshold", V(-.5f,.04f,-5.03f), V(2.8f,.08f,.17f), trim, genkan.transform);
            Decor("Front door panel", V(-.5f,1.35f,-7.88f), V(1.55f,2.55f,.09f), doorWood, genkan.transform);

            ChatHauntSlidingDoor bedroomDoor = Door("Bedroom Sliding Door", V(-2,0,4), 1.36f, 1.55f, bedroom.transform);
            ChatHauntSlidingDoor livingDoor = Door("Living Room Sliding Door", V(0,0,1), 1.9f, 2.05f, living.transform);

            var bedroomRoom = Room(bedroom, ChatHauntRoomId.Bedroom, V(-4,2.55f,4), new Color(1,.66f,.39f), new[]{bedroomDoor});
            var hallwayRoom = Room(hallway, ChatHauntRoomId.Hallway, V(-1,2.55f,0), new Color(1,.63f,.35f), new ChatHauntSlidingDoor[0]);
            var livingRoom = Room(living, ChatHauntRoomId.LivingRoom, V(2.5f,2.55f,1.6f), new Color(1,.68f,.45f), new[]{livingDoor});
            var kitchenRoom = Room(kitchen, ChatHauntRoomId.Kitchen, V(2.7f,2.55f,-3), new Color(.72f,.82f,1), new ChatHauntSlidingDoor[0]);
            var genkanRoom = Room(genkan, ChatHauntRoomId.Genkan, V(-.5f,2.45f,-6.7f), new Color(.67f,.77f,1), new ChatHauntSlidingDoor[0]);

            GameObject night = new GameObject("Night Directional Light");
            night.transform.rotation = Quaternion.Euler(45, -30, 0);
            Light moon = night.AddComponent<Light>();
            moon.type = LightType.Directional; moon.intensity = 0f; moon.color = new Color(.55f,.7f,1);

            GameObject playerObject = new GameObject("First Person Player");
            playerObject.transform.position = V(-4.2f,.08f,4);
            playerObject.transform.rotation = Quaternion.Euler(0,90,0);
            CharacterController motor = playerObject.AddComponent<CharacterController>();
            motor.height=1.8f; motor.radius=.27f; motor.center=V(0,.9f,0);
            GameObject viewObject = new GameObject("Player Camera");
            viewObject.transform.SetParent(playerObject.transform, false);
            viewObject.transform.localPosition=V(0,1.62f,0);
            viewObject.tag="MainCamera";
            Camera camera=viewObject.AddComponent<Camera>();
            camera.fieldOfView=68; camera.farClipPlane=45;
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=RenderSettings.fogColor;
            viewObject.AddComponent<AudioListener>();
            ChatHauntPlayer player=playerObject.AddComponent<ChatHauntPlayer>(); player.view=camera;
            GameObject flashlightObject=new GameObject("Flashlight Beam");
            flashlightObject.transform.SetParent(viewObject.transform,false);
            flashlightObject.transform.localPosition=V(0,-.08f,.2f);
            flashlightObject.transform.localRotation=Quaternion.Euler(10,0,0);
            Light beam=flashlightObject.AddComponent<Light>();
            beam.type=LightType.Spot; beam.spotAngle=55; beam.range=6f; beam.intensity=9;
            beam.color=new Color(.72f,.85f,1); beam.enabled=false;
            ChatHauntFlashlight flashlight=viewObject.AddComponent<ChatHauntFlashlight>(); flashlight.beam=beam;

            GameObject ambienceObject=new GameObject("Placeholder Ambient Drone");
            ChatHauntAudio ambience=ambienceObject.AddComponent<ChatHauntAudio>();
            ambience.tone=ChatHauntAudio.Tone.Ambience; ambience.volume=.1f;
            GameObject eventObject=new GameObject("Hallway Blackout Event");
            ChatHauntAudio sting=eventObject.AddComponent<ChatHauntAudio>();
            sting.tone=ChatHauntAudio.Tone.Sting; sting.volume=.3f;
            ChatHauntHorrorEvent scare=eventObject.AddComponent<ChatHauntHorrorEvent>();
            scare.lightRig=hallwayRoom.lighting;
            scare.houseLights=new[]{bedroomRoom.lighting,hallwayRoom.lighting,livingRoom.lighting,kitchenRoom.lighting,genkanRoom.lighting};
            scare.cue=sting; scare.oneShot=false;
            ChatHauntLocalBridge local=eventObject.AddComponent<ChatHauntLocalBridge>(); local.horrorEvent=scare;

            GameObject registryObject=new GameObject("House Rooms And Events");
            ChatHauntHouseEvents registry=registryObject.AddComponent<ChatHauntHouseEvents>();
            registry.rooms=new[]{bedroomRoom,hallwayRoom,livingRoom,kitchenRoom,genkanRoom};
            registry.hallwayBlackout=scare;
            GameObject ghostPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Characters/GhostWoman/Prefabs/GhostWoman.prefab");
            if (!ghostPrefab) throw new Exception("GhostWoman prefab is missing");
            GameObject ghostObject=new GameObject("Reusable GhostWoman Scare Event");
            GhostScareEvent ghost=ghostObject.AddComponent<GhostScareEvent>();
            ghost.ghostPrefab=ghostPrefab;
            ghost.placements=new[] {
                new GhostScareEvent.Placement { id="HALLWAY_GHOST", roomId=ChatHauntRoomId.Hallway, anchor=Anchor("Ghost Hallway Far End",V(-1,0,-3.65f),0,hallway.transform), animationState="GW_FloorCrawl_Loop", visibleSeconds=6f },
                new GhostScareEvent.Placement { id="BEDROOM_DOORWAY_GHOST", roomId=ChatHauntRoomId.Bedroom, anchor=Anchor("Ghost Bedroom Doorway",V(-2.7f,0,4.5f),90,bedroom.transform), animationState="GW_Peek_Right", visibleSeconds=4.2f },
                new GhostScareEvent.Placement { id="LIVING_ROOM_GHOST", roomId=ChatHauntRoomId.LivingRoom, anchor=Anchor("Ghost Living Doorway",V(.85f,0,1.25f),-90,living.transform), animationState="GW_Peek_Left", visibleSeconds=4f }
            };
            registry.ghostScare=ghost;
            GameObject hudObject=new GameObject("Controls And Interaction HUD");
            hudObject.AddComponent<ChatHauntHUD>().player=player;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            PlayerSettings.productName="ChatHaunt";
            AssetDatabase.SaveAssets();
            Validate();
            Debug.Log("CHAT_HAUNT_HOUSE_BUILT "+ScenePath);
        }

        [MenuItem("ChatHaunt/Validate Compact House")]
        public static void Validate()
        {
            if (SceneManager.GetActiveScene().path != ScenePath) throw new Exception("Wrong scene");
            ChatHauntHouseEvents registry=UnityEngine.Object.FindFirstObjectByType<ChatHauntHouseEvents>();
            if (!registry || registry.rooms.Length != 5) throw new Exception("Five rooms not registered");
            foreach (ChatHauntRoomId id in Enum.GetValues(typeof(ChatHauntRoomId)))
                if (!registry.FindRoom(id) || !registry.FindRoom(id).lighting) throw new Exception("Missing room light: "+id);
            if (!registry.hallwayBlackout || !UnityEngine.Object.FindFirstObjectByType<ChatHauntLocalBridge>()) throw new Exception("Local scare bridge missing");
            if (!registry.ghostScare || !registry.ghostScare.ghostPrefab || registry.ghostScare.placements.Length != 3) throw new Exception("Ghost scare missing");
            if (UnityEngine.Object.FindObjectsByType<ChatHauntSlidingDoor>(FindObjectsSortMode.None).Length != 2) throw new Exception("Sliding doors missing");
            Debug.Log("CHAT_HAUNT_HOUSE_VALIDATED");
        }

        [MenuItem("ChatHaunt/Test Hallway Ghost Scare (Play Mode)")]
        public static void TestHallwayGhost()
        {
            if (!Application.isPlaying) throw new Exception("Enter Play Mode before testing the ghost scare");
            ChatHauntHouseEvents events=UnityEngine.Object.FindFirstObjectByType<ChatHauntHouseEvents>();
            if (!events || !events.TriggerGhost("HALLWAY_GHOST"))
                throw new Exception("Hallway ghost scare is unavailable or already running");
        }

        static ChatHauntRoom Room(GameObject root, ChatHauntRoomId id, Vector3 lampPosition, Color color, ChatHauntSlidingDoor[] doors)
        {
            ChatHauntRoom room=root.AddComponent<ChatHauntRoom>(); room.id=id; room.doors=doors;
            GameObject lampObject=new GameObject(id+" Lamp"); lampObject.transform.SetParent(root.transform);
            lampObject.transform.position=lampPosition;
            Light lamp=lampObject.AddComponent<Light>(); lamp.type=LightType.Point;
            lamp.color=color; lamp.intensity=id==ChatHauntRoomId.Genkan?2.1f:3.1f; lamp.range=6.5f;
            lamp.shadows=LightShadows.None;
            ChatHauntLightRig lighting=root.AddComponent<ChatHauntLightRig>(); lighting.roomLights=new[]{lamp}; room.lighting=lighting;
            GameObject anchor=new GameObject(id+" Event Anchor"); anchor.transform.SetParent(root.transform);
            anchor.transform.position=V(lampPosition.x,0,lampPosition.z); room.eventAnchor=anchor.transform;
            Decor(id+" Lamp Housing", V(lampPosition.x,2.78f,lampPosition.z), V(.34f,.1f,.34f), trim, root.transform);
            return room;
        }

        static ChatHauntSlidingDoor Door(string name, Vector3 center, float width, float travel, Transform parent)
        {
            GameObject root=new GameObject(name); root.transform.SetParent(parent); root.transform.position=center;
            GameObject moving=new GameObject("Sliding Panel And Lattice"); moving.transform.SetParent(root.transform,false);
            Box(name+" Shoji Panel",V(0,1.15f,0),V(.11f,2.3f,width),doorWood,moving.transform,true);
            foreach(float side in new[]{-.065f,.065f})
            {
                for(int i=0;i<4;i++)
                    Decor(name+" Lattice Horizontal",V(side,.31f+i*.57f,0),V(.025f,.025f,width),trim,moving.transform,true);
                for(int i=0;i<4;i++)
                    Decor(name+" Lattice Vertical",V(side,1.15f,-width*.375f+i*width*.25f),V(.025f,2.25f,.025f),trim,moving.transform,true);
                Decor(name+" Handle",V(side*1.5f,1.08f,-width*.34f),V(.025f,.16f,.09f),trim,moving.transform,true);
            }
            Decor(name+" Top Rail",center+V(0,2.34f,0),V(.18f,.07f,width+.16f),trim,parent);
            Box(name+" Fixed Pull",V(-.03f,1.45f,width*.5f-.06f),V(.14f,1.4f,.08f),trim,root.transform,true);
            ChatHauntSlidingDoor door=root.AddComponent<ChatHauntSlidingDoor>();
            door.movingPart=moving.transform; door.openOffset=V(0,0,travel); door.travelSeconds=.65f;
            AudioSource audio=root.AddComponent<AudioSource>(); audio.spatialBlend=1f; audio.playOnAwake=false;
            door.audioSource=audio;
            return door;
        }

        static Material ReferenceMat(string relative)
        {
            string path=Root+"/Environment/Area51/Materials/JapaneseHouse/"+relative+".mat";
            Material result=AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!result) throw new Exception("Missing reference material: "+path);
            return result;
        }

        static Transform Anchor(string name, Vector3 position, float yaw, Transform parent)
        {
            GameObject anchor=new GameObject(name);
            anchor.transform.SetParent(parent);
            anchor.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
            return anchor.transform;
        }

        static void Wall(string name,float x,float z,float width,float length,Transform parent)
        { Box(name,V(x,1.4f,z),V(width,2.8f,length),wall,parent); }
        static void WallZ(string name,float x,float z,float length,Transform parent)
        { Box(name,V(x,1.4f,z),V(length,2.8f,.16f),wall,parent); }
        static void Lintel(string name,float x,float z,bool alongZ,float opening,Transform parent)
        { Box(name,V(x,2.55f,z),alongZ?V(.18f,.5f,opening):V(opening,.5f,.18f),trim,parent); }
        static Vector3 V(float x,float y,float z) { return new Vector3(x,y,z); }
        static Material Mat(string name,Color color)
        {
            string path=Root+"/Materials/"+name.Replace(' ','_')+".mat";
            Material result=AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result!=null) return result;
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            result=new Material(shader){name=name,color=color};
            AssetDatabase.CreateAsset(result,path); return result;
        }
        static GameObject Box(string name,Vector3 pos,Vector3 scale,Material material,Transform parent,bool local=false)
        {
            GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name;
            go.transform.SetParent(parent,false);
            if (local) go.transform.localPosition=pos; else go.transform.position=pos;
            go.transform.localScale=scale; go.GetComponent<Renderer>().sharedMaterial=material;
            return go;
        }
        static GameObject Decor(string name,Vector3 pos,Vector3 scale,Material material,Transform parent,bool local=false)
        {
            GameObject go=Box(name,pos,scale,material,parent,local);
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
    }
}
