﻿using System;
using System.Collections;
using System.Collections.Generic;
using Anarchy.Configuration;
using Optimization.Caching;
using RC;
using System.IO;
using System.Linq;
using Anarchy;
using Antis;
using UnityEngine;

internal partial class FengGameManagerMKII
{
    private Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
    private Dictionary<string, Dictionary<string, GameObject>> objects = new Dictionary<string, Dictionary<string, GameObject>>();
    private AssetBundle bundleCustomMap;
    private GameObject customMap;
    private string bundlePath = Path.Combine(Application.dataPath, @"CustomMap/");
    public float HeroGrav = 20f;
    public bool MCForceStats;
    public int MCGAS;
    public int MCBLA;
    public int MCSPD;
    public int MCACL;
    public float ImpactDeathSpeed;
    public bool ImpactDeathEnabled;


    [RPC]
    private void ForceStatsRPC(bool force, int gas, int bla, int spd, int acl)
    {
        MCForceStats = force;
        MCGAS = gas;
        MCBLA = bla;
        MCSPD = spd;
        MCACL = acl;

        var stats = IN_GAME_MAIN_CAMERA.MainHERO.Setup.myCostume.stat;
        stats.Acl = MCACL;
        stats.Bla = MCBLA;
        stats.Spd = MCSPD;
        stats.Gas = MCGAS;
    }
    [RPC]
    private void SetImpactDeathRPC(bool enabled, float speed, PhotonMessageInfo info)
    {
        ImpactDeathEnabled = enabled;
        ImpactDeathSpeed = speed;
    }
    [RPC]
    private void SetGravityRPC(float g, PhotonMessageInfo info)
    {
        HeroGrav = g;
    }

    [RPC]
    private void LightRPC(string option, string value, PhotonMessageInfo info)
    {
        switch (option)
        {
            case "intensity":
                CacheGameObject.Find("mainLight").GetComponent<Light>().intensity = float.Parse(value);
                break;
            case "color":
                CacheGameObject.Find("mainLight").GetComponent<Light>().color = value.HexToColor();
                break;
        }
    }
    [RPC]
    private void FogRPC(string option, string value, PhotonMessageInfo info)
    {
        switch (option)
        {
            case "start":
                RenderSettings.fogStartDistance = float.Parse(value);
                break;
            case "end":
                RenderSettings.fogEndDistance = float.Parse(value);
                break;
            case "density":
                RenderSettings.fogDensity = float.Parse(value);
                break;
            case "color":
                RenderSettings.fogColor = value.HexToColor();
                break;
            case "enabled":
                var result = Convert.ToBoolean(int.Parse(value));
                VideoSettings.MCFogOverride = result;
                RenderSettings.fog = result;
                break;
            case "mode":
                switch (value)
                {
                    case "ExponentialSquared":
                        RenderSettings.fogMode = FogMode.ExponentialSquared;
                        break;
                    case "Exponential":
                        RenderSettings.fogMode = FogMode.Exponential;
                        break;
                    case "Linear":
                        RenderSettings.fogMode = FogMode.Linear;
                        break;
                }
                break;
        }
    }

    [RPC]
    private void FlareColour(Vector3 pos, Quaternion rot, float r, float g, float b, PhotonMessageInfo info)
    {
        Color col = new Color(r, g, b);
        var obj = (GameObject)Instantiate(CacheResources.Load("FX/flareBullet1"), pos, rot);
        obj.GetComponent<ParticleSystem>().startColor = col;
        obj.GetComponent<FlareMovement>().dontShowHint();
    }

