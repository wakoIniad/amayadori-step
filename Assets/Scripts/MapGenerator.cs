using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEditor;
using System;


[System.Serializable]
public class LayerSetting
{
    public string name;
    public float DepthScale;
    public List<GameObject> uniqueSprites = new List<GameObject>();
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
        var prop = property.serializedObject.targetObject.GetType()
        .GetProperty(casted.optionFieldName, 
        System.Reflection.BindingFlags.Public | 
        System.Reflection.BindingFlags.Instance | 
        System.Reflection.BindingFlags.Static | 
        System.Reflection.BindingFlags.NonPublic
        );

        if(prop.GetValue(property.serializedObject.targetObject) is string[] str_options) {
            int index = EditorGUI.Popup(position, label.text, System.Array.IndexOf(str_options, property.stringValue), str_options);
            property.stringValue = str_options[index];
        } else if(prop.GetValue(property.serializedObject.targetObject) is (string, int)[] tuple_options) {
            int index = EditorGUI.IntPopup(position, label.text, property.intValue, 
                tuple_options.Select(t=>t.Item1).ToArray(),
                tuple_options.Select(t=>t.Item2).ToArray()
            );
            property.intValue = tuple_options[index].Item2;
        }  else if(prop.GetValue(property.serializedObject.targetObject) is int[] int_options)
        {
            int index = EditorGUI.Popup(position, label.text, System.Array.IndexOf(int_options, property.intValue), int_options.Select(i=>i.ToString()).ToArray());
            property.intValue = int_options[index];
        }
    }
}

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
    private (string, int)[] playerLayerOptions => layers.Select((obj,i) => (tag: obj.name, val: i)).ToArray();
    [DropdownWithConstrainOtherParam("playerLayerOptions")]
    public int playerLayerIndex;
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
    public List<GameObject> carSprites = new List<GameObject>();
    public RangeInt stageCoordinateHolizonalRange;
}

[System.Serializable]
public class UseObjects
{
    public List<GameObject> farbackgrounds = new List<GameObject>();
    public List<GameObject> backgrounds = new List<GameObject>();
    public List<GameObject> objects = new List<GameObject>();
    public List<GameObject> cars = new List<GameObject>();
}

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

    private Dictionary<string, List<GameObject>> functional_layer = new Dictionary<string, List<GameObject>>()
    {
        { "car", new List<GameObject>(){} },
        { "roof", new List<GameObject>(){} }
    };

    public bool player_is_under_roof = true;
    public bool player_is_moving = true;

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
    }

    public void MoveBackgrounds()
    {
        foreach(var (layer, layerSetting) in layers.Values.Zip(stageSettings.layers, (a,b)=>(a,b)))
        {
            foreach(GameObject obj in layer)
            {
                var pos = obj.transform.localPosition;
                pos.x -= gameSettings.baseSpeed/layerSetting.DepthScale;
                obj.transform.localPosition = pos;

                Bounds area = GetComponent<SpriteRenderer>().bounds;
                if(area.min.x < this.stageSettings.stageCoordinateHolizonalRange.start)
                {
                    Destroy(obj);
                }
            }
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
                if(
                    Util.ContainInRange(
                        @nowPlayerPosition,
                        settings.range.start,
                        settings.range.length
                    )
                )
                {
                    tmp_roof_proof = true;
                    tmp_first_end_of_roof = settings.range.start + settings.range.length;
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

            if(
                Util.ContainInRange(
                    @nowPlayerPosition,
                    settings.range.start,
                    settings.range.length
                )
            )
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
            pos.x -= rel_velocity/stageSettings[settings.layer].DepthScale;
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
            GameObject obj = Instantiate(stageSettings.carSprites[0]);
            obj.AddComponent<CarSetting>();
            obj.GetComponent<CarSetting>().speed = 1f;
            this.ApplyLayer(obj, 3);
        }
    }
}