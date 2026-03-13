using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEditor;
using System;
using UnityEditor.Tilemaps;
using UnityEditor.Search;


[System.Serializable]
public class LayerSetting
{
    public string name;
    public float DepthScale;
    public List<TileSetting> uniqueSprites = new List<TileSetting>();
    public float RefSize;
    public GameObject Frame;
}

public class DropdownWithConstrainOtherParamAttribute : PropertyAttribute
{
    public string optionFieldName;
    public DropdownWithConstrainOtherParamAttribute(string optionFieldName)
    {
        this.optionFieldName = optionFieldName;
    }
}

[CustomPropertyDrawer(typeof(DropdownWithConstrainOtherParamAttribute))]
class DropDownDrownerWithConstrains: PropertyDrawer
{
     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var casted = (DropdownWithConstrainOtherParamAttribute)attribute;
        string[] path =  property.propertyPath.Split('.');
        object tmp_obj = property.serializedObject.targetObject;
        for(int i = 0;i < path.Length-1;i++)
        {
            tmp_obj = tmp_obj.GetType().GetField(path[i]).GetValue(tmp_obj);
        }

        System.Reflection.PropertyInfo prop = tmp_obj.GetType()
        .GetProperty(casted.optionFieldName, 
        System.Reflection.BindingFlags.Public | 
        System.Reflection.BindingFlags.Instance | 
        System.Reflection.BindingFlags.Static | 
        System.Reflection.BindingFlags.NonPublic
        );

        if(prop.GetValue(tmp_obj) is string[] str_options) {
            int index = EditorGUI.Popup(position, label.text, System.Array.IndexOf(str_options, property.stringValue), str_options);
            property.stringValue = str_options[index];
        } else if(prop.GetValue(tmp_obj) is (string, int)[] tuple_options) {
            int index = EditorGUI.IntPopup(position, label.text, property.intValue, 
                tuple_options.Select(t=>t.Item1).ToArray(),
                tuple_options.Select(t=>t.Item2).ToArray()
            );
            property.intValue = tuple_options[index].Item2;
        }  else if(prop.GetValue(tmp_obj) is int[] int_options)
        {
            int index = EditorGUI.Popup(position, label.text, System.Array.IndexOf(int_options, property.intValue), int_options.Select(i=>i.ToString()).ToArray());
            property.intValue = int_options[index];
        }
    }
}

[System.Serializable]
public class TileSetting
{
    public float GenreValue;
    public Sprite useSprite;
    public UnityEngine.Vector2 getTransformPotition(UnityEngine.Vector2 leftBottomPosition, SpriteRenderer renderer)
    {
        return renderer.gameObject.transform.position + ((UnityEngine.Vector3)leftBottomPosition - renderer.bounds.min);
    }
}

[System.Serializable]
public class StageSettings
{
    public List<LayerSetting> layers = new List<LayerSetting>()
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
    public string GetLayerNameByIndex(int index) => layers[index].name;
    public (string, int)[] layerOptions => layers.Select((obj,i) => (tag: obj.name, val: i)).ToArray();
    [DropdownWithConstrainOtherParam("layerOptions")]
    public int playerLayerIndex;
    [DropdownWithConstrainOtherParam("layerOptions")]
    public int carLayerIndex;
    public LayerSetting getLayerSettingByName(string name)
    {
        return this.layers.Find(layerSetting => layerSetting.name == name);
    }
    public LayerSetting this[int index]
    {
        get {
            return this.layers[index];
        }
    }
    public LayerSetting this[string key]
    {
        get {
            return this.layers.Find(i=>i.name==key);
        }
    }
    public Span stageCoordinateHolizonalRange;
    public List<Sprite> carSprites = new List<Sprite>();
    public GameObject CarFrame;
}

/*[System.Serializable]
public class UseObjects
{
    public List<GameObject> farbackgrounds = new List<GameObject>();
    public List<GameObject> backgrounds = new List<GameObject>();
    public List<GameObject> objects = new List<GameObject>();
    public List<GameObject> cars = new List<GameObject>();
}*/

