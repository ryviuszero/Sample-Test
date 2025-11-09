using Unity.Mathematics;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public int noise_seed;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, noise_seed);

        MapDisplay display = GetComponent<MapDisplay>();
        Debug.Log(display);
        display.DrawNoiseMap(noiseMap);
    }


}
