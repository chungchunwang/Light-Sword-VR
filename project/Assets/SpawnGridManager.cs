using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

public class SpawnGridManager : MonoBehaviour
{
    [SerializeField] GameObject[] row0Spawners = new GameObject[4];
    [SerializeField] GameObject[] row1Spawners = new GameObject[4];
    [SerializeField] GameObject[] row2Spawners = new GameObject[4];
    GameObject[][] masterSpawnerGrid;

    [SerializeField] GameObject[] redBlocks;
    [SerializeField] GameObject[] blueBlocks;
    [SerializeField] GameObject bomb;

    [SerializeField] GameObject barrier;

    Transform blocksParent;

    // Start is called before the first frame update
    void Start()
    {
        masterSpawnerGrid = new GameObject[4][];
        masterSpawnerGrid[0] = row0Spawners;
        masterSpawnerGrid[1] = row1Spawners;
        masterSpawnerGrid[2] = row2Spawners;

        blocksParent = GameObject.FindGameObjectWithTag("Blocks").transform;
    }
    public GameObject spawnOnGridAndMoveForward(Note currSpawnNote, float speed, float lifespan)
    {
        int row = currSpawnNote._lineLayer;
        int column = currSpawnNote._lineIndex;
        MapSystem.NoteType type = (MapSystem.NoteType)currSpawnNote._type;
        MapSystem.CutDirection direction = (MapSystem.CutDirection)currSpawnNote._cutDirection;
        GameObject note;
        if (type == MapSystem.NoteType.LeftNote)
        {
            note = LeanPool.Spawn(redBlocks[(int)direction]);
        }
        else if (type == MapSystem.NoteType.RightNote)
        {
            note = LeanPool.Spawn(blueBlocks[(int)direction]);
        }
        else note = LeanPool.Spawn(bomb);
        note.GetComponent<Sliceable>().note = currSpawnNote;
        note.transform.position = masterSpawnerGrid[row][column].transform.position;
        note.transform.SetParent(blocksParent);
        note.GetComponent<Mover>().moveWithVelocity(new Vector3(0,0,-speed));
        note.SetActive(true);
        note.GetComponent<TimedDespawner>().startTimer(lifespan);
        return note;
    }
    public GameObject spawnObstacleOnGridAndMoveForward(float speed, float duration, int width, int column, MapSystem.ObstacleType obstacleType, float lifespan)
    {
        float actualLength = speed * duration;
        float actualWidth = .7f * width;
        float actualHeight;
        if (obstacleType == MapSystem.ObstacleType.Full) actualHeight = .7f*3;
        else actualHeight = .7f*1;

        GameObject cube = LeanPool.Spawn(barrier);
        cube.transform.localScale = new Vector3(actualWidth, actualHeight, actualLength);
        //cube position offset
        Vector3 offset = new Vector3(actualWidth/2, actualHeight/2, actualLength/2) - new Vector3(.35f,.35f,.35f);
        if (obstacleType == MapSystem.ObstacleType.Full) cube.transform.position = masterSpawnerGrid[0][column].transform.position + offset;
        else cube.transform.position = masterSpawnerGrid[2][column].transform.position + offset;
        cube.GetComponent<Mover>().moveWithVelocity(new Vector3(0, 0, -speed));
        cube.GetComponent<TimedDespawner>().startTimer(lifespan);
        return cube;
    }
    //Create Unity Cube Mesh
    public GameObject createCubeMesh(float width, float height, float length)
    {
        GameObject cubePrefab = new GameObject();
        MeshRenderer meshRenderer = cubePrefab.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter meshFilter = cubePrefab.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[8]
        {
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(0, height, 0),
            new Vector3(width, height, 0),
            new Vector3(0, 0, length),
            new Vector3(width, 0, length),
            new Vector3(0, height, length),
            new Vector3(width, height, length)
        };
        mesh.vertices = vertices;

        int[] tris = new int[36]
        {
            0, 2, 1,
            2, 3, 1,
            0, 4, 2,
            4, 6, 2,
            0,1,4,
            4,1,5,
            1,3,5,
            3,7,5,
            5,7,6,
            5,6,4,
            2,6,7,
            2,7,3,
        };
        mesh.triangles = tris;
        mesh.Optimize();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        return cubePrefab;
    }
}
