using UnityEngine;
using System;
using System.Collections.Generic;


public class LevelCreator : MonoBehaviour {

    public Vector2 MapSize = new Vector2(5,5); // Zeilen * Spalten
    public int BorderSize = 1;
    public int NrOfRobots = 1;

    //Prefabs
    public Vector2 SpriteSizeInPixel = new Vector2(100f, 100f); // Zeilen * Spalten
    public GameObject Cell;
    public GameObject Robot;
    public Vector2 RendezvousPointCoordinates = new Vector2(1f, 1f);
    //public LayerMask WallLayer;

    public Transform Map; // Elternobjekt für alle Zellen-Prefabs
    public Transform Robots; // Elternobjekt für alle Robot-Prefabs

    public int rendevouzPointIndex;

    private void createGardenMap(List<Vector2> map)
    {
        //public Vector2 MapSize = new Vector2(5,5); // Zeilen * Spalten
        //public int BorderSize = 1;
        Vector2 Point1 = new Vector2((float)Math.Round(MapSize.x * 0.1f) , (float)Math.Round(MapSize.y * 0.65f));
        Vector2 Point2 = new Vector2((float)Math.Round(MapSize.x * 0.1f), (float)Math.Round(MapSize.y * 0.35f));
        
        Vector2 Point3 = new Vector2((float)Math.Round(MapSize.x * 0.9f), (float)Math.Round(MapSize.y * 0.65f));
        Vector2 Point4 = new Vector2((float)Math.Round(MapSize.x * 0.9f), (float)Math.Round(MapSize.y * 0.35f));

        float tmpX = Point1.x;
        float tmpY = Point1.y;
        bool alternate = true;
        int StepSize = 1;
        // Links oben
        while (tmpY < ((float)Math.Round(MapSize.y * 0.9f)))
        {
            map.Add(new Vector2(tmpX,tmpY));
            if (alternate)
            {
                tmpX = tmpX + StepSize;
                alternate = false;
            }
            else
            {
                tmpY = tmpY + StepSize;
                alternate = true;
            }
        }

        tmpX = Point2.x;
        tmpY = Point2.y;
        alternate = true;

        // Links unten
        while (tmpY > ((float)Math.Round(MapSize.y * 0.1f)))
        {
            map.Add(new Vector2(tmpX, tmpY));
            if (alternate)
            {
                tmpX = tmpX + StepSize;
                alternate = false;
            }
            else
            {
                tmpY = tmpY - StepSize;
                alternate = true;
            }
        }

        tmpX = Point3.x;
        tmpY = Point3.y;
        alternate = true;
        // Rechts oben
        while (tmpY < ((float)Math.Round(MapSize.y * 0.9f)))
        {
            map.Add(new Vector2(tmpX, tmpY));
            if (alternate)
            {
                tmpX = tmpX - StepSize;
                alternate = false;
            }
            else
            {
                tmpY = tmpY + StepSize;
                alternate = true;
            }
        }

        tmpX = Point4.x;
        tmpY = Point4.y;
        alternate = true;
        // Rechts unten
        while (tmpY > ((float)Math.Round(MapSize.y * 0.1f)))
        {
            map.Add(new Vector2(tmpX, tmpY));
            if (alternate)
            {
                tmpX = tmpX - StepSize;
                alternate = false;
            }
            else
            {
                tmpY = tmpY - StepSize;
                alternate = true;
            }
        }

        //Mittelpunkt der Karte
        Vector2 Point5 = new Vector2((float)Math.Round(MapSize.x * 0.4f), (float)Math.Round(MapSize.y * 0.5f));
        Vector2 Point6 = new Vector2((float)Math.Round(MapSize.x * 0.5f), (float)Math.Round(MapSize.y * 0.4f));

        Vector2 Point7 = new Vector2((float)Math.Round(MapSize.x * 0.6f), (float)Math.Round(MapSize.y * 0.5f));
        Vector2 Point8 = new Vector2((float)Math.Round(MapSize.x * 0.5f), (float)Math.Round(MapSize.y * 0.6f));

        tmpX = Point5.x;
        tmpY = Point5.y;
        alternate = true;
        // Links oben
        while (tmpY > ((float)Math.Round(MapSize.y * 0.4f)))
        {
            map.Add(new Vector2(tmpX, tmpY));
            if (alternate)
            {
                tmpX = tmpX + StepSize;
                alternate = false;
            }
            else
            {
                tmpY = tmpY - StepSize;
                alternate = true;
            }
        }

        tmpX = Point6.x;
        tmpY = Point6.y;
        alternate = true;
        // Links unten
        while (tmpY < ((float)Math.Round(MapSize.y * 0.5f)))
        {
            map.Add(new Vector2(tmpX, tmpY));
            if (alternate)
            {
                tmpX = tmpX + StepSize;
                alternate = false;
            }
            else
            {
                tmpY = tmpY + StepSize;
                alternate = true;
            }
        }

        tmpX = Point7.x;
        tmpY = Point7.y;
        alternate = true;
        // Rechts oben
        while (tmpY < ((float)Math.Round(MapSize.y * 0.6f)))
        {
            map.Add(new Vector2(tmpX, tmpY));
            if (alternate)
            {
                tmpX = tmpX - StepSize;
                alternate = false;
            }
            else
            {
                tmpY = tmpY + StepSize;
                alternate = true;
            }
        }

        tmpX = Point8.x;
        tmpY = Point8.y;
        alternate = true;
        // Rechts unten
        while (tmpY > ((float)Math.Round(MapSize.y * 0.5f)))
        {
            map.Add(new Vector2(tmpX, tmpY));
            if (alternate)
            {
                tmpX = tmpX - StepSize;
                alternate = false;
            }
            else
            {
                tmpY = tmpY - StepSize;
                alternate = true;
            }
        }
    }

