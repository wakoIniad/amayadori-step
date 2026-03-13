using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public class Span
{
    public float start;
    public float end;
    
    public Span(float start, float end)
    {
        this.start = start;
        this.end = end;
    }
}
class MapObjectSettings: MonoBehaviour
{
    public int layer;
    public RangeInt range;
    public Span GetSpan()
    {
        Bounds bound = this.gameObject.GetComponent<BoxCollider2D>().bounds;
        return new Span(bound.min.x, bound.max.x);
    }
    public float objectGenre;
}