    [RPC]
    private void SpawnWagon(int horseId, bool refill, string fileName, PhotonMessageInfo info)
    {
        var h = PhotonView.Find(horseId);
        var vector3 = (-h.transform.forward) * 14;
        var vector4 = h.transform.position;
        vector4 += h.transform.right * 6.5f;
        vector4.y -= 0.1f;
        var obj1 = RCManager.a.Load(fileName) as GameObject;
        obj1.transform.localScale = new Vector3(2, 2, 2);
        var obj2 = Instantiate(obj1, vector3 + vector4,
            Quaternion.Euler(0, h.transform.rotation.eulerAngles.y + 180, 0)) as GameObject;
        obj2.transform.SetParent(h.transform, true);
        foreach (var comp in obj2.GetComponentsInChildren<Collider>())
        {
            comp.gameObject.layer = LayerMask.NameToLayer("Ground");
        }
        obj2.gameObject.layer = LayerMask.NameToLayer("Ground");
        GameObject go = obj2.transform.FindChild("SupplyWagon1_1").gameObject;
        WheelRotate wr = go.AddComponent<WheelRotate>();
        wr.FRWheelT = go.transform.FindChild("Wheel3/Mesh7");
        wr.FLWheelT = go.transform.FindChild("Wheel4/Mesh8");
        wr.BRWheelT = go.transform.FindChild("Wheel2/Mesh6");
        wr.BLWheelT = go.transform.FindChild("Wheel1/Mesh3");
        wr.Col = go.GetComponentInChildren<WheelCollider>();
        if (refill)
        {
            var v3 = obj2.transform.position;
            v3.y += 2;
            v3 += (obj2.transform.right * 6.5f);
            v3 -= (obj2.transform.forward * 5f);
            var obj3 = Resources.Load("aot_supply") as GameObject;
            obj3.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            var obj4 =
                Instantiate(obj3, v3, Quaternion.Euler(0, obj2.transform.eulerAngles.y + 180, 0)) as GameObject;
            obj4.transform.parent = obj2.transform;
            obj4.AddComponent<WagonAutoFill>();
        }
        h.GetComponent<Horse>().wag = obj2;
        h.GetComponent<Horse>().Wagon = true;
        obj2.AddComponent<Wagon>();
    }

    [RPC]
    private void DisconnectWagon(int horseId, Vector3 pos, Quaternion rot, PhotonMessageInfo info)
    {
        var h = PhotonView.Find(horseId).GetComponent<Horse>();
        h.wag.transform.position = pos;
        h.wag.transform.rotation = rot;
        h.wag.transform.parent = null;
        h.Wagon = false;
    }
    [RPC]
    private void ReconnectWagon(int horseID, PhotonMessageInfo info)
    {
        var h = PhotonView.Find(horseID);
        var wag = h.GetComponent<Horse>().wag;
        var vector3 = (-h.transform.forward) * 14;
        var vector4 = h.transform.position;
        vector4 += h.transform.right * 6.5f;
        vector4.y -= 0.1f;
        wag.transform.position = vector3 + vector4;
        wag.transform.rotation = Quaternion.Euler(0, h.transform.rotation.eulerAngles.y + 180, 0);
        wag.transform.parent = h.transform;
        h.GetComponent<Horse>().Wagon = true;
    }
    public void SPTitan(int type, float size, int health, float speed, int count, int chaseDistance, int attackWait, float posX, float posY, float posZ, bool lockAxis, string bodySkinLink = "", string eyeSkinLink = "", float animationSpeed = 1f)
    {
        var position = new Vector3(posX, posY, posZ);
        var rotation = new Quaternion(0f, 0f, 0f, 0f);
        for (var i = 0; i < count; i++)
        {
            TITAN t = SpawnTitan(100, position, rotation);
            t.ResetLevel(size);
            t.hasSetLevel = true;
            t.maxHealth = health;
            t.currentHealth = health;
            t.CustomAttackWait = attackWait;
            t.CustomSpeed = speed;
            t.rigidbody.freezeRotation = lockAxis;
            t.chaseDistance = chaseDistance;
            t.BasePV.RPC("AniSpeed", PhotonTargets.AllBuffered, animationSpeed);
            t.BasePV.RPC("loadskinRPC", PhotonTargets.AllBuffered, bodySkinLink, eyeSkinLink);
            t.SetAbnormalType((AbnormalType)type);
        }
    }
    [RPC]
    private void EMCustomMapRPC(string localPath, string prefabName, Vector3 position, Quaternion rotation, PhotonMessageInfo info = null)
    {
        if (info.Sender.IsMasterClient)
        {
            if (bundleCustomMap != null)bundleCustomMap.Unload(true);
            base.StartCoroutine(LoadCustomMap(localPath, prefabName, position, rotation));
        }
    }

    [RPC]
    private void LoadBundleRPC(string bundleName, PhotonMessageInfo info)
    {
        if (info.Sender.IsMasterClient)
        {
            StartCoroutine(LoadBundle(bundleName));
        }
    }

    [RPC]
    private void LoadObjectRPC(string bundleName, string objectName, PhotonMessageInfo info)
    {
        if (info.Sender.IsMasterClient)
        {
            StartCoroutine(LoadObject(bundleName, objectName));
        }
    }

    [RPC]
    private void SpawnObjectRPC(string bundleName, string objectName, Vector3 position, Quaternion rotation, PhotonMessageInfo info)
    {
        if (info.Sender.IsMasterClient)
        {
            StartCoroutine(SpawnObject(bundleName, objectName, position, rotation));
        }
    }

    [RPC]
    private void MoveObjectRPC(string name, Vector3 position, Quaternion rotation, PhotonMessageInfo info)
    {
        if (info.Sender.IsMasterClient)
        {
            StartCoroutine(MoveObject(name, position, rotation));
        }
    }