    private List<Vector2> map1 = new List<Vector2>
    {
        new Vector2(12f, 29f),
        new Vector2(12f, 28f),
        new Vector2(12f, 27f),
        new Vector2(12f, 26f),
        new Vector2(12f, 25f),

        new Vector2(12f, 24f),
        new Vector2(12f, 23f),
        new Vector2(12f, 22f),
        new Vector2(12f, 21f),
        new Vector2(12f, 20f),

        new Vector2(12f, 19f),
        new Vector2(12f, 18f),
        new Vector2(12f, 17f),
        new Vector2(12f, 16f),
        new Vector2(12f, 15f),

        new Vector2(12f, 14f),
        new Vector2(12f, 13f),
        new Vector2(12f, 12f),
        new Vector2(12f, 11f),
        new Vector2(12f, 10f),

        new Vector2(5f, 0f),
        new Vector2(5f, 1f),
        new Vector2(5f, 2f),
        new Vector2(5f, 3f),
        new Vector2(5f, 4f),

        new Vector2(5f, 5f),
        new Vector2(5f, 6f),
        new Vector2(5f, 7f),
        new Vector2(5f, 8f),
        new Vector2(5f, 9f),

        new Vector2(5f, 10f),
        new Vector2(5f, 11f),
        new Vector2(5f, 12f),
        new Vector2(5f, 13f),
        new Vector2(5f, 14f),

        new Vector2(5f, 15f),
        new Vector2(5f, 16f),
        new Vector2(5f, 17f),
        new Vector2(5f, 18f),
        new Vector2(5f, 19f),
        
    };

    // Wenn die Map zu Groß für den Bildschirm wird, muss die Größe der Sprites angepasst werden (transform.localScale)
    // Dabei werden zwei Skalierungsfaktoren berechnet (für x und y), aber nur der größte wird zurückgegeben und auf alle Achsen angewandt
    private Vector3 getScaleFactor()
    {
        float columnsPixel = MapSize.x * SpriteSizeInPixel.x;
        float rowsPixel = MapSize.y * SpriteSizeInPixel.y;

        //Debug.Log("columnsPixel = " + columnsPixel.ToString());
        //Debug.Log("rowsPixel = " + rowsPixel.ToString());

        if (Screen.height < columnsPixel || Screen.width < rowsPixel)
        {
            float scaleX = Screen.height / columnsPixel;
            float scaleY = Screen.width / rowsPixel;

            //Debug.Log("scaleX = Screen.height / columns = " + Screen.height.ToString() + " / " + columnsPixel.ToString() + " = " + scaleX.ToString());
            //Debug.Log("scaleY = Screen.width / rows = " + Screen.width.ToString() + " / " + rowsPixel.ToString() + " = " + scaleY.ToString());

            if (scaleX < scaleY)
            {
                return new Vector3(scaleX, scaleX, scaleX);
            }
            else
            {
                return new Vector3(scaleY, scaleY, scaleY);
            }
        }
        else
        {
            //Debug.Log("scale = 1");
            return new Vector3(1f, 1f, 1f);
        }
    }

