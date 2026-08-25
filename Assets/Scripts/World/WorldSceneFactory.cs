using NinetyMinutes.Art;
using UnityEngine;

namespace NinetyMinutes.World
{
    public static class WorldSceneFactory
    {
        public const string PersistentScene = "World_Persistent";
        public const string LockerScene = "Loc_Locker";
        public const string StreetScene = "Loc_Street";

        public const float LockerW = 14f;
        public const float LockerD = 11f;
        public const float StreetW = 18f;
        public const float StreetD = 16f;
        public const float WallH = 3.4f;

        public static GameObject BuildPersistent()
        {
            var root = new GameObject("World_Persistent");

            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(root.transform, false);
            sunGo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.88f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;

            var camGo = new GameObject("WorldCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(root.transform, false);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = false;
            cam.fieldOfView = 55f;
            cam.nearClipPlane = 0.12f;
            cam.farClipPlane = 80f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.42f, 0.48f, 0.54f);
            cam.depth = 10;
            cam.allowMSAA = true;
            camGo.AddComponent<AudioListener>();
            var rig = camGo.AddComponent<WorldCameraRig>();

            var player = WorldSprites.PlayerVisual("Player", ArtCatalog.SpritePlayer, root.transform);
            player.transform.position = new Vector3(0f, 0f, -2.2f);
            var pc = player.AddComponent<PlayerController>();
            EnsurePlayerPhysics(player);
            pc.CameraRig = rig;
            rig.Target = player.transform;
            return root;
        }

        public static GameObject BuildLocker()
        {
            var root = new GameObject("loc_locker");
            var wood = new Color(0.46f, 0.38f, 0.3f);
            var plaster = new Color(0.72f, 0.73f, 0.71f);
            var ceiling = new Color(0.82f, 0.82f, 0.8f);
            var metal = new Color(0.22f, 0.24f, 0.26f);

            WorldSprites.Floor("Floor", LockerW, LockerD, root.transform, wood);
            BuildRoomShell(root.transform, LockerW, LockerD, plaster, ceiling);

            var backArt = WorldSprites.Backdrop("LockerArt", ArtCatalog.Tex(ArtCatalog.LocationLocker), LockerW - 0.6f, 2.6f, root.transform);
            backArt.transform.localPosition = new Vector3(0f, 1.55f, LockerD * 0.5f - 0.22f);

            for (var i = 0; i < 6; i++)
            {
                var x = -5f + i * 1.7f;
                var locker = WorldSprites.Box("LockerUnit", new Vector3(1.1f, 2.2f, 0.55f), metal, root.transform);
                locker.transform.localPosition = new Vector3(x, 1.1f, LockerD * 0.5f - 0.55f);
                var trim = WorldSprites.Box("Trim", new Vector3(1.12f, 0.08f, 0.58f), WorldSprites.KitYellow, root.transform, false);
                trim.transform.localPosition = new Vector3(x, 2.05f, LockerD * 0.5f - 0.55f);
            }

            var bench = WorldSprites.Box("Bench", new Vector3(6.2f, 0.42f, 0.7f), new Color(0.4f, 0.28f, 0.18f), root.transform);
            bench.transform.localPosition = new Vector3(0f, 0.21f, 0.4f);
            var legL = WorldSprites.Box("BenchLegL", new Vector3(0.12f, 0.4f, 0.6f), metal, root.transform, false);
            legL.transform.localPosition = new Vector3(-2.8f, 0.2f, 0.4f);
            var legR = WorldSprites.Box("BenchLegR", new Vector3(0.12f, 0.4f, 0.6f), metal, root.transform, false);
            legR.transform.localPosition = new Vector3(2.8f, 0.2f, 0.4f);

            var lampGo = new GameObject("LockerLamp");
            lampGo.transform.SetParent(root.transform, false);
            lampGo.transform.position = new Vector3(0f, 2.8f, 0f);
            var lamp = lampGo.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = new Color(1f, 0.9f, 0.72f);
            lamp.intensity = 1.35f;
            lamp.range = 16f;
            lamp.shadows = LightShadows.Soft;

            var coach = WorldSprites.Pawn("npc_coach", ArtCatalog.PortraitCoach, new Color(0.28f, 0.32f, 0.3f), root.transform);
            coach.transform.position = new Vector3(2.4f, 0f, 1.6f);
            var coachNpc = coach.AddComponent<NpcInteractable>();
            coachNpc.NpcId = "npc_coach";
            coachNpc.Prompt = "E — говорить с тренером";
            coachNpc.RequireFlagMissing = "training_done";
            coachNpc.DoneLine = "Тренировка уже позади.";

            var skip = WorldSprites.Box("skip_training", new Vector3(1.1f, 0.7f, 0.8f), new Color(0.42f, 0.28f, 0.22f), root.transform);
            skip.transform.position = new Vector3(-4.2f, 0.35f, -3.4f);
            var skipCol = skip.GetComponent<BoxCollider>();
            skipCol.isTrigger = true;
            skipCol.size = new Vector3(1.5f, 2.2f, 1.5f);
            var skipNpc = skip.AddComponent<NpcInteractable>();
            skipNpc.NpcId = "skip_training";
            skipNpc.Prompt = "E — пропустить тренировку";

            var door = BuildDoor(root.transform, "door_to_street", new Vector3(-LockerW * 0.5f + 0.12f, 1.15f, -1.2f), new Color(0.25f, 0.42f, 0.55f));
            var doorComp = door.AddComponent<DoorInteractable>();
            doorComp.Prompt = "E — на бровку";
            doorComp.TargetLocationId = "loc_street";
            doorComp.TargetSpawn = new Vector2(6.5f, 0f);

            var loc = root.AddComponent<LocationScene>();
            loc.LocationId = "loc_locker";
            loc.LocalLamp = lamp;
            loc.Door = doorComp;
            loc.Npcs = new[] { coachNpc, skipNpc };
            return root;
        }

