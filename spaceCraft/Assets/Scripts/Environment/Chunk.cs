using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {

    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    public ChunkCoord coord;

    int vertexIndex = 0;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();

    World world;

    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];


    private bool _isActive;

    public bool IsVoxelMapPopulated = false;

    public Chunk(ChunkCoord _coord, World _world,bool GenerateOnLoad)
    {
        coord = _coord;
        world = _world;
        IsActive = true;

        if (GenerateOnLoad)
            Init();
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk-x-" + coord.x + "-y-" + coord.z;

        PopulateVoxelMap();
        UpdateChunk();
       
    }

    void PopulateVoxelMap()
    {
        for (var y = 0; y < VoxelData.ChunkHeight; y++) {
            for (var x = 0; x < VoxelData.ChunkWidth; x++){
                for (var z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + GetPosition());
                }
            }
        }
        IsVoxelMapPopulated = true;
    }


    void UpdateChunk()
    {
        ClearMeshData();

        for (var y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (var x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (var z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if(world.BlockTypes[voxelMap[x,y,z]].IsSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }
        CreateMesh();
    }

    public bool IsActive
    {
       get { return _isActive;  }
        set
        {
            _isActive = value;
            if(chunkObject != null)
            {
                chunkObject.SetActive(value);
            }
        }
    }

    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }


    public Vector3 GetPosition()
    {
        return chunkObject.transform.position;
    }


    bool IsVoxelInChunk(int x, int y, int z)
    {

        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;
        else
            return true;
    }

    public void EditVoxel(Vector3 Position, byte NewId)
    {
        int xCheck = Mathf.FloorToInt(Position.x);
        int yCheck = Mathf.FloorToInt(Position.y);
        int zCheck = Mathf.FloorToInt(Position.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck] = NewId;

        UpdateSurroundingVoxels(xCheck,yCheck,zCheck);


        UpdateChunk();
    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int j = 0; j < 6; j++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[j];
            
            if(!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.GetChunkFromVector3(thisVoxel + GetPosition()).UpdateChunk();
            }
        }
    }

    bool CheckVoxel(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
            return world.CheckForVoxel(pos + GetPosition());

        return world.BlockTypes[voxelMap[x, y, z]].IsSolid;

    }

    public byte GetVoxelFromGlobalVector3(Vector3 Position)
    {

        int xCheck = Mathf.FloorToInt(Position.x);
        int yCheck = Mathf.FloorToInt(Position.y);
        int zCheck = Mathf.FloorToInt(Position.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];

    }

    void AddTexture(int TextureId)
    {
        float y = TextureId / VoxelData.TextureAtlasSizeInBlock;
        float x = TextureId - (y * VoxelData.TextureAtlasSizeInBlock);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x,y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));

    }

    void UpdateMeshData(Vector3 Position)
    {
        for (int j = 0; j < 6; j++)
        {
            if (!CheckVoxel(Position + VoxelData.faceChecks[j]))
            {

                byte BlockId = voxelMap[(int)Position.x, (int)Position.y, (int)Position.z];

                vertices.Add(Position + VoxelData.voxelVerts[VoxelData.voxelTris[j, 0]]);
                vertices.Add(Position + VoxelData.voxelVerts[VoxelData.voxelTris[j, 1]]);
                vertices.Add(Position + VoxelData.voxelVerts[VoxelData.voxelTris[j, 2]]);
                vertices.Add(Position + VoxelData.voxelVerts[VoxelData.voxelTris[j, 3]]);

                AddTexture(world.BlockTypes[BlockId].GetTextureid(j));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2) ;
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}

public class ChunkCoord{

    public int x;
    public int z;

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(Vector3 pos)
    {

        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;

    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else return false;
    }
}
