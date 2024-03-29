﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData {

    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    public static readonly int WorldSizeInChunks = 25;

    public static readonly int ViewDistanceInChunks = 5;

    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * ChunkWidth; }
    }

    public static readonly Vector3[] voxelVerts = new Vector3[8] {

        new Vector3(0.0f,0.0f,0.0f),

        new Vector3(1.0f,0.0f,0.0f),

        new Vector3(1.0f,1.0f,0.0f),

        new Vector3(0.0f,1.0f,0.0f),

        new Vector3(0.0f,0.0f,1.0f),

        new Vector3(1.0f,0.0f,1.0f),

        new Vector3(1.0f,1.0f,1.0f),

        new Vector3(0.0f,1.0f,1.0f),

    };

    public static readonly int TextureAtlasSizeInBlock = 4;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlock; }
    }

    public static readonly int[,] voxelTris = new int[6,4]
    {
        //0 1 2 2 1 3
        {0,3,1,2 },//back face
        {5,6,4,7 },//front
        { 3,7,2,6 }, // top face
        {1,5,0,4 },//bottom face
        { 4,7,0,3},//left face
        { 1,2,5,6}//right face
    };

    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0.0f,0.0f,-1.0f),

        new Vector3(0.0f,0.0f,1.0f),

        new Vector3(0.0f,1.0f,0.0f),

        new Vector3(0.0f,-1.0f,0.0f),

        new Vector3(-1.0f,0.0f,0.0f),

        new Vector3(1.0f,0.0f,0.0f),

    };


    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2(0.0f,0.0f),

        new Vector2(0.0f,1.0f),

        new Vector2(1.0f,0.0f),

        new Vector2(1.0f,1.0f),
    };
}