        public static GameObject BuildStreet()
        {
            var root = new GameObject("loc_street");
            var grass = new Color(0.4f, 0.52f, 0.32f);
            var concrete = new Color(0.62f, 0.63f, 0.6f);
            var pitchTex = ArtCatalog.Tex(ArtCatalog.LocationPitch) ?? ArtCatalog.Tex(ArtCatalog.LocationStreet);
            WorldSprites.Floor("Pitch", StreetW, StreetD, root.transform, grass, pitchTex);

            var sideline = WorldSprites.Box("Sideline", new Vector3(StreetW, 0.05f, 3.2f), concrete, root.transform, false);
            sideline.transform.localPosition = new Vector3(0f, 0.04f, -StreetD * 0.5f + 1.6f);

            var backdrop = WorldSprites.Backdrop("StadiumArt", ArtCatalog.Tex(ArtCatalog.LocationStreet), 22f, 7.2f, root.transform);
            backdrop.transform.localPosition = new Vector3(0f, 3.4f, StreetD * 0.5f + 0.4f);

            for (var row = 0; row < 3; row++)
            {
                var stand = WorldSprites.Box("Bleacher", new Vector3(14f, 0.45f, 0.9f), new Color(0.55f, 0.56f, 0.54f), root.transform);
                stand.transform.localPosition = new Vector3(0f, 0.3f + row * 0.42f, StreetD * 0.5f - 1.4f - row * 0.7f);
            }

            for (var i = 0; i < 4; i++)
            {
                var x = -7f + i * 4.6f;
                var pole = WorldSprites.Cylinder("Floodlight", new Vector3(0.18f, 2.4f, 0.18f), new Color(0.35f, 0.36f, 0.38f), root.transform, false);
                pole.transform.localPosition = new Vector3(x, 2.4f, StreetD * 0.5f - 0.6f);
                var lamp = WorldSprites.Box("LampHead", new Vector3(0.7f, 0.25f, 0.5f), WorldSprites.KitYellow, root.transform, false);
                lamp.transform.localPosition = new Vector3(x, 4.7f, StreetD * 0.5f - 0.6f);
            }

            BuildInvisibleBounds(root.transform, StreetW, StreetD);

            var door = BuildDoor(root.transform, "door_to_locker", new Vector3(StreetW * 0.5f - 0.12f, 1.15f, 0f), new Color(0.38f, 0.32f, 0.22f));
            var doorComp = door.AddComponent<DoorInteractable>();
            doorComp.Prompt = "E — в раздевалку";
            doorComp.TargetLocationId = "loc_locker";
            doorComp.TargetSpawn = new Vector2(-5.2f, -1.2f);

            var glock = WorldSprites.Pawn("npc_glock", ArtCatalog.PortraitGlock, WorldSprites.KitDark, root.transform);
            glock.transform.position = new Vector3(-3.2f, 0f, -1.4f);
            var glockNpc = glock.AddComponent<NpcInteractable>();
            glockNpc.NpcId = "npc_glock";
            glockNpc.Prompt = "E — говорить с Глоком";
            glockNpc.RequireFlagMissing = "street_glock_done";
            glockNpc.DoneLine = "С Глоком уже поговорили.";

            var sokol = WorldSprites.Pawn("npc_sokol", ArtCatalog.PortraitSokol, WorldSprites.KitDark, root.transform);
            sokol.transform.position = new Vector3(2.2f, 0f, -0.6f);
            var sokolNpc = sokol.AddComponent<NpcInteractable>();
            sokolNpc.NpcId = "npc_sokol";
            sokolNpc.Prompt = "E — говорить с Соколом";
            sokolNpc.RequireFlagMissing = "street_sokol_done";
            sokolNpc.DoneLine = "С Соколом уже поговорили.";

            var self = WorldSprites.Pawn("self_thought", ArtCatalog.PortraitBardin, WorldSprites.KitDark, root.transform);
            self.transform.position = new Vector3(-0.4f, 0f, 1.8f);
            var selfNpc = self.AddComponent<NpcInteractable>();
            selfNpc.NpcId = "self_thought";
            selfNpc.Prompt = "E — остаться с собой";
            selfNpc.RequireFlagMissing = "street_self_done";
            selfNpc.DoneLine = "Этот разговор уже был.";

            var loc = root.AddComponent<LocationScene>();
            loc.LocationId = "loc_street";
            loc.Door = doorComp;
            loc.Npcs = new[] { glockNpc, sokolNpc, selfNpc };
            return root;
        }

