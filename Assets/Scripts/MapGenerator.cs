using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;


[System.Serializable]
public class LayerSetting
{
    public string name;
    public float DepthScale;
    public List<GameObject> uniqueSprites = new List<GameObject>();
}

public class LayerManager
{
    List<LayerSetting> layers = new List<LayerSetting>()
    {
        new LayerSetting()
        {
            name = "far"
        },
        new LayerSetting()
        {
            name = "close"
        },
        new LayerSetting()
        {
            name = "stage"
        },
        new LayerSetting()
        {
            name = "front"
        }
    };
    public LayerSetting getLayerSettingByName(string name)
    {
        return this.layers.Find(layerSetting => layerSetting.name == name);
    }
}

//[System.Serializable]
//public class Layers: IEnumerable<LayerSetting>
//{
//    public List<LayerSetting> layers;
//    //public LayerSetting far;
//    //public LayerSetting close;
//    //public LayerSetting stage;
//    //public LayerSetting car;
//    /*public float this[string key] {
//        get() {
//            return Key 
//        }
//    }*/
//    public IEnumerator<LayerSetting> GetEnumerator()
//    {
//        yield return far;
//        yield return close;
//        yield return stage;
//        yield return car;
//    }
//    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
//}

[System.Serializable]
public class UseObjects
{
    public List<GameObject> farbackgrounds = new List<GameObject>();
    public List<GameObject> backgrounds = new List<GameObject>();
    public List<GameObject> objects = new List<GameObject>();
    public List<GameObject> cars = new List<GameObject>();
}
public class MapGenerator: MonoBehaviour
{
    public UseObjects useObjects;

    public Layers layerSettings;
    private float[] _depthScale;
    public float baseSpeed;
    public float carSpeed;

    private SortedDictionary<string, List<GameObject>> layers = new SortedDictionary<string, List<GameObject>>()
    {
      {"far", new List<GameObject>(){}},
      {"close", new List<GameObject>(){}},
      {"stage", new List<GameObject>(){}},
      {"front", new List<GameObject>(){}}  
    };
    private Dictionary<string, List<GameObject>> functional_layer = new Dictionary<string, List<GameObject>>()
    {
        {"car", new List<GameObject>(){} }
    };

    public bool player_is_under_roof = true;
    public bool player_is_moving = true;

    public RangeInt screenHolizonalRange;

    public int playerLayer;
    void Start()
    {
        IntergrationManager.instance.mapGenerator = this;
        //this._depthScale = layerSettings.ToArray();
    }

    void Update()
    {
        CarProcess();
        if(player_is_under_roof)
        {
            CarGenerator();
        }
        if(player_is_moving) {
            MoveBackgrounds();
        }
    }

    public void MoveBackgrounds()
    {
        foreach(var (layer, layerSetting) in layers.Values.Zip(layerSettings, (a,b)=>(a,b)))
        {
            foreach(GameObject obj in layer)
            {
                var pos = obj.transform.localPosition;
                pos.x -= baseSpeed/layerSetting.DepthScale;
                obj.transform.localPosition = pos;
            }
        }
    }

    public void CarProcess()
    {
        foreach(GameObject obj in functional_layer["car"])
        {   
            float? rel_velocity = obj.GetComponent<CarSetting>()?.speed;
            MapObjectSettings settings = obj.GetComponent<MapObjectSettings>();


            if(IntergrationManager.instance.playerControl.playerState == 
            PlayerControl.PlayerState.UMBRELLA_BACK)
            {
                
            }
            var pos = obj.transform.localPosition;
            pos.x -= rel_velocity/;
            obj.transform.localPosition = pos;
        }
    }
    public void CarGenerator()
    {
        
    }
}