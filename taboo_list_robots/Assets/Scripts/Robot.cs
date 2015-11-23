using UnityEngine;

public class Robot : MonoBehaviour {

    public int ID = -1; //default id = -1, changed when instantiated, gleichzeitig auch Priorität
    public Vector2 GridPosition = new Vector2(0f, 0f);
    public int LastCellIndex = 0; // Der Index der Letzten Zelle, auf der der Roboter stand
    public int OwnMapIndex = 0; // Der Index der Zelle, auf der der Roboter grade steht
    public int ViewRadius = 4; // View Radius in Zellen
    public int agentState = 0; // 0:LOOP_DETECTION 1:LOOP_CONTROL 2:LOOP_CLOSING 3:LOOP_CLEANING 4:STANDBY 5:OFF
    public int CurrentDirection = 0;
    public int ClosedCells = 0;
    public bool AfterLoopBeginning = false;

    public bool IsInSight(Transform cellTransform)
    {
        // 2. Ist die Zelle durch eine Mauer/Sonstiges verdeckt?
        int layerMask = 1 << LayerMask.NameToLayer("Wall");
        RaycastHit2D hit = Physics2D.Linecast(GetComponent<Transform>().position, cellTransform.position, layerMask);

        if (hit)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool InViewField(Transform cellTransform)
    {
        // 1. Ist die Zelle in Reichweite des Roboters (view radius)
        if (Vector2.Distance(cellTransform.GetComponent<Cell>().GridPosition, GridPosition) <= ViewRadius)
        {            
            return true;
        }
        else
        {
            return false;
        }
    }
}
