using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell : MonoBehaviour {

    public Vector2 GridPosition = new Vector2(0f, 0f);
    public int ownIndex;

    public Sprite UnexploredCell;
    public Sprite ExploredCell;
    public Sprite ClosedCell;
    public Sprite Wall;
    public Sprite RendezvousPoint;
    public Sprite LoopCell;

    public bool isExplored = false;
    public bool isUnexplored = false;
    public bool isWall = false;
    public bool isOpen = false;
    public bool isClosed = false;
    public bool isRendezvousPoint = false;
    public bool isLoopCell = false;

    public int dispersionGradient = 0;
    public int ControlledBy = -1; // -1 : kein Roboter kontrolliert diese Zelle

    private SpriteRenderer myRenderer;

    public class Stored
    {
        public int LastExitDirection = -1; // -1 : noch nie besucht, 0 : oben, 1 : recht, 2 : unten, 3 : links
        public int LastNrOfClosedCells = 0;
        public bool IsLoopCell = false;
    }
    public List<Stored> MarkedByRobot = new List<Stored>();


    public void toUnexploredCell()
    {
        myRenderer.sprite = UnexploredCell;
        isExplored = false;
        isUnexplored = true;
        isWall = false;
        isOpen = true;
        isClosed = false;
        isRendezvousPoint = false;
        isLoopCell = false;
        gameObject.layer = LayerMask.NameToLayer("Floor");
    }

    public void toExploredCell(int RobotID)
    {
        myRenderer.sprite = ExploredCell;
        isExplored = true;
        isUnexplored = false;
        isWall = false;
        isOpen = true;
        isClosed = false;
        isRendezvousPoint = false;
        isLoopCell = false;
        gameObject.layer = LayerMask.NameToLayer("Floor");
        MarkedByRobot[RobotID].IsLoopCell = false;
    }

    public void toLoopCell(int RobotID)
    {
        //myRenderer.sprite = LoopCell;
        myRenderer.sprite = ExploredCell;
        isExplored = false;
        isUnexplored = false;
        isWall = false;
        isOpen = true;
        isClosed = false;
        isRendezvousPoint = false;
        isLoopCell = true;
        gameObject.layer = LayerMask.NameToLayer("Floor");
        MarkedByRobot[RobotID].IsLoopCell = true;
    }

    public void toClosedCell()
    {
        myRenderer.sprite = ClosedCell;
        isExplored = false;
        isUnexplored = false;
        isWall = false;
        isOpen = false;
        isClosed = true;
        isRendezvousPoint = false;
        isLoopCell = false;
        gameObject.layer = LayerMask.NameToLayer("Closed Floor");
    }

    // Markiert die Zelle als geschlossen, ohne dass das Sprite geändert wird. 
    public void fakeClosedCell()
    {
        isExplored = false;
        isUnexplored = false;
        isWall = false;
        isOpen = false;
        isClosed = true;
        isRendezvousPoint = false;
        isLoopCell = false;
        gameObject.layer = LayerMask.NameToLayer("Closed Floor");
    }

    public void toWall()
    {
        myRenderer.sprite = Wall;
        isExplored = false;
        isUnexplored = false;
        isWall = true;
        isOpen = false;
        isClosed = false;
        isRendezvousPoint = false;
        isLoopCell = false;
        gameObject.layer  = LayerMask.NameToLayer("Wall");
    }

    public void toRendezvousPoint()
    {
        myRenderer.sprite = RendezvousPoint;
        isExplored = false;
        isUnexplored = false;
        isWall = false;
        isOpen = true;
        isClosed = false;
        isRendezvousPoint = true;
        isLoopCell = false;
        gameObject.layer = LayerMask.NameToLayer("Floor");
    }

    void Awake ()
    {
        myRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Am Anfang werden soviele Einträge erstellt, wie es Roboter gibt. Die Roboter speichern ihre Daten dann am Index ihrer eigenen ID
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("GameController")[0].GetComponent<LevelCreator>().NrOfRobots; i++)
        {
            Stored Default = new Stored();
            MarkedByRobot.Add(Default);
        }
    }
}