        public static void EnsurePlayerPhysics(GameObject player)
        {
            var rb = player.GetComponent<Rigidbody>();
            if (rb == null) rb = player.AddComponent<Rigidbody>();
            rb.mass = 80f;
            rb.drag = 6f;
            rb.angularDrag = 8f;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var col = player.GetComponent<CapsuleCollider>();
            if (col == null) col = player.AddComponent<CapsuleCollider>();
            col.height = 1.8f;
            col.radius = 0.32f;
            col.center = new Vector3(0f, 0.9f, 0f);
        }

        static void BuildRoomShell(Transform parent, float w, float d, Color wall, Color ceiling)
        {
            var hw = w * 0.5f;
            var hd = d * 0.5f;
            var t = 0.28f;

            var n = WorldSprites.Box("WallN", new Vector3(w + t, WallH, t), wall, parent);
            n.transform.localPosition = new Vector3(0f, WallH * 0.5f, hd);
            var e = WorldSprites.Box("WallE", new Vector3(t, WallH, d), wall, parent);
            e.transform.localPosition = new Vector3(hw, WallH * 0.5f, 0f);

            const float doorZ = -1.2f;
            const float doorHalf = 0.75f;
            var northLen = hd - (doorZ + doorHalf);
            if (northLen > 0.2f)
            {
                var westN = WorldSprites.Box("WallW_N", new Vector3(t, WallH, northLen), wall, parent);
                westN.transform.localPosition = new Vector3(-hw, WallH * 0.5f, doorZ + doorHalf + northLen * 0.5f);
            }

            var southLen = (doorZ - doorHalf) - (-hd);
            if (southLen > 0.2f)
            {
                var westS = WorldSprites.Box("WallW_S", new Vector3(t, WallH, southLen), wall, parent);
                westS.transform.localPosition = new Vector3(-hw, WallH * 0.5f, -hd + southLen * 0.5f);
            }

            MakeInvisibleWall(parent, "DoorBlockW", new Vector3(-hw, 1.2f, doorZ), new Vector3(0.25f, 2.4f, doorHalf * 2f));
            MakeInvisibleWall(parent, "BoundS", new Vector3(0f, 1.2f, -hd - 0.2f), new Vector3(w + 1f, 2.4f, 0.4f));

            var beam = WorldSprites.Box("CeilingBeam", new Vector3(w, 0.12f, 0.35f), ceiling, parent, false);
            beam.transform.localPosition = new Vector3(0f, WallH, hd - 0.4f);
        }

        static void BuildInvisibleBounds(Transform parent, float w, float d)
        {
            var hw = w * 0.5f;
            var hd = d * 0.5f;
            MakeInvisibleWall(parent, "BoundN", new Vector3(0f, 1.2f, hd + 0.2f), new Vector3(w + 1f, 2.4f, 0.4f));
            MakeInvisibleWall(parent, "BoundS", new Vector3(0f, 1.2f, -hd - 0.2f), new Vector3(w + 1f, 2.4f, 0.4f));
            MakeInvisibleWall(parent, "BoundE", new Vector3(hw + 0.2f, 1.2f, 0f), new Vector3(0.4f, 2.4f, d + 1f));
            MakeInvisibleWall(parent, "BoundW", new Vector3(-hw - 0.2f, 1.2f, 0f), new Vector3(0.4f, 2.4f, d + 1f));
        }

        static void MakeInvisibleWall(Transform parent, string name, Vector3 pos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var col = go.AddComponent<BoxCollider>();
            col.size = size;
        }

        static GameObject BuildDoor(Transform parent, string name, Vector3 pos, Color color)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = pos;

            var frameL = WorldSprites.Box("FrameL", new Vector3(0.14f, 2.3f, 0.14f), color, root.transform, false);
            frameL.transform.localPosition = new Vector3(0f, 0f, -0.55f);
            var frameR = WorldSprites.Box("FrameR", new Vector3(0.14f, 2.3f, 0.14f), color, root.transform, false);
            frameR.transform.localPosition = new Vector3(0f, 0f, 0.55f);
            var lintel = WorldSprites.Box("Lintel", new Vector3(0.14f, 0.14f, 1.24f), WorldSprites.KitYellow, root.transform, false);
            lintel.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            var leaf = WorldSprites.Box("Leaf", new Vector3(0.08f, 2.1f, 0.95f), color * 1.15f, root.transform, false);
            leaf.transform.localPosition = new Vector3(0.05f, 0f, 0f);

            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.2f, 0f);
            trigger.size = new Vector3(1.2f, 2.4f, 1.6f);
            return root;
        }
    }
}