    [RPC]
    private void DestroyObjectsRPC(string[] names, PhotonMessageInfo info)
    {
        if (info.Sender.IsMasterClient)
        {
            StartCoroutine(DestroyObjects(names));
        }
    }
    [RPC]
    private IEnumerator LoadCustomMap(string localPath, string prefabName, Vector3 position, Quaternion rotation)
    {
        string path = String.Concat(bundlePath, $"{localPath}.unity3d");
        byte[] data = System.IO.File.ReadAllBytes(path);
        AssetBundleCreateRequest assets = AssetBundle.CreateFromMemory(data);
        yield return assets;
        bundleCustomMap = assets.assetBundle;
        customMap = ((GameObject)assets.assetBundle.Load(prefabName));
        base.StartCoroutine(SpawnCustomMap(position, rotation));
    }

    private IEnumerator LoadObject(string bundleName, string objectName)
    {
        while (!bundles.ContainsKey(bundleName))
        {
            yield return null;
        }
        if (bundles.ContainsKey(bundleName))
        {
            objects[bundleName].Add(objectName, bundles[bundleName].Load(objectName) as GameObject);
        }
    }
    private IEnumerator SpawnCustomMap(Vector3 position, Quaternion rotation)
    {
        while (customMap == null)
        {
            yield return null;
        }
        IN_GAME_MAIN_CAMERA.MainCamera.GetComponent<TiltShift>().enabled = true;
        Anarchy.UI.Chat.Add(string.Concat("Instantiating a Unity map: Position - ", position, "; Rotation - ", rotation));
        customMap = (GameObject)UnityEngine.Object.Instantiate(customMap, position, rotation);
        foreach (Terrain t in Terrain.activeTerrains)
        {
            if (Settings.TreeBillboardDistanceOverride) t.treeBillboardDistance = Settings.TreeBillboardDistance;
            if (Settings.BaseMapDistanceOverride) t.basemapDistance = Settings.BaseMapDistance;
            if (Settings.TreeDistanceOverride) t.treeDistance = Settings.TreeDistance;
            if (Settings.DetailObjectDistanceOverride) t.detailObjectDistance = Settings.DetailObjectDistance;
        }
        IN_GAME_MAIN_CAMERA.MainCamera.GetComponent<TiltShift>().enabled = VideoSettings.Blur.Value;
    }

    private IEnumerator SpawnObject(string bundleName, string objectName, Vector3 pos, Quaternion rot)
    {
        while (!objects[bundleName].ContainsKey(objectName))
        {
            yield return null;
        }
        Instantiate(objects[bundleName][objectName], pos, rot);
    }

    private IEnumerator LoadBundle(string bundle)
    {
        byte[] data = File.ReadAllBytes(bundlePath + bundle + ".unity3d");
        AssetBundleCreateRequest assets = AssetBundle.CreateFromMemory(data);
        yield return assets;
        if (assets != null)
        {
            bundles.Add(bundle, assets.assetBundle);
            objects.Add(bundle, new Dictionary<string, GameObject>());
        }
    }

    private IEnumerator MoveObject(string name, Vector3 position, Quaternion rotation)
    {
        GameObject obj = GameObject.Find(name);
        while (obj == null)
        {
            obj = GameObject.Find(name);
            yield return null;
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
    }

    private IEnumerator DestroyObjects(string[] names)
    {
        List<GameObject> objects = new List<GameObject>();
        while (objects.Count == 0)
        {
            foreach(var obj in FindObjectsOfType<GameObject>())
            {
                if (names.Contains(obj.name))
                {
                    objects.Add(obj);
                }
            }

            yield return null;
        }

        foreach (var obj in objects)
        {
            Destroy(obj);
        }
    }
    public IEnumerator ExecuteCommandList(string path)
    {
        var commandList = File.ReadAllLines(path);
        foreach (string line in commandList)
        {
            if (line.StartsWith("WAIT"))
            {
                yield return new WaitForSeconds(float.Parse(line.Substring(5)));
            }
            else
            {
                if (RC.RCManager.RCEvents.ContainsKey("OnChatInput"))
                {
                    string key = (string) RC.RCManager.RCVariableNames["OnChatInput"];
                    var collection = RC.RCManager.stringVariables;
                    if (collection.ContainsKey(key))
                    {
                        collection[key] = line;
                    }
                    else
                    {
                        collection.Add(key, line);
                    }

                    ((RCEvent) RC.RCManager.RCEvents["OnChatInput"]).checkEvent();
                }

                Anarchy.UI.Chat.CMDHandler.TryHandle(line);
            }
        }
    }
}