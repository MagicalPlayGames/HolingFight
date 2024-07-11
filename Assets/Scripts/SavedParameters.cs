using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SavedParameters : MonoBehaviour
{
    private float[][][] bestWeights;
    //private Organize[] players;
    private Move[] players;
    private int index = 0;
    private bool saved = false;
    private float bestPoints = -999;
    private int count;

    [Tooltip("Default/Updated parameters to set the next player")]
    public MoveParameters nextParams;
    public GameObject playerPrefab;
    [Tooltip("The maximum number of players on the field")]
    public byte numOfPlayers;
    [Tooltip("The color for the player with the best weights")]
    public Material golden;

    public Material blank;

    [Tooltip("Iterations counting text")]
    public Text countText;

    public static SavedParameters Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        reloadPlayers();
        numOfPlayers = (byte)players.Length;
    }

    private void reloadPlayers()
    {
        bool noChange = !nullInList(players);
        if (noChange)
            return;
        GameObject[] p = GameObject.FindGameObjectsWithTag("Player");
        players = new Move[p.Length];
        //players = new Organize[p.Length];
        for (int i = 0; i < p.Length; i++)
            players[i] = p[i].GetComponent<Move>();
            //players[i] = p[i].GetComponent<Organize>();
    }

    private static bool nullInList(Move[] list)
    {
        if (list==null)
            return true;
        if (list.Length == 0)
            return true;
        for (int i = 0; i < list.Length; i++)
            if (list[i] == null)
                return true;

            return false;
    }

    private static bool nullInList(Organize[] list)
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


    public void nextPerson()
    {
        Vector3 spawnPos = new Vector3(0,1.2f,0);
        index = -1;
        for (byte i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                if(players[i].transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material == golden)
                    players[i].transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material = blank;
                float points = players[i].getPoints();
                if (points > bestPoints)
                {
                    bestPoints = points;
                    index = i;
                }
            }
        }
        if (index != -1)
        {
            if (players[index] != null)
            {
                bestWeights = players[index].gameObject.GetComponent<MoveANN>().getWeights();
                if(players[index].gameObject.GetComponent<MoveANN>().getParameters()!=null)
                    nextParams = players[index].gameObject.GetComponent<MoveANN>().getParameters();
                spawnPos = players[index].transform.position;
                players[index].transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material = golden;
                saved = true;
            }
        }
            if (GameObject.FindGameObjectsWithTag("Player").Length <= numOfPlayers)
        {
            GameObject newPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity, null);
            if (count % 10==1)
                numOfPlayers++;
            count++;
            countText.text = "Iterations:" + count.ToString();
            newPlayer.GetComponent<Move>().parameters = nextParams;
            //newPlayer.GetComponent<Organize>().parameters = nextParams;
            nextParams.useAgain();
            if (saved)
                newPlayer.GetComponent<MoveANN>().setWeights(bestWeights);
        }
        reloadPlayers();
    }
}
