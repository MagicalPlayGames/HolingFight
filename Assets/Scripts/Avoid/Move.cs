using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Move : MonoBehaviour
{
    // Start is called before the first frame update

    /*
     * 
     * 
     */
    
    private float reduceBy;
    private float subBy;
    private float points;
    private Vector3 nextPos;
    private GameObject[] players;
    private Vector3 velocity;
    private bool flag = false;
    private NavMeshAgent agent;

    
    [SerializeField]
    [Tooltip("The Artifical Neural Network to use")]
    private MoveANN network;
    public MoveParameters parameters;
    [HideInInspector]
    public SavedParameters save;

    public Vector3 minBounds;
    public Vector3 maxBounds;

    [Tooltip("Colors to use when a player is doing good, bad, or the best")]
    public Material[] goodBadBest;

    private void Awake()
    {
        StartCoroutine(timer());
        reduceBy = maxBounds.x - minBounds.x;
        subBy = maxBounds.x; 
        agent = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        save = SavedParameters.Instance;
        setParameters();
    }

    public float getPoints()
    {
        return points;
    }
    private void reloadPlayers()
    {
        if (!nullInList(players))
            return;
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    private static bool nullInList(GameObject[] list)
    {
        if (list == null)
            return true;
        if (list.Length == 0)
            return true;
        for (int i = 0; i < list.Length; i++)
            if (list[i] == null)
                return true;

        return false;
    }
    private void setParameters()
    {
        if (parameters != null)
        {
            reloadPlayers();
            network.setUpNetwork(parameters);
            network.resetWeights();
            network.readInput(getInputs());
            float[] outputs = network.writeOutputs();
            nextPos = new Vector3((outputs[0] * reduceBy) - subBy, minBounds.y, (outputs[1] * reduceBy) - subBy);
            StartCoroutine(recordPoints());
        }
        else
            StartCoroutine(waitToSet());

    }

    private void finishedMoving()
    {
            network.readTrueOutput(getBestArea());
            network.updateAllWeights();
        network.readInput(getInputs());
        float[] outputs = network.writeOutputs();
        nextPos = new Vector3((outputs[0]* reduceBy) - subBy, minBounds.y, (outputs[1]* reduceBy) - subBy);
    }

    private GameObject findFirst(GameObject curPlayer)
    {
        foreach(GameObject player in players)
        {
            if (player != null)
                if(curPlayer!=player)
                    return player;
        }
        return null;
    }

    // Update is called once per frame
    private Vector3[] getInputs()
    {
        Vector3[] Pos = new Vector3[save.numOfPlayers];
        int j = 1;
        for (int i = 0; i < players.Length; i++)
        {
            if (j >= Pos.Length)
                break;
            if (players[i] != null)
                if (players[i] != this.gameObject)
                    Pos[j] = players[i].transform.position;
                else
                    j--;
            else
                Pos[j] = findFirst(this.gameObject).transform.position;
            j++;
        }
        Pos[0] = this.transform.position;
        for (int i = 0; i < Pos.Length; i++)
        {
            Pos[i].x = (Pos[i].x +subBy+0.1f)/reduceBy;
            Pos[i].z = (Pos[i].z + subBy+0.1f) /reduceBy;
        }

            return Pos;

    }
    private void Update()
    {
        if (Vector3.Distance(transform.position, nextPos) > 1 && !flag)
        {
            move();
            flag = true;
        }
    }

    private void move()
    {
        agent.destination = nextPos;
    }

    private bool awayFromOthers(float x, float z, bool addPoints = false)
    {
        reloadPlayers();
        Vector3 pos = new Vector3(x, minBounds.y, z);
        for (int i =0;i<players.Length;i++)
        {
            GameObject player = players[i];
            if (player != null)
                if (player.transform.position != pos)
                {
                    if (Vector3.Distance(pos, player.transform.position) < GetComponent<SphereCollider>().radius*2)
                    {
                        return false;
                    }
                    else if (addPoints)
                        points += Vector3.Distance(pos, player.transform.position);
                }
        }
        return true;
    }

    private Vector3 getBestArea()
    {
        float x = UnityEngine.Random.Range(minBounds.x, maxBounds.x);
        float z = UnityEngine.Random.Range(minBounds.z, maxBounds.z);
        while (!awayFromOthers(x,z))
        {
            x = UnityEngine.Random.Range(minBounds.x, maxBounds.x);
            z = UnityEngine.Random.Range(minBounds.z, maxBounds.z);
        }
        return new Vector3(x,minBounds.y,z);
    }

    private void OnTriggerStay(Collider other)
    {
        if (flag && other.tag=="Player")
        {
            flag = false;
            finishedMoving();
            points -= 30f;
            if(this.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material!=goodBadBest[2])
                this.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material = goodBadBest[1];
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (this.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material != goodBadBest[2])
                this.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material = goodBadBest[0];
        }
    }


    private IEnumerator recordPoints()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1));
        awayFromOthers(transform.position.x, transform.position.z, true);
        StartCoroutine(recordPoints());
    }

    private IEnumerator waitToSet()
    {
        yield return new WaitForSeconds(1);
        setParameters();
    }

    private IEnumerator timer()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(20, 60));
        save.nextPerson();
        Destroy(this.gameObject);
    }
}
