using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
   public const int chunkSize = 16;
   public GameObject terrainChunkPrefab;

   public Transform player;

   public static Dictionary<ChunkPos, TerrainChunk> terrainChunks = new Dictionary<ChunkPos, TerrainChunk>();

   FastNoise noise = new FastNoise();

   int chunkDist = 5;

   List<TerrainChunk> pooledChunks = new List<TerrainChunk>();

   List<ChunkPos> toGenerate = new List<ChunkPos>();

   ChunkPos curChunk = new ChunkPos(-1, -1);

   void Start()
    {
        LoadChunks();
    }

    private void Update()
    {
        LoadChunks();
    }

    void LoadChunks(bool instant = false)
    {
        // the current chunk the player is in 
        int curChunkPosX = Mathf.FloorToInt(player.position.x / chunkSize) * 16;
        int curChunkPosZ = Mathf.FloorToInt(player.position.z / chunkSize) * 16;

        // entered a new chunk
        if(curChunk.x != curChunkPosX || curChunk.z != curChunkPosZ)
        {
            curChunk.z = curChunkPosZ;
            curChunk.x = curChunkPosX;

            for(int i = curChunkPosX - chunkSize * chunkDist; i <= curChunkPosX + chunkSize * chunkDist; i += chunkSize)
                for(int j = curChunkPosZ - chunkSize * chunkDist; j <= curChunkPosZ + chunkSize * chunkDist; j += chunkSize)
                {
                    ChunkPos pos = new ChunkPos(i, j);
                    if(!terrainChunks.ContainsKey(pos) && !toGenerate.Contains(pos))
                    {
                        if(instant)
                        {
                            BuildChunk(pos.x, pos.z);
                        }
                        else
                        {
                            toGenerate.Add(pos);
                        }
                        
                    }
                }
            
            // remove chunks that are too far away
            List<ChunkPos> toDestroy = new List<ChunkPos>();
            // unload chunks
            foreach(KeyValuePair<ChunkPos, TerrainChunk> c in terrainChunks)
            {
                ChunkPos cp = c.Key;
                if(Mathf.Abs(cp.x - curChunkPosX) > chunkSize * (chunkDist + 3) || Mathf.Abs(cp.z - curChunkPosZ) > chunkSize * (chunkDist + 3))
                {
                    toDestroy.Add(cp);
                }
            }
            
            foreach(ChunkPos cp in toDestroy)
            {
                terrainChunks[cp].gameObject.SetActive(false);
                pooledChunks.Add(terrainChunks[cp]);
                terrainChunks.Remove(cp);
            }

            StartCoroutine(DelayBuildChunks());

        }
    }

    IEnumerator DelayBuildChunks()
    {
        while(toGenerate.Count > 0)
        {
            BuildChunk(toGenerate[0].x, toGenerate[0].z);
            toGenerate.RemoveAt(0);
            yield return new WaitForSeconds(.2f);
        }
    }

    void BuildChunk(int xPos, int zPos)
    {
        TerrainChunk chunk;
        if (pooledChunks.Count > 0) // look in the pool first
        {
            chunk = pooledChunks[0];
            chunk.gameObject.SetActive(true);
            pooledChunks.RemoveAt(0);
            chunk.transform.position = new Vector3(xPos, 0, zPos);
        }
        else
        {
            GameObject chunkGO = Instantiate(terrainChunkPrefab, new Vector3(xPos, 0, zPos), Quaternion.identity);
            chunk = chunkGO.GetComponent<TerrainChunk>();
        }

          for(int x = 0; x < TerrainChunk.chunkWidth+2; x++)
            for(int z = 0; z < TerrainChunk.chunkWidth+2; z++)
                for(int y = 0; y < TerrainChunk.chunkHeight; y++)
                {
                    //if(Mathf.PerlinNoise((xPos + x-1) * .1f, (zPos + z-1) * .1f) * 10 + y < TerrainChunk.chunkHeight * .5f)
                    chunk.blocks[x, y, z] = GetBlockType(xPos+x-1, y, zPos+z-1);
                }

        // GenerateTrees(chunk.blocks, xPos, zPos);

        chunk.BuildMesh();

        WaterChunk wat = chunk.transform.GetComponentInChildren<WaterChunk>();
        wat.SetLocs(chunk.blocks);
        wat.BuildMesh();

        terrainChunks.Add(new ChunkPos(xPos, zPos), chunk);


    }

    //get the block type at a specific coordinate
    BlockType GetBlockType(int x, int y, int z)
    {
        /*if(y < 33)
            return BlockType.Dirt;
        else
            return BlockType.Air;*/


        //print(noise.GetSimplex(x, z));
        float simplex1 = noise.GetSimplex(x * 0.8f, z * 0.8f) * 10;
        float simplex2 = noise.GetSimplex(x * 3f, z* 3f) * 10 * (noise.GetSimplex(x*.3f, z*.3f)+.5f);

        float heightMap = simplex1 + simplex2;

        // add the 2d noise to the middle of the terrain chunk
        float baseLandHeight =  TerrainChunk.chunkHeight * .5f + heightMap;

        // 3d noise for caves and overhangs and such
        float caveNoise1 = noise.GetPerlinFractal(x*5f, y*10f, z*5f);
        float caveMask = noise.GetSimplex(x * .3f, z * .3f)+.3f;

        //stone layer heightmap
        float simplexStone1 = noise.GetSimplex(x * 1f, z * 1f) * 10;
        float simplexStone2 = (noise.GetSimplex(x * 5f, z * 5f)+.5f) * 20 * (noise.GetSimplex(x * .3f, z * .3f) + .5f);

        float stoneHeightMap = simplexStone1 + simplexStone2;
        float baseStoneHeight = TerrainChunk.chunkHeight * .25f + stoneHeightMap;


        //float cliffThing = noise.GetSimplex(x * 1f, z * 1f, y) * 10;
        //float cliffThingMask = noise.GetSimplex(x * .4f, z * .4f) + .3f;

        BlockType blockType = BlockType.Air;

        //under the surface, dirt block
        if (y < baseLandHeight)
        {
            blockType = BlockType.Dirt;

            // just on the surface, use a grass type
            if (y >= baseLandHeight - 1 && y > WaterChunk.waterHeight - 2)
            {
                blockType = BlockType.Grass;
            }

            if (y < baseStoneHeight)
            {
                blockType = BlockType.Stone;
            }
        }

        if (caveNoise1 > Mathf.Max(caveMask, 0.2f))
            blockType = BlockType.Air;
        

        /*if(blockType != BlockType.Air)
            blockType = BlockType.Stone;*/

        //if(blockType == BlockType.Air && noise.GetSimplex(x * 4f, y * 4f, z*4f) < 0)
          //  blockType = BlockType.Dirt;

        //if(Mathf.PerlinNoise(x * .1f, z * .1f) * 10 + y < TerrainChunk.chunkHeight * .5f)
        //    return BlockType.Grass;

        return blockType;
    }







}

public struct ChunkPos
{
    public int x, z;

    public ChunkPos(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
}
