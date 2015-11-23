using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

    // Public
    public float SimulationStepSeconds = 0.0f;

    // Private
    private LevelCreator levelCreator;
    private Transform Map; // Elternobjekt für alle Zellen-Prefabs
    private Transform Robots; // Elternobjekt für alle Zellen-Prefabs
    private Vector2 MapSize; // Zeilen * Spalten
    private int BorderSize;
    private int rendevouzPointIndex;
    private int level = 3;
    private bool isStep = false;

    // #########################################################################################################
    // ################################################################ Hilfsfunktionen

    // gibt eine Liste aller Indexe der Nachbarn einer Zelle zurück, wobei die Vertikalen Nachbarn ignoriert werden
    private void GetOpenNeighboursIndex(int SourceCellIndex, List<int> Neighbours)
    {
        int mapX = (int)MapSize.x + (BorderSize * 2);
        int mapY = (int)MapSize.y + (BorderSize * 2);
        Vector2 GridPosition = Map.GetChild(SourceCellIndex).GetComponent<Cell>().GridPosition;

        if (GridPosition.x > 0)
        {
            if (Map.GetChild(SourceCellIndex - mapX).GetComponent<Cell>().isOpen)
            {
                Neighbours.Add(Map.GetChild(SourceCellIndex - mapX).GetComponent<Cell>().ownIndex);
            }
        }

        if (GridPosition.x < mapX)
        {
            if (Map.GetChild(SourceCellIndex + mapX).GetComponent<Cell>().isOpen)
            {
                Neighbours.Add(Map.GetChild(SourceCellIndex + mapX).GetComponent<Cell>().ownIndex);
            }
        }

        if (GridPosition.y > 0)
        {
            if (Map.GetChild(SourceCellIndex - 1).GetComponent<Cell>().isOpen)
            {
                Neighbours.Add(Map.GetChild(SourceCellIndex - 1).GetComponent<Cell>().ownIndex);
            }
        }

        if (GridPosition.y < mapY)
        {
            if (Map.GetChild(SourceCellIndex + 1).GetComponent<Cell>().isOpen)
            {
                Neighbours.Add(Map.GetChild(SourceCellIndex + 1).GetComponent<Cell>().ownIndex);
            }
        }
    }

    // Gibt true zurück, wenn es im Sichtbereich des Roboters (der auf der Zelle mit dem SourceCellIndex steht) einen offenen Weg zur Zelle mit dem TargetCellIndex gibt
    private bool CellIsReachable(int SourceCellIndex, int TargetCellIndex, List<Transform> RealFieldOfView)
    {
        HashSet<int> AlreadyFound = new HashSet<int>();
        List<int> OldNeighbours = new List<int>();
        List<int> NewNeighbours = new List<int>();
        int OldNrOfCells = 0;
        int NewNrOfCells = 0;

        OldNeighbours.Add(SourceCellIndex);
        AlreadyFound.Add(SourceCellIndex);
        NewNrOfCells = 1; // == AlreadyFound.Count zu diesem Zeitpunkt

        while (OldNrOfCells < NewNrOfCells)
        //while(NewNrOfCells < RealFieldOfView.Count)
        {
            if (AlreadyFound.Contains(TargetCellIndex))
            {
                return true;
            }
            else
            {
                foreach (int n in OldNeighbours)
                {
                    GetOpenNeighboursIndex(n, NewNeighbours);
                }
                OldNeighbours.Clear();
                foreach (int n in NewNeighbours)
                {
                    if (!AlreadyFound.Contains(n) && RealFieldOfView.Contains(Map.GetChild(n)))
                    {
                        AlreadyFound.Add(n);
                        OldNeighbours.Add(n);
                    }
                }
                OldNrOfCells = NewNrOfCells;
                NewNrOfCells = AlreadyFound.Count;
                NewNeighbours.Clear();
            }
        }
        return false;
    }

    // Überprüft, ob bereits ein anderer Roboter auf einer Zelle steht
    private bool CellIsEmpty(int CellIndex)
    {
        for (int i = 0; i < Robots.childCount; i++)
        {
            if(Robots.GetChild(i).GetComponent<Robot>().OwnMapIndex == CellIndex)
            {
                return false;
            }
        }
        return true;
    }

    // Gibt die Roboter zurück, die im Sichtbereich des momentan aktiven Roboters liegen, inklusive des eigenen Roboters
    private void getRobotsInView(Robot robot, List<Robot> robotInViewList, List<Transform> inRealView)
    {
        for (int i = 0;i<Robots.childCount;i++) {
            Robot r = Robots.GetChild(i).GetComponent<Robot>();
            foreach (Transform t in inRealView)
            {
                if (t.GetComponent<Cell>().ownIndex == r.OwnMapIndex)
                {
                    robotInViewList.Add(r);
                }
            }
        }
    }


    // Gibt die Zellen zurück, die am Rand des Sichtfelds liegen
    private void getFrontierCells(Robot robot, List<Transform> inRealView, List<Transform> fronterCells)
    {
        List<Transform> neig = new List<Transform>();

        foreach (Transform t in inRealView)
        {
            getAllNeighbours(t, neig);
            foreach (Transform tt in neig)
            {
                if (!robot.InViewField(tt) || !robot.IsInSight(tt) && (robot.InViewField(tt) && !tt.GetComponent<Cell>().isWall))
                {
                    if (!t.GetComponent<Cell>().isClosed)
                    {
                        fronterCells.Add(t);
                    }
                    break;
                }
            }

            neig.Clear();
        }
    }

    // Füllt die Liste mit allen Feldern, die der Roboter sehen kann
    private void GetRealFieldOfView(Robot robot, List<Transform> InRealView)
    {
        for (int i = 0; i < Map.childCount; i++)
        {
            // Gehe alle Zellen der Karte durch und überprüfe, ob sie im Sichtbereich des Roboters liegen (ohne Schatten/Sichtblockaden)
            if (robot.InViewField(Map.GetChild(i)))
            {
                // Gehe alle Zellen im Sichtradius durch und überprüfe, ob sie Verdeckt sind/im Schatten einer Wand liegen
                if (robot.IsInSight(Map.GetChild(i)))
                {
                    InRealView.Add(Map.GetChild(i));
                }
            }
        }
    }

    private void GetReachableFieldOfView(Robot robot, List<Transform> inRealView, List<Transform> InReachableView)
    {
        foreach (Transform t in inRealView)
        {
            if (CellIsReachable(robot.OwnMapIndex,t.GetComponent<Cell>().ownIndex,inRealView))
            {
                InReachableView.Add(t);
            }
        }
    }

    // gibt eine Liste aller Nachbarn einer Zelle zurück, wobei die Vertikalen Nachbarn ignoriert werden
    private void getCrossNeighbours(Transform cellTransform, List<Transform> neighbours)
    {
        Cell cell = cellTransform.GetComponent<Cell>();
        int mapX = (int)MapSize.x + (BorderSize * 2);
        int mapY = (int)MapSize.y + (BorderSize * 2);


        if (cell.GridPosition.x > 0)
        {
            neighbours.Add(Map.GetChild(cell.ownIndex - mapX));
        }

        if (cell.GridPosition.x < mapX)
        {
            neighbours.Add(Map.GetChild(cell.ownIndex + mapX));
        }

        if (cell.GridPosition.y > 0)
        {
            neighbours.Add(Map.GetChild(cell.ownIndex - 1));
        }

        if (cell.GridPosition.y < mapY)
        {
            neighbours.Add(Map.GetChild(cell.ownIndex + 1));
        }
    }

    // gibt eine Liste aller Nachbarn einer Zelle zurück, auch die Vertikalen
    private void getAllNeighbours(Transform cellTransform, List<Transform> neighbours)
    {
        Cell cell = cellTransform.GetComponent<Cell>();
        int mapX = (int)MapSize.x + (BorderSize * 2);
        int mapY = (int)MapSize.y + (BorderSize * 2);


        if (cell.GridPosition.x > 0)
        {
            neighbours.Add(Map.GetChild(cell.ownIndex - mapX));
            if (cell.GridPosition.y > 0)
            {
                neighbours.Add(Map.GetChild(cell.ownIndex - mapX - 1));
            }
            if (cell.GridPosition.y < mapY)
            {
                neighbours.Add(Map.GetChild(cell.ownIndex - mapX + 1));
            }
        }

        if (cell.GridPosition.x < mapX)
        {
            neighbours.Add(Map.GetChild(cell.ownIndex + mapX));
            if (cell.GridPosition.y > 0)
            {
                neighbours.Add(Map.GetChild(cell.ownIndex + mapX - 1));
            }
            if (cell.GridPosition.y < mapY)
            {
                neighbours.Add(Map.GetChild(cell.ownIndex + mapX + 1));
            }
        }

        if (cell.GridPosition.y > 0)
        {
            neighbours.Add(Map.GetChild(cell.ownIndex - 1));
        }

        if (cell.GridPosition.y < mapY)
        {
            neighbours.Add(Map.GetChild(cell.ownIndex + 1));
        }
    }

    private bool LoopDetected(int RobotID, int CellIndex)
    {
        // Feld wurde schoneinmal besucht und man betritt es durch ein anderes Feld als letztemal
        if (Map.GetChild(CellIndex).GetComponent<Cell>().MarkedByRobot[RobotID].LastExitDirection != -1 && (Map.GetChild(CellIndex).GetComponent<Cell>().MarkedByRobot[RobotID].LastExitDirection+2) % 4 != Robots.GetChild(RobotID).GetComponent<Robot>().CurrentDirection)
        {
            // Seit dem Letzten Besuch hat der Roboter keine Zellen geschlossen und die Zelle wird von keinem anderen Roboter Kontrolliert
            if (Map.GetChild(CellIndex).GetComponent<Cell>().ControlledBy == -1 && Map.GetChild(CellIndex).GetComponent<Cell>().MarkedByRobot[RobotID].LastNrOfClosedCells == Robots.GetChild(RobotID).GetComponent<Robot>().ClosedCells)
            {
                //Debug.Log("Robot Nr " + RobotID.ToString() + ": LOOP DETECTED. Current Direction = " + Robots.GetChild(RobotID).GetComponent<Robot>().CurrentDirection.ToString() + ", last Dir % 4 = " + (Map.GetChild(CellIndex).GetComponent<Cell>().MarkedByRobot[RobotID].LastExitDirection % 4).ToString() + ", last exit dir = " + Map.GetChild(CellIndex).GetComponent<Cell>().MarkedByRobot[RobotID].LastExitDirection.ToString());
                return true;
            }
        }
        return false;
    }

    private int GetDirection(Vector2 StartCellPosition, Vector2 DestinationCellPosition)
    {
        Vector2 result = StartCellPosition - DestinationCellPosition;

        if (result.x == 1)
        {
            return 3;
        }
        else if (result.x == -1)
        {
            return 1;
        }
        else if (result.y == 1)
        {
            return 2;
        }
        else if (result.y == -1)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }

    // #########################################################################################################
    // ################################################################ BMILRV Main Functions

    //Der erste Schritt des Algorithmus im Zustand LOOP_DETECTION
    private void FlagFieldOfView(int RobotID, List<Transform> RealFieldOfView, List<Transform> FrontierCells)
    {
        Robot robot = Robots.GetChild(RobotID).GetComponent<Robot>();

        List<Robot> robotsInView = new List<Robot>();
        getRobotsInView(robot, robotsInView, RealFieldOfView);

        List<Transform> reachableCells = new List<Transform>();
        GetReachableFieldOfView(robot, RealFieldOfView, reachableCells);

        // Markiere alle benötigten Grenzen
        foreach (Transform t in FrontierCells)
        {
            if (!t.GetComponent<Cell>().isRendezvousPoint)
            {
                t.GetComponent<Cell>().toExploredCell(robot.ID);
            }
        }

        // Schließe alle anderen Felder, außer denen, die du brauchst, um die Grenzen zu erreichen oder das RendevouzFeld zu erreichen
        foreach (Transform t in RealFieldOfView)
        {
            if (!FrontierCells.Contains(t) && reachableCells.Contains(t) && !t.GetComponent<Cell>().isRendezvousPoint && !t.GetComponent<Cell>().isClosed && CellIsEmpty(t.GetComponent<Cell>().ownIndex)) 
            {

                //t.GetComponent<Cell>().toClosedCell();
                t.GetComponent<Cell>().fakeClosedCell();
                bool ok = true;

                // erst überprüfen, ob die zelle notwendig für die grenzen ist oder für einen anderen Roboter nötig ist... (robots in View enthält auch den eigenen Roboter!)
                foreach (Robot r in robotsInView)
                {
                    foreach (Transform tt in FrontierCells)
                    {
                        if(!CellIsReachable(r.OwnMapIndex, tt.GetComponent<Cell>().ownIndex, RealFieldOfView) && reachableCells.Contains(tt))
                        {
                            t.GetComponent<Cell>().toExploredCell(robot.ID);
                            ok = false;
                            break;
                        }
                    }
                }

                // ... und dann, ob sie notwendig für die rückkehr zum rendevouz punkt ist
                if (robot.InViewField(Map.GetChild(rendevouzPointIndex)))
                {
                    if (!CellIsReachable(robot.OwnMapIndex, rendevouzPointIndex, RealFieldOfView))
                    {
                        t.GetComponent<Cell>().toExploredCell(robot.ID);
                        ok = false;
                    }
                }

                if (ok)
                {
                    t.GetComponent<Cell>().toClosedCell();
                    robot.ClosedCells = robot.ClosedCells + 1;
                }
            }
        }
    }

    // Der zweite Schritt des Algorithmus im Zustand LOOP_DETECTION
    // Gehe zur nächsten Zelle. Wenn diese nicht existiert, ist der Algorithmus vorbei
    private void Navigate(int RobotID)
    {
        Robot robot = Robots.GetChild(RobotID).GetComponent<Robot>();
        int nextCell = Map.GetChild(robot.OwnMapIndex).GetComponent<Cell>().ownIndex;
        int bestEmpyt = Map.GetChild(robot.OwnMapIndex).GetComponent<Cell>().ownIndex;
        int bestNonempty = Map.GetChild(robot.OwnMapIndex).GetComponent<Cell>().ownIndex;
        int disGradientEmpty = 9999;
        int disGradientNonEmpty = 9999;

        // Wähle aus der Liste der offenen Nachbarn eine Zelle aus, dabei gilt: unexplored > explored
        List<int> OpenNeigbours = new List<int>();
        GetOpenNeighboursIndex(robot.OwnMapIndex, OpenNeigbours);

        if(OpenNeigbours.Count == 0)
        {
            //Algorithmus ist zuende
            Debug.Log("Ende für Roboter " + robot.ID.ToString());
            robot.agentState = 5; // OFF
        }
        else
        {
            foreach (int i in OpenNeigbours)
            {
                Cell CurrentNeigbour = Map.GetChild(i).GetComponent<Cell>();

                // Wenn es eine Unerkundete Zelle in der Nachbarschaft gibt, wird diese allen anderen Vorgezogen
                if (CurrentNeigbour.isUnexplored)
                {
                    nextCell = CurrentNeigbour.ownIndex;
                    break;
                }
                else
                {
                    if (CellIsEmpty(CurrentNeigbour.ownIndex))
                    {
                        if (CurrentNeigbour.dispersionGradient < disGradientEmpty)
                        {
                            bestEmpyt = CurrentNeigbour.ownIndex;
                            disGradientEmpty = CurrentNeigbour.dispersionGradient;
                        }
                    }
                    else
                    {
                        // Wenn die nächste Zelle eine bereits erkundete ist, wird dem dispersion gradient gefolgt, wobei Leere Zellen vorgezogen werden (kein anderer Roboter auf der Zelle)
                        if (CurrentNeigbour.dispersionGradient < disGradientNonEmpty)
                        {
                            bestNonempty = CurrentNeigbour.ownIndex;
                            disGradientNonEmpty = CurrentNeigbour.dispersionGradient;
                        }
                    }
                }
            }
            if (bestEmpyt != robot.OwnMapIndex)
            {
                nextCell = bestEmpyt;
            }
            else
            {
                nextCell = bestNonempty;
            }

            // Nun wird der Schritt ausgeführt
            robot.CurrentDirection = GetDirection(robot.GridPosition, Map.GetChild(nextCell).GetComponent<Cell>().GridPosition);
            robot.GridPosition = Map.GetChild(nextCell).GetComponent<Cell>().GridPosition;
            robot.LastCellIndex = robot.OwnMapIndex;
            robot.OwnMapIndex = Map.GetChild(nextCell).GetComponent<Cell>().ownIndex;
            Robots.GetChild(RobotID).position = Map.GetChild(nextCell).position;
        }   
    }

    //Der dritte Schritt des Algorithmus im Zustand LOOP_DETECTION
    // Jetzt wird überprüft, ob die Zelle, von der der Roboter grade kommt, geschlossen werden kann. Dazu wird sie probehalber geschlossen und geguckt, ob vom neuen Standort aus alle anderen offenen Zellen im Sichtbereich erreicht werden können
    private void CloseLastCell(int RobotID, int LastCellIndex, List<Transform> FrontierCells, List<Transform> RealFieldOfView)
    {
        Cell LastCell = Map.GetChild(LastCellIndex).GetComponent<Cell>();
        Robot robot = Robots.GetChild(RobotID).GetComponent<Robot>();
        bool ok = false;
        int gradValue = 9999;
        List<int> OpenNeigbours = new List<int>();
        GetOpenNeighboursIndex(LastCellIndex, OpenNeigbours);

        // setze den dispersion Gradient Value der Zelle
        foreach (int i in OpenNeigbours)
        {
            if (Map.GetChild(i).GetComponent<Cell>().dispersionGradient <= gradValue)
            {
                gradValue = Map.GetChild(i).GetComponent<Cell>().dispersionGradient + 1;
            }
        }
        LastCell.dispersionGradient = gradValue;

        // Überprüfe dann, ob du sie Zelle schließen kannst
        if (!LastCell.isRendezvousPoint)
        {
            LastCell.fakeClosedCell();

            foreach (Transform FrontierCell in FrontierCells)
            {
                if (!CellIsReachable(robot.OwnMapIndex, FrontierCell.GetComponent<Cell>().ownIndex, RealFieldOfView))
                {
                    ok = false;
                    break;
                }
            }

            if (robot.InViewField(Map.GetChild(rendevouzPointIndex)))
            {
                // TEste, ob du auch mit diesem feld den RP nicht erreichen kannst
                Map.GetChild(LastCellIndex).GetComponent<Cell>().toExploredCell(robot.ID);
                if (!CellIsReachable(robot.OwnMapIndex, rendevouzPointIndex, RealFieldOfView))
                {
                    Map.GetChild(LastCellIndex).GetComponent<Cell>().fakeClosedCell();
                    if (CellIsEmpty(LastCellIndex) && OpenNeigbours.Count <= 1)
                    {
                        ok = true;
                    }
                }
                else
                {
                    Map.GetChild(LastCellIndex).GetComponent<Cell>().fakeClosedCell();
                    if (!CellIsReachable(robot.OwnMapIndex, rendevouzPointIndex, RealFieldOfView))
                    {
                        ok = false;
                    }
                    else if (CellIsEmpty(LastCellIndex) && OpenNeigbours.Count <= 1)
                    {
                        ok = true;
                    }
                }

            }
            else if (CellIsEmpty(LastCellIndex) && OpenNeigbours.Count <= 1)
            {
                ok = true;
            }

            if (ok)
            {
                LastCell.toClosedCell();
                robot.ClosedCells = robot.ClosedCells + 1;
            }
            else
            {
                LastCell.toExploredCell(robot.ID);
                LastCell.MarkedByRobot[robot.ID].LastNrOfClosedCells = robot.ClosedCells;
                LastCell.MarkedByRobot[robot.ID].LastExitDirection = GetDirection(LastCell.GridPosition, robot.GridPosition);
                //Debug.Log("Robot Nr " + RobotID.ToString() + " markiert die letzte Zelle " + LastCell.GridPosition.ToString() + ". LastExitDirection = " + LastCell.MarkedByRobot[RobotID].LastExitDirection.ToString());
            }
        }
        else
        {
            robot.agentState = 0;
        }
    }

    private void LoopControl(int RobotID)
    {
        Robot robot = Robots.GetChild(RobotID).GetComponent<Robot>();
        Cell CurrentLoopCell = Map.GetChild(robot.OwnMapIndex).GetComponent<Cell>();
        Cell NextCell = null;

        List<int> OpenNeigbours = new List<int>();
        GetOpenNeighboursIndex(CurrentLoopCell.ownIndex, OpenNeigbours);

        if(CurrentLoopCell.MarkedByRobot[robot.ID].IsLoopCell)
        {
            if(CurrentLoopCell.ControlledBy == RobotID)
            {
                // Anfangszelle gefunden, gehe zu LOOP_CLOSING, aber vorher noch einen Schritt weiter!
                robot.agentState = 2;

                // Folge den eigenen Richtungsangaben
                foreach (int i in OpenNeigbours)
                {
                    if (GetDirection(CurrentLoopCell.GridPosition, Map.GetChild(i).GetComponent<Cell>().GridPosition) == CurrentLoopCell.MarkedByRobot[RobotID].LastExitDirection && !Map.GetChild(i).GetComponent<Cell>().isClosed)
                    {
                        NextCell = Map.GetChild(i).GetComponent<Cell>();
                    }
                }
                if (NextCell != null)
                {
                    // Gehe zur nächsten Zelle
                    robot.GridPosition = NextCell.GridPosition;
                    robot.LastCellIndex = robot.OwnMapIndex;
                    robot.OwnMapIndex = NextCell.ownIndex;
                    Robots.GetChild(RobotID).position = Map.GetChild(NextCell.ownIndex).position;
                    robot.AfterLoopBeginning = true;
                }
                else
                {
                    robot.agentState = 3;
                }
            }
            else if (CurrentLoopCell.ControlledBy < RobotID)
            {
                // Ich habe Vorrang und warte, bis der andere Roboter diese Zelle "gecleaned" hat
                robot.agentState = 0;
            }
            else
            {
                // Der andere Roboter hat Vorrang, ich muss sauber machen
                robot.agentState = 3;
            }
        }
        else
        {
            // Folge den eigenen Richtungsangaben
            foreach (int i in OpenNeigbours)
            {
                if (GetDirection(CurrentLoopCell.GridPosition,Map.GetChild(i).GetComponent<Cell>().GridPosition) == CurrentLoopCell.MarkedByRobot[RobotID].LastExitDirection && !Map.GetChild(i).GetComponent<Cell>().isClosed)
                {
                    NextCell = Map.GetChild(i).GetComponent<Cell>();
                }
            }
            if (NextCell != null)
            {
                // Markiere die aktuelle Zelle
                CurrentLoopCell.toLoopCell(robot.ID);
                CurrentLoopCell.ControlledBy = RobotID;

                // Gehe zur nächsten Zelle
                robot.GridPosition = NextCell.GridPosition;
                robot.LastCellIndex = robot.OwnMapIndex;
                robot.OwnMapIndex = NextCell.ownIndex;
                Robots.GetChild(RobotID).position = Map.GetChild(NextCell.ownIndex).position;
            }
            else
            {
                robot.agentState = 3;
            }
        }
    }

    private void LoopClosing(int RobotID)
    {
        Robot robot = Robots.GetChild(RobotID).GetComponent<Robot>();
        Cell CurrentLoopCell = Map.GetChild(robot.OwnMapIndex).GetComponent<Cell>();
        Cell NextCell = null;

        List<int> OpenNeigbours = new List<int>();
        GetOpenNeighboursIndex(robot.OwnMapIndex, OpenNeigbours);
        
        if(CurrentLoopCell.MarkedByRobot[robot.ID].IsLoopCell)
        //if (CurrentLoopCell.isLoopCell)
        {
            if(robot.AfterLoopBeginning && OpenNeigbours.Count >= 3)
            {
                // Der Roboter ist an einer Kreuzung, gehe zu LOOP_CLEANING
                robot.agentState = 3;
                robot.AfterLoopBeginning = false;
            }
            else if (!robot.AfterLoopBeginning && OpenNeigbours.Count >= 2) 
            {
                // Der Roboter ist an einer Kreuzung, gehe zu LOOP_CLEANING
                robot.agentState = 3;
            }
            else
            {
                robot.AfterLoopBeginning = false;
                // Folge den eigenen Richtungsangaben
                foreach (int i in OpenNeigbours)
                {
                    if (GetDirection(CurrentLoopCell.GridPosition, Map.GetChild(i).GetComponent<Cell>().GridPosition) == CurrentLoopCell.MarkedByRobot[RobotID].LastExitDirection && !Map.GetChild(i).GetComponent<Cell>().isClosed)
                    {
                        NextCell = Map.GetChild(i).GetComponent<Cell>();
                    }
                }
                if (NextCell != null)
                {
                    // Markiere die aktuelle Zelle
                    CurrentLoopCell.toClosedCell();

                    // Gehe zur nächsten Zelle
                    robot.GridPosition = NextCell.GridPosition;
                    robot.LastCellIndex = robot.OwnMapIndex;
                    robot.OwnMapIndex = NextCell.ownIndex;
                    Robots.GetChild(RobotID).position = Map.GetChild(NextCell.ownIndex).position;
                }
                else
                {
                    //Error, es gibt kein nächste Zelle, gehe zu Loop_CLEANING
                    robot.agentState = 3;
                }
            }
        }
        else
        {
            //Error, gehe zu Loop_DETECTION
            robot.agentState = 0;
        }
    }

    private void LoopCleaning(int RobotID)
    {
        Robot robot = Robots.GetChild(RobotID).GetComponent<Robot>();
        Cell CurrentLoopCell = Map.GetChild(robot.OwnMapIndex).GetComponent<Cell>();
        Cell NextCell = null;

        List<int> OpenNeigbours = new List<int>();
        GetOpenNeighboursIndex(robot.OwnMapIndex, OpenNeigbours);

        if(CurrentLoopCell.MarkedByRobot[robot.ID].IsLoopCell)
        {
            // Entferne alle Spuren von dir
            CurrentLoopCell.toExploredCell(robot.ID);
            if(CurrentLoopCell.ControlledBy == robot.ID)
            {
                CurrentLoopCell.ControlledBy = -1;
            }

            // Folge den eigenen Richtungsangaben
            foreach (int i in OpenNeigbours)
            {
                if (robot.LastCellIndex == i && !Map.GetChild(i).GetComponent<Cell>().isClosed)
                {
                    NextCell = Map.GetChild(i).GetComponent<Cell>();
                }
            }
            if (NextCell != null)
            {
                // Markiere die aktuelle Zelle
                CurrentLoopCell.toExploredCell(robot.ID);

                // Gehe zur nächsten Zelle
                robot.GridPosition = NextCell.GridPosition;
                robot.LastCellIndex = robot.OwnMapIndex;
                robot.OwnMapIndex = NextCell.ownIndex;
                Robots.GetChild(robot.ID).position = Map.GetChild(NextCell.ownIndex).position;
            }
            else
            {
                //Error, es gibt kein nächste Zelle, gehe zu Loop_DETECTION
                robot.agentState = 0;
            }
        }
        else
        {
            //gehe zu Loop_DETECTION
            robot.agentState = 0;
        }
    }

    private void Standby(int RobotID)
    {
        Robot robot = Robots.GetChild(RobotID).GetComponent<Robot>();
        Cell CurrentCell = Map.GetChild(robot.OwnMapIndex).GetComponent<Cell>();

        if (CurrentCell.ControlledBy == -1) //Zelle ist frei geworden
        {
            robot.agentState = 1; // gehe zu loop_control
        }
        else if (CurrentCell.ControlledBy != robot.ID ||  CurrentCell.isClosed)
        {
            robot.agentState = 3; // gehe zu loop_cleaning
        }
    }

    // one simulation step
    private bool running = false;
    public IEnumerator step()
    {
        while (true)
        {
            if (running)
            {
                for (int i = 0; i < Robots.childCount; i++)
                {
                    Robot robot = Robots.GetChild(i).GetComponent<Robot>();
                    List<Transform> RealFieldOfView = new List<Transform>();
                    GetRealFieldOfView(robot, RealFieldOfView);

                    List<Transform> FrontierCells = new List<Transform>();
                    getFrontierCells(robot, RealFieldOfView, FrontierCells);

                    //FlagFieldOfView(robot.ID, RealFieldOfView, FrontierCells);
                    //Navigate(robot.ID);
                    //CloseLastCell(robot.ID, robot.LastCellIndex, FrontierCells, RealFieldOfView);

                    // 0:LOOP_DETECTION 1:LOOP_CONTROL 2:LOOP_CLOSING 3:LOOP_CLEANING 4:STANDBY 5:OFF
                    switch (robot.agentState)
                    {
                        case 0:
                            FlagFieldOfView(robot.ID, RealFieldOfView, FrontierCells);
                            if (!LoopDetected(robot.ID, robot.OwnMapIndex)) {
                                Navigate(robot.ID);
                                CloseLastCell(robot.ID, robot.LastCellIndex, FrontierCells, RealFieldOfView);
                                break;
                            }
                            else
                            {
                                // Wenn eine Schleife erkannt wurde, mache bei Loop_CONTROL WEITER
                                robot.agentState = 1;
                                goto case 1;
                            }
                        case 1:
                            LoopControl(robot.ID);
                            break;
                        case 2:
                            LoopClosing(robot.ID);
                            break;
                        case 3:
                            LoopCleaning(robot.ID);
                            break;
                        case 4:
                            Standby(robot.ID);
                            break;
                        case 5:
                            break;
                        default:
                            break;
                    }
                    //Debug.Log(logg);
                    if (isStep)
                    {
                        running = false;
                        isStep = false;
                    }
                        
                }
                yield return new WaitForSeconds(SimulationStepSeconds);
            }
            yield return new WaitForSeconds(0.0f);
        }
    }

    // #########################################################################################################
    // ################################################################ Simulation Methods and Input

    // Wird beim Start des Programms ausgeführt 
    void Awake()
    {
        levelCreator = GetComponent<LevelCreator>();
    }

    // Wird beim Start, aber nach Awake() ausgeführt
    void Start()
    {
        levelCreator.setupScene(level);
        Map = levelCreator.Map;
        MapSize = levelCreator.MapSize;
        BorderSize = levelCreator.BorderSize;
        Robots = levelCreator.Robots;
        rendevouzPointIndex = levelCreator.rendevouzPointIndex;

        // Der Button zum Starten/Stoppen der Simulation wird komplett per Script verwaltet
        Button simButton = GameObject.FindGameObjectsWithTag("SimButton")[0].GetComponent<Button>();
        simButton.onClick.AddListener(() => this.startSim());

        // Der Button zum Steppen der Simulation wird komplett per Script verwaltet
        Button stepButton = GameObject.FindGameObjectsWithTag("StepButton")[0].GetComponent<Button>();
        stepButton.onClick.AddListener(() => this.stepSim());
    }

    public void startSim()
    {
        running = true;
        StartCoroutine(step());
        GameObject simButtonObject = GameObject.FindGameObjectsWithTag("SimButton")[0];
        Button simButton = simButtonObject.GetComponent<Button>();

        simButton.onClick.RemoveAllListeners();
        simButton.onClick.AddListener(() => this.stopSim());
        simButton.GetComponentInChildren<Text>().text = "Stop";
    }

    public void stopSim()
    {
        running = false;

        GameObject simButtonObject = GameObject.FindGameObjectsWithTag("SimButton")[0];
        Button simButton = simButtonObject.GetComponent<Button>();
        simButton.onClick.RemoveAllListeners();
        simButton.onClick.AddListener(() => this.forwardSim());
        simButton.GetComponentInChildren<Text>().text = "Weiter";
    }

    public void forwardSim()
    {
        running = true;

        GameObject simButtonObject = GameObject.FindGameObjectsWithTag("SimButton")[0];
        Button simButton = simButtonObject.GetComponent<Button>();
        simButton.onClick.RemoveAllListeners();
        simButton.onClick.AddListener(() => this.stopSim());
        simButton.GetComponentInChildren<Text>().text = "Stop";
    }

    public void stepSim()
    {
        if (!running)
        {
            running = true;
            isStep = true;
        }
    }

    void Update()
    {
        // Kamerahöhe
        if (Input.GetButtonDown("Vertical"))
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                Camera.main.orthographicSize = Camera.main.orthographicSize + 1;
            }
            else
            {
                Camera.main.orthographicSize = Camera.main.orthographicSize - 1;
            }
        }

        // Map nach links/rechts schieben
        if (Input.GetButtonDown("A"))
        {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x - 0.1f, Camera.main.transform.position.y, Camera.main.transform.position.z);
        }

        if (Input.GetButtonDown("D"))
        {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x + 0.1f, Camera.main.transform.position.y, Camera.main.transform.position.z);
        }
    }
}
