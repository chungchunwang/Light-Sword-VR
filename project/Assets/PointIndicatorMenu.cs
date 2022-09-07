using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointIndicatorMenu : MonoBehaviour
{
    [SerializeField] PointIndicatorManager[] col0 = new PointIndicatorManager[3];
    [SerializeField] PointIndicatorManager[] col1 = new PointIndicatorManager[3];
    [SerializeField] PointIndicatorManager[] col2 = new PointIndicatorManager[3];
    [SerializeField] PointIndicatorManager[] col3 = new PointIndicatorManager[3];
    PointIndicatorManager[][] grid = new PointIndicatorManager[4][];
    private void Start()
    {
        grid[0] = col0;
        grid[1] = col1;
        grid[2] = col2;
        grid[3] = col3;
    }
    public void spawnInColumn(int points, int col)
    {
        StartCoroutine(spawnInColumnRoutine(points, col));
        
    }
    IEnumerator spawnInColumnRoutine(int points, int col)
    {
        bool spawned = false;
        while (!spawned)
        {
            foreach (PointIndicatorManager m in grid[col])
            {
                if (m.active == false)
                {
                    m.display(points);
                    spawned = true;
                    break;
                }
            }
            yield return null;
        }
    }
}