public class Util
{
    public static bool Between(float target, float start, float end)
    {
        return target > start && target < end;
    }
    public static bool ContainInRange(float target, float start, float length)
    {
        return target - start > 0 && target - start < length;
    }
    /*エイリアス法*/
    public static Func<int> GetRandFuncFollowDiscreteDistribution(float[] distribution)
    {
        Queue<float> dist = new Queue<float>(distribution);
        float min = distribution.Min();
        float sum = distribution.Sum();
        
        float unit = min-(float)(sum%min/(Math.Ceiling(sum/min)-1));

        List<(float,int)> dict = new List<(float, int)>();
        float[] tmp = new float[]{ dist.Dequeue(), 0 };
        int index = 0;
        while(tmp[0] > 0)
        {
            
            tmp[0] -= unit;
            dict.Add(
            (
               tmp[0] > 0 ? 1 : 1+tmp[0]/unit, index//, -1
            ));

            if(tmp[0] <= 0)
            {
                float use = dist.Count > 0 ? dist.Dequeue() : float.NegativeInfinity;
                use += tmp[0];
                tmp[0] = use; 
                index ++;
                //dict[dict.Count-1].Item3 = index;
            }
        }
        //for(int i = 0; i < Math.Ceiling(sum/min);i++)
        //{
        //    tmp[0] -= min;
        //    if(tmp[0] > 0)
        //    {
        //        
        //    }
        //};
        
        System.Random rand = new System.Random();
        return () =>
        {
            (float, int) use = dict[rand.Next(0, dict.Count())];
            if(rand.NextDouble() < use.Item1)
            {
                return use.Item2;
            } return use.Item2+1;
        };
    }
}


[System.Serializable]
public class GameSettings
{
    public float baseSpeed;
    public float carSpeed;    
    public float carSpawnCondition = 1f;

}

public class MapGenerator: MonoBehaviour
{
    public StageSettings stageSettings;
    public GameSettings gameSettings;

    private SortedDictionary<string, List<GameObject>> layers = new SortedDictionary<string, List<GameObject>>()
    {
      {"far", new List<GameObject>(){}},
      {"close", new List<GameObject>(){}},
      {"stage", new List<GameObject>(){}},
      {"front", new List<GameObject>(){}}  
    };
    struct EachLayerProcessContext
    {
        public float LastObjectGenreValue;
        public GameObject lastObject;
    }
    private Dictionary<string, GameObject> eachLayerLastObject
     = new Dictionary<string, GameObject>();

    private Dictionary<string, List<GameObject>> functional_layer = new Dictionary<string, List<GameObject>>()
    {
        { "car", new List<GameObject>(){} },
        { "roof", new List<GameObject>(){} }
    };

    [System.NonSerialized]public bool player_is_under_roof = true;
    [System.NonSerialized]public bool player_is_moving = true;

    public int playerLayer;
    void Start()
    {
        IntergrationManager.instance.mapGenerator = this;
    }

    private float @nowPlayerPosition;
    private PlayerControl.PlayerState @playerState;
    void Update()
    {
        /*Using Other Manager Values*/
        @nowPlayerPosition = IntergrationManager.instance.playerControl.playerPosition;
        @playerState = IntergrationManager.instance.playerControl.playerState;
        /**/

        foreach(var (key, layer) in this.layers)
        {
            layer.RemoveAll(i => i == null);
        }
        foreach(var (key, layer) in this.functional_layer)
        {
            layer.RemoveAll(i => i == null);
        }
        
        CarProcess();
        if(player_is_under_roof)
        {
            CarGenerator();
        }
        if(player_is_moving) {
            MoveBackgrounds();
        }
        BackgroundGenerator();
        CarGenerator();
    }

    public float toDeltaVector(float t) 
    {
        return t * Time.deltaTime;
    }
    public UnityEngine.Vector2 toDeltaVector(UnityEngine.Vector2 t) 
    {
        return t * Time.deltaTime;
    }
    public UnityEngine.Vector3 toDeltaVector(UnityEngine.Vector3 t) 
    {
        return t * Time.deltaTime;
    }

