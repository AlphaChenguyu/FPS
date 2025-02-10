using UnityEngine;
using Unity.Mathematics;
/*
 * Unity Noise-based Terrain Generator
 * 
 * Based on original code by Brackeys (https://www.youtube.com/watch?v=vFvwyu_ZKfU)
 */
public class Generator : MonoBehaviour
{
    public Terrain terrain;

    public MapType mapType = MapType.Flat;

    [Range(1, 10000)]
    public int randomSeed = 10; // Seed for RNG

    [Header("Terrain Size")]
    public int width = 256;
    public int depth = 256;
    [Range(0, 100)]
    public int height = 20;

    public enum MapType
    {
        Flat, Slope, Random, Perlin, Simplex, PerlinOctave
    };

    [Header("Perlin Noise")]
    [Range(0f, 100f)]
    public float frequency = 20f;
    [Range(0f, 10000f)]
    public float offsetX = 100f;
    [Range(0f, 10000f)]
    public float offsetY = 100f;

    public bool animateOffset = false;

    [Header("Octaves")]
    [Range(1, 8)]
    public int octaves = 3;
    [Range(0, 4)]
    public float amplitudeModifier;
    [Range(0, 4)]
    public float frequencyModifier;

    public void Start()
    {
        // Get a reference to the terrain component
        terrain = GetComponent<Terrain>();
    }

    // Update is called every frame
    public void Update()
    {
        // Generate the terrain according to current parameters
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        // Move along the X axis
        if (animateOffset)
            offsetX += Time.deltaTime * 5f;
    }

    // Update the terrain height values
    public TerrainData GenerateTerrain(TerrainData data)
    {
        // Set size and resolution for the terrain data
        data.heightmapResolution = width + 1;
        data.size = new Vector3(width, height, depth);

        float[,] heightMap;


        // Generate a height map
        switch (mapType)
        {
            case (MapType.Slope):
                heightMap = SlopingMap();
                break;
            case (MapType.Random):
                heightMap = RandomMap();
                break;
            case (MapType.Perlin):
                heightMap = NoiseMap(false);
                break;
            case (MapType.Simplex):
                heightMap = NoiseMap(true);
                break;
            case (MapType.PerlinOctave):
                heightMap = PerlinOctaveMap();
                break;
            default:
                heightMap = FlatMap();
                break;
        }

        // Set the terrain data to the new height map
        data.SetHeights(0, 0, heightMap);

        return data;
    }

    // Generate a flat height map (all zero)
    public float[,] FlatMap()
    {
        float[,] heights = new float[width, depth];

        return heights;
    }

    // Generate a sloping height map - you need to fix this!
    public float[,] SlopingMap()
    {
        float[,] heights = new float[width, depth];

        // Iterate over map positions
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < depth; y++)
            {
                // Set height at this position
                //heights[x, y] = 0;
                heights[x, y] = ((float)x/width + (float)y/depth) / 2;
            }
        }

        return heights;
    }
    public float[,] RandomMap()
    {
        float[,] heights = new float[width, depth];

        System.Random rng =  new System.Random(randomSeed);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < depth; y++)
            {
                heights[x, y] = (float)rng.NextDouble();
            }
        }

        return heights;
    }

    public float[,] NoiseMap(bool useSimplex)
    {
        float[,] heights = new float[width, depth];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < depth; y++)
            {
                float sampleX = frequency * (float)x / width + offsetX;
                float sampleY = frequency * (float)y / width + offsetY;

                float noiseValue;
                if (useSimplex)
                {
                    noiseValue = noise.snoise(new float2(sampleX, sampleY));
                    noiseValue = (noiseValue + 1) / 2;
                }
                else
                {
                    noiseValue = Mathf.PerlinNoise(sampleX, sampleY);
                }

                heights[x, y] = noiseValue;
            }
        }

        return heights;
    }
    public float[,] PerlinOctaveMap()
    {
        float[,] heights = new float[width, depth];
        System.Random rng = new System.Random(randomSeed); 

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < depth; y++)
            {
                float amplitude = 1f; // 初始振幅
                float frequency = this.frequency; // 初始频率
                float noiseHeight = 0f; // 计算该点的总噪声高度

                for (int i = 0; i < octaves; i++) // 遍历所有 Octaves
                {
                    // 让每个 octave 使用不同的偏移量，避免相干噪声
                    float octaveOffsetX = offsetX + rng.Next(-10000, 10000) * 0.0001f;
                    float octaveOffsetY = offsetY + rng.Next(-10000, 10000) * 0.0001f;

                    // **修正采样方式**，防止步长过大导致"三角锥"
                    float sampleX = (x + octaveOffsetX) * frequency / width;
                    float sampleY = (y + octaveOffsetY) * frequency / depth;

                    // 计算 Perlin Noise 值
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                    // 叠加噪声层
                    noiseHeight += perlinValue * amplitude;

                    // 频率增加，高频层提供更多细节
                    frequency *= frequencyModifier;

                    // 振幅减少，避免高频噪声过强
                    amplitude *= amplitudeModifier;
                }

                // 记录最小 & 最大高度，后续用于归一化
                if (noiseHeight > maxHeight) maxHeight = noiseHeight;
                if (noiseHeight < minHeight) minHeight = noiseHeight;

                // 存储未归一化的高度值
                heights[x, y] = noiseHeight;
            }
        }
        // **修正归一化，使高度值平滑**
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < depth; y++)
            {
                heights[x, y] = Mathf.InverseLerp(minHeight, maxHeight, heights[x, y]);
            }
        }
        return heights;
    }

}
