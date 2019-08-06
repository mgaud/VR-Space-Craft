using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour {


    public int seed;
    public Transform Player;
    public Vector3 SpawnPosition;

    public BiomeAttribute Data;

    public Material material;
    public BlockType[] BlockTypes;


    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks,VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> ChunksToCreate = new List<ChunkCoord>();

    private bool IsCreatingChunks;

    public void Start()
    {
        Random.InitState(seed);

        SpawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight - 50f, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(Player.position);
    }

    public void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(Player.position);

        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (ChunksToCreate.Count() > 0 && !IsCreatingChunks)
            StartCoroutine("CreateChunks");
    }

    void GenerateWorld()
    {
        for (var x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < ((VoxelData.WorldSizeInChunks / 2) +VoxelData.ViewDistanceInChunks); x++)
        {
            for (var z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < ((VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks); z++)
            {
                //CreateNewChunk(x, z);
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }

        Player.position = SpawnPosition;
    }

    IEnumerator CreateChunks()
    {
        IsCreatingChunks = true;

        while(ChunksToCreate.Count() > 0)
        {
            chunks[ChunksToCreate[0].x, ChunksToCreate[0].z].Init();
            ChunksToCreate.RemoveAt(0);
            yield return null;
        }            

        IsCreatingChunks = false;
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(Player.transform.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for(var x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++ ){
            for (var z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        ChunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    //CreateNewChunk(x, z);
                    else if (!chunks[x, z].IsActive)
                    {
                        chunks[x, z].IsActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }
                }
                for (var i = 0; i < previouslyActiveChunks.Count(); i++)
                {
                    if(previouslyActiveChunks[i].Equals(new ChunkCoord(x,z)))
                    {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach(var chunkCoord in previouslyActiveChunks)
        {
            chunks[chunkCoord.x, chunkCoord.z].IsActive = false;
        }
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return new ChunkCoord(x, z);
    }

    public bool CheckForVoxel(Vector3 pos)
    {

        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsVoxelMapPopulated)
            return BlockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].IsSolid;

        return BlockTypes[GetVoxel(pos)].IsSolid;

    }


    public byte GetVoxel(Vector3 Position)
    {
        int yPosition = Mathf.FloorToInt(Position.y);

        if (!IsVoxelInWorld(Position))
            return 0; //Air block
        if (yPosition == 0)
            return 2; //Bedrock


        //Basic Terrain

        int terrainHeight = Mathf.FloorToInt(Data.TerrainHeight *  (Noise.Get2dPerlin(new Vector2(Position.x, Position.z), 0, Data.TerrainScale))) + Data.SolidGroundHeight;

        byte voxelValue = 0;

        if (yPosition == terrainHeight)
        {
            voxelValue = 3; //DirtGrass
        }
        else if(yPosition < terrainHeight && yPosition > terrainHeight - 4)
        {
            voxelValue = 4; //plain dirt
        }
        else if( yPosition < terrainHeight)
        {
            voxelValue = 1;  //stone block
        }
        else return 0;

        //Second Pass

        if (voxelValue == 1)
        {
            foreach(var lode in Data.lodes)
            {
                if(yPosition > lode.MinHeight && yPosition < lode.MaxHeight)
                {

                    if (Noise.Get3dPerlin(Position,lode.NoiseOffset,lode.Scale,lode.Threashold))
                    {
                        return voxelValue = lode.BlockId;
                    }
                }
            }
        }

         return voxelValue;


    }

    //void CreateNewChunk(int x, int z)
    //{
    //    chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
    //    activeChunks.Add(new ChunkCoord(x, z));
    //}

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x >= 0 && coord.x <= VoxelData.WorldSizeInChunks - 1 && coord.z >= 0 && coord.z <= VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return false;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {

        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;

    }
}

[System.Serializable]
public class BlockType
{
    public string BlockName;
    public bool IsSolid;

    [Header("Texture Values")]
    public int BackFaceTexture;
    public int FrontFaceTexture;
    public int TopFaceTexture;
    public int BottomFaceTexture;
    public int LeftFaceTexture;
    public int RightFaceTexture;



    public int GetTextureid(int FaceIndex)
    {
        //back, front top , bottom , left , right
        switch (FaceIndex)
        {
            case 0: return BackFaceTexture;
            case 1: return FrontFaceTexture;
            case 2: return TopFaceTexture;
            case 3: return BottomFaceTexture;
            case 4: return LeftFaceTexture;
            case 5: return RightFaceTexture;
            default:
                Debug.Log("Error");
                return BackFaceTexture;
        }
    }
}

