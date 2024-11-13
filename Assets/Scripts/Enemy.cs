using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;

// FSM States for the enemy
public enum EnemyState { STATIC, CHASE, REST, MOVING, DEFAULT };

public enum EnemyBehavior {EnemyBehavior1, EnemyBehavior2, EnemyBehavior3 };

public class Enemy : MonoBehaviour
{
    //pathfinding
    protected PathFinder pathFinder;
    public GenerateMap mapGenerator;
    protected Queue<Tile> path;
    protected GameObject playerGameObject;

    public Tile currentTile;
    protected Tile targetTile;
    public Vector3 velocity;

    //properties
    public float speed = 1.0f;
    public float visionDistance = 5;
    public int maxCounter = 5;
    protected int playerCloseCounter;

    protected EnemyState state = EnemyState.DEFAULT;
    protected Material material;

    public EnemyBehavior behavior = EnemyBehavior.EnemyBehavior1; 

    // Start is called before the first frame update
    void Start()
    {
        path = new Queue<Tile>();
        pathFinder = new PathFinder();
        playerGameObject = GameObject.FindWithTag("Player");
        playerCloseCounter = maxCounter;
        material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (mapGenerator.state == MapState.DESTROYED) return;

        // Stop Moving the enemy if the player has reached the goal
        if (playerGameObject.GetComponent<Player>().IsGoalReached() || playerGameObject.GetComponent<Player>().IsPlayerDead())
        {
            //Debug.Log("Enemy stopped since the player has reached the goal or the player is dead");
            return;
        }

        switch(behavior)
        {
            case EnemyBehavior.EnemyBehavior1:
                HandleEnemyBehavior1();
                break;
            case EnemyBehavior.EnemyBehavior2:
                HandleEnemyBehavior2();
                break;
            case EnemyBehavior.EnemyBehavior3:
                HandleEnemyBehavior3();
                break;
            default:
                break;
        }

    }

    public void Reset()
    {
        Debug.Log("enemy reset");
        path.Clear();
        state = EnemyState.DEFAULT;
        currentTile = FindWalkableTile();
        transform.position = currentTile.transform.position;
    }

    Tile FindWalkableTile()
    {
        Tile newTarget = null;
        int randomIndex = 0;
        while (newTarget == null || !newTarget.mapTile.Walkable)
        {
            randomIndex = (int)(Random.value * mapGenerator.width * mapGenerator.height - 1);
            newTarget = GameObject.Find("MapGenerator").transform.GetChild(randomIndex).GetComponent<Tile>();
        }
        return newTarget;
    }

    // Dumb Enemy: Keeps Walking in Random direction, Will not chase player
    private void HandleEnemyBehavior1()
    {
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 
                
                //Changed the color to white to differentiate from other enemies
                material.color = Color.white;
                
                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;
                
                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }

                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // TODO: Enemy chases the player when it is nearby
    private void HandleEnemyBehavior2()
    {
        switch(state){
            case EnemyState.DEFAULT:
                material.color = Color.blue;
        
                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;
            case EnemyState.MOVING:
                float distanceToPlayer = Vector3.Distance(playerGameObject.transform.position, currentTile.transform.position);
                if (distanceToPlayer < visionDistance){
                    state = EnemyState.CHASE;
                    break;
                }
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;
                
                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }

                break;
            case EnemyState.CHASE:
            // The path is not being updated fast enough but it is pathing to the tile that the game object walked before being updated again and recalculations  
                Debug.Log("It is being chased");
                material.color = Color.red;
                Tile ptile = playerGameObject.GetComponent<Player>().currentTile;
                if (path.Count <= 0 || targetTile != ptile){
                    targetTile = ptile;
            
                    path = pathFinder.FindPathAStar(currentTile, targetTile);
                }

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                else
                {
                    state = EnemyState.DEFAULT;
                }
               
                break;
            default:
                material.color = Color.blue;
                state = EnemyState.DEFAULT;
                break;

        }
        
    }

    // TODO: Third behavior So its like the algoritm above but with an offset pursuit
    private void HandleEnemyBehavior3()
    {
        switch(state){
            case EnemyState.DEFAULT:
                material.color = Color.green;
        
                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;
            case EnemyState.MOVING:
                float distanceToPlayer = Vector3.Distance(playerGameObject.transform.position, currentTile.transform.position);
                if (distanceToPlayer < visionDistance){
                    state = EnemyState.CHASE;
                    break;
                }
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;
                
                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }

                break;
            case EnemyState.CHASE:
                Debug.Log("It is being chased");
                material.color = Color.red;
                Tile ptile = playerGameObject.GetComponent<Player>().currentTile;
                Tile offsetTile = GetOffsetTile(ptile, 2);
                if (path.Count <= 0 || targetTile != offsetTile){
                    targetTile = offsetTile;
            
                    path = pathFinder.FindPathAStar(currentTile, targetTile);
                }

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                else
                {
                    state = EnemyState.DEFAULT;
                }
               
                break;

            default:
                material.color = Color.blue;
                state = EnemyState.DEFAULT;
                break;
        }

        

    }
    private Tile GetOffsetTile(Tile playerTile, int offset)
    {
    foreach (Tile nextTile in playerTile.Adjacents)
    {
        Tile current = nextTile;
        bool pathIsClear = true;
        for (int i = 1; i < offset; i++)
        {
            if (!current.isPassable || current.Adjacents.Count == 0)
            {
                pathIsClear = false;
                break;
            }
            current = current.Adjacents[0]; 
        }
        if (pathIsClear && current.isPassable)
        {
            return current;
        }
    }
    return playerTile;
    
    }
}
