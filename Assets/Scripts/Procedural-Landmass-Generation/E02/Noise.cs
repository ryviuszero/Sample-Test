using UnityEngine;

public class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int seed = 0)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float sampleX = x / scale;
                float sampleY = y / scale;

                float perlinValue = Mathf.PerlinNoise(sampleX + seed, sampleY + seed);
                noiseMap[x, y] = perlinValue;
            }
        }

        return noiseMap;
    }
}