    public void MoveBackgrounds()
    {
        foreach(var (layer, layerSetting) in layers.Values.Zip(stageSettings.layers, (a,b)=>(a,b)))
        {
            float tmp_x_pos = float.NegativeInfinity;
            GameObject tmp_last_obj = null;
            foreach(GameObject obj in layer)
            {
                var pos = obj.transform.localPosition;
                pos.x -= this.toDeltaVector(gameSettings.baseSpeed/layerSetting.DepthScale);
                obj.transform.localPosition = pos;

                Bounds area = GetComponent<SpriteRenderer>().bounds;
                if(area.max.x > tmp_x_pos) {
                    tmp_x_pos = area.max.x;
                    tmp_last_obj = obj;
                }
                if(area.min.x < this.stageSettings.stageCoordinateHolizonalRange.start)
                {
                    Destroy(obj);
                }
            }
            eachLayerLastObject[layerSetting.name] = tmp_last_obj;
        }
    }
    float first_end_of_roof = float.NegativeInfinity;
    public void roofProcess()
    {
        string targetLayer = stageSettings.GetLayerNameByIndex(
            stageSettings.playerLayerIndex
        );
        bool tmp_roof_proof = false;
        float tmp_first_end_of_roof = float.NegativeInfinity;
        foreach(GameObject obj in layers[targetLayer])
        {
            if(obj.GetComponent<RoofSetting>())
            {
                MapObjectSettings settings = obj.GetComponent<MapObjectSettings>();
                
                Span span = settings.GetSpan();
                if(Util.Between(@nowPlayerPosition, span.start, span.end)) {
                    tmp_roof_proof = true;
                    tmp_first_end_of_roof = span.end;
                }
            }
        }
        player_is_under_roof = tmp_roof_proof;
        first_end_of_roof = tmp_first_end_of_roof;
    }
    public void CarProcess()
    {
        foreach(GameObject obj in functional_layer["car"])
        {
            MapObjectSettings settings = obj.GetComponent<MapObjectSettings>();

            Span span = settings.GetSpan();
            if(Util.Between(@nowPlayerPosition, span.start, span.end))
            {
                if(settings.layer > stageSettings.playerLayerIndex)
                {   
                    if(@playerState != PlayerControl.PlayerState.UMBRELLA_FRONT)
                    {
                        Debug.Log("GameEnd");
                    }
                }
                if(settings.layer < stageSettings.playerLayerIndex)
                {
                    if(@playerState != PlayerControl.PlayerState.UMBRELLA_BACK)
                    {
                        Debug.Log("GameEnd");
                    }
                }
            }
            float rel_velocity = obj.GetComponent<CarSetting>().speed;
            var pos = obj.transform.localPosition;
            pos.x -= this.toDeltaVector(rel_velocity/stageSettings[settings.layer].DepthScale);
            obj.transform.localPosition = pos;
        }
    }
    public void ApplyLayer(GameObject target, int layer)
    {
        target.GetComponent<SpriteRenderer>().sortingOrder = layer;
        this.layers[this.stageSettings.GetLayerNameByIndex(layer)]
        .Add(target);
    }
    public void CarGenerator()
    {
        if(first_end_of_roof - @nowPlayerPosition > gameSettings.carSpawnCondition && player_is_under_roof)
        {
            GameObject obj = Instantiate(stageSettings.CarFrame);
            obj.AddComponent<CarSetting>();
            obj.GetComponent<CarSetting>().speed = 1f;
            this.ApplyLayer(obj, this.stageSettings.carLayerIndex);
            obj.AddComponent<SpriteRenderer>();
            obj.GetComponent<SpriteRenderer>().sprite = stageSettings.carSprites[
                (int)(UnityEngine.Random.value*stageSettings.carSprites.Count)
            ];
        }
    }
    private float _spawnBuffer = 64f;
    public void BackgroundGenerator()
    {
        foreach(var (name, obj) in this.eachLayerLastObject)
        {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            TileSetting lastTs = obj.GetComponent<TileSetting>();
            /*Unityのスプライトはpivotの位置が基準になるらしい*/
            UnityEngine.Vector2 localPivotPos = renderer.sprite.pivot/renderer.sprite.pixelsPerUnit;
            float endx = renderer.bounds.max.x;
            if(endx - stageSettings.stageCoordinateHolizonalRange.end <= _spawnBuffer)
            {
                UnityEngine.Vector2 targetPos = new UnityEngine.Vector2(renderer.bounds.max.x, renderer.bounds.min.y);

                TileSetting[] useTiles = stageSettings[name].uniqueSprites.OrderBy(a => Math.Abs(a.GenreValue - lastTs.GenreValue)).ToArray()[0..5];
                //.Select(x => (Math.Abs(x.GenreValue - lastTs.GenreValue),x)).ToArray();
                float[] mass = useTiles.Select(x => Math.Abs(x.GenreValue - lastTs.GenreValue)).ToArray();
                float tmp_max = mass.Max();
                mass = mass.Select(x => tmp_max - x).ToArray();
                Func<int> rnd = Util.GetRandFuncFollowDiscreteDistribution(mass);
                TileSetting useTile = useTiles[rnd()];
                
                float spriteNaturalHeight = useTile.useSprite.bounds.max.y - useTile.useSprite.bounds.min.y;
                float scale = stageSettings[name].RefSize/spriteNaturalHeight;

                GameObject container = Instantiate(this.stageSettings[name].Frame);
                container.AddComponent<SpriteRenderer>();
                container.GetComponent<SpriteRenderer>().sprite = useTile.useSprite;
                container.transform.localScale *= scale;
                container.transform.position = useTile.getTransformPotition(targetPos, container.GetComponent<SpriteRenderer>());
            }
        }
    }
}