    private void setupMap(Vector3 localScale)
    {
        Map = new GameObject("Map").transform;
        int mapX = (int) MapSize.x + (BorderSize * 2); // + (BorderSize*2) wegen dem Rand
        int mapY = (int) MapSize.y + (BorderSize * 2);
        int i = 0;

        for (int x = 0; x < mapX; x++)
        {
            for (int y = 0; y < mapY; y++)
            {
                //GameObject toInstantiate = Cell;
                GameObject Instance = Instantiate(Cell, new Vector3(x * localScale.x, y * localScale.y, 0f), Quaternion.identity) as GameObject;

                if (x < BorderSize || x > mapX - BorderSize - 1 || y < BorderSize || y > mapY - BorderSize - 1) // == Zelle ist am Rand
                {
                    Instance.GetComponent<Cell>().toWall();
                }
                else if (x == (int) RendezvousPointCoordinates.x && y == (int)RendezvousPointCoordinates.y)
                {
                    Instance.GetComponent<Cell>().toRendezvousPoint();
                    rendevouzPointIndex = i;
                }
                else
                {
                    Instance.GetComponent<Cell>().toUnexploredCell();
                }

                Instance.GetComponent<Cell>().GridPosition = new Vector2(x,y);
                Instance.GetComponent<Cell>().ownIndex = i;

                i = i + 1;
                Instance.transform.SetParent(Map);
                Instance.transform.localScale = localScale;
                // Die Sprites (und damit auch die Prefabs) haben eine Größe von 100*100 Pixeln
                // Deswegen werden sie so skaliert, dass am Ende alle Prefabs auf einen Bildschirm passen
            }
        }

        List<Vector2> GardenMap = new List<Vector2>();
        createGardenMap(GardenMap);

        for (int ii = 0;ii<Map.childCount;ii++)
        {
            // Die Karte aka die Mauern laden
            /*foreach (Vector2 v in map1)
            {
                if (v.x + BorderSize == Map.GetChild(ii).GetComponent<Cell>().GridPosition.x && v.y + BorderSize == Map.GetChild(ii).GetComponent<Cell>().GridPosition.y)
                {
                    Map.GetChild(ii).GetComponent<Cell>().toWall();
                }
            }*/

            
            if (GardenMap.Contains(Map.GetChild(ii).GetComponent<Cell>().GridPosition))
            {
                Map.GetChild(ii).GetComponent<Cell>().toWall();
            }
        }
    }

    private void setupRobots(Vector3 localScale)
    {
        Robots = new GameObject("Robots").transform;

        for (int i = 0;i< NrOfRobots;i++)
        {
            //GameObject toInstantiate = Robot;
            GameObject Instance = Instantiate(Robot, new Vector3(RendezvousPointCoordinates.x * localScale.x, RendezvousPointCoordinates.y * localScale.y, 0f), Quaternion.identity) as GameObject;
            // Alle Roboter starten im RendezvousPoint und sollen über den Zellen "schweben"

            Instance.GetComponent<Robot>().GridPosition = new Vector2(RendezvousPointCoordinates.x, RendezvousPointCoordinates.y);
            for (int ii = 0; ii < Map.childCount; ii++)
            {
                if(Map.GetChild(ii).GetComponent<Cell>().GridPosition.x == RendezvousPointCoordinates.x && Map.GetChild(ii).GetComponent<Cell>().GridPosition.y == RendezvousPointCoordinates.y)
                {
                    Instance.GetComponent<Robot>().OwnMapIndex = Map.GetChild(ii).GetComponent<Cell>().ownIndex;
                }
            }
            Instance.GetComponent<Robot>().ID = i;
            Instance.layer = LayerMask.NameToLayer("Robot");
            Instance.transform.SetParent(Robots);
            Instance.transform.localScale = localScale;
        }
    }
    
    public void setupScene(int level)
    {
        Vector3 localScale = getScaleFactor();
        setupMap(localScale);
        setupRobots(localScale);

        // Setze Kamera auf den Mittelpunkt der Map
        Camera.main.transform.position = new Vector3((MapSize.x / 2)* localScale.x, (MapSize.y / 2)* localScale.y, Camera.main.transform.position.z);
    }
}
