using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Move Parameter")]
public class MoveParameters : ScriptableObject
{
    [Tooltip("Layers of hidden nodes")]
    public byte layers;
    [Tooltip("Node size for each layer")]
    public int[] nodes; 
    public float learningRate;
    [Tooltip("There are 3 different algorithms for backward propergation\nChoose 0,1,or 2")]
    public byte algoNum; 
    public byte outputs;
    [Tooltip("This is how many iterations have used this parameter")]
    public float numUses;
    public void setUp(byte l, int[] n,float lR,byte aN,byte o)
    {
        layers = l;
        n = nodes;
        learningRate = lR;
        algoNum = aN;
        outputs = o;
    }
    public void useAgain()
    {
        numUses++;
    }

}
