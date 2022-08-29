using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Npc : MonoBehaviour
{

    NavMeshAgent agent;
    Animator animator;
    public Transform player;
    State currentState;
    public Transform[] waypoints;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentState = new Idle(gameObject, agent, animator, player, waypoints);
    }

    // Update is called once per frame
    void Update()
    {
        currentState = currentState.Process();


    }
}
