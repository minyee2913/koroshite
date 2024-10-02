using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData {
    public string id;
    public string Name;
    public List<Vector2> spawnPos;
    public Vector2 center;
    public Vector2 mapSize;
    public List<GameMode> modes;

}
public class MapManager : MonoBehaviour
{
    public static MapManager Instance {get; private set;}
    public List<MapData> maps;
    public string selectedId;
    [HideInInspector]
    public MapData selectedMap;

    void Awake() {
        Instance = this;

        SelectById(selectedId);
    }

    public void SelectById(string id) {
        var map = Get(id);

        if (map != null) {
            Select(map);
        }
    }

    public void SelectByMod(GameMode mode) {
        var map_ = maps.FindAll((v)=>v.modes.Contains(mode));

        if (maps.Count > 0) {
            Select(map_[Mathf.FloorToInt(UnityEngine.Random.Range(0f, map_.Count))]);
        }
    }

    public void Select(MapData map) {
        selectedMap = map;
        selectedId = map.id;
    }

    public MapData Get(string id) {
        return maps.Find((v)=>v.id == id);
    }

    private void OnDrawGizmos() {
        var map = Get(selectedId);
        
        if (map != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(map.center, map.mapSize * 2);
        }
    }
}
