using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class State
{
    public enum STATE
	{
		IDLE, PATROL, CHASE
	}

	public enum EVENT
	{
		ENTER, UPDATE, EXIT
	}

	public STATE stateName;
	protected EVENT stage;
	protected GameObject npc;
	protected NavMeshAgent agent;
	protected Animator animator;
	protected Transform player;
	protected Transform[] waypoints;
	protected State nextState;

	float visDist = 10.0f; 
	float visAngle = 30.0f;

	public State(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _player, Transform[] _waypoints)
	{
		npc = _npc;
		agent = _agent;
		animator = _animator;
		player = _player;
		waypoints = _waypoints;
	}

	public virtual void Enter()
	{
		stage = EVENT.UPDATE;
	}

	public virtual void Update()
	{
		stage = EVENT.UPDATE;
	}

	public virtual void Exit()
	{
		stage = EVENT.EXIT;
	}

	public State Process()
	{
		if(stage == EVENT.ENTER)
		{
			Enter();
		}
		else if(stage == EVENT.UPDATE)
		{
			Update();
		}
		else
		{
			Exit();
			return nextState;
		}

		return this;
	}

	public bool CanSeePlayer()
	{
		Vector3 direction = player.position - npc.transform.position; 
		float angle = Vector3.Angle(direction, npc.transform.forward); 

		
		if (direction.magnitude < visDist && angle < visAngle)
		{
			return true; 
		}
		return false; 
	}

}

public class Idle : State
{
	float timer;
	public Idle(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _player, Transform[] _waypoints) : base(_npc, _agent, _animator, _player, _waypoints)
	{
		stateName = STATE.IDLE;
	}

	public override void Enter()
	{
		agent.isStopped = true;
		animator.SetTrigger("idle");
		Debug.Log("Entrou em idle");
		base.Enter();
	}

	public override void Update()
	{
		Debug.Log("Rodando o idle");
		timer += Time.deltaTime;
		if(timer > 3)
		{
			nextState = new Patrol(npc, agent, animator, player, waypoints);
			stage = EVENT.EXIT;
		}
	}

	public override void Exit()
	{
		animator.ResetTrigger("idle");
		base.Exit();
	}
}

public class Patrol : State
{
	int currentIndex = -1;

	public Patrol(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player, Transform[] _waypoints)
				: base(_npc, _agent, _anim, _player, _waypoints)
	{

		stateName = STATE.PATROL; 
		agent.speed = 1.5f; 
		agent.isStopped = false; 
	}

	public override void Enter()
	{
		float lastDist = Mathf.Infinity; 

		
		for (int i = 0; i < waypoints.Length; i++)
		{
			Transform thisWP = waypoints[i];
			float distance = Vector3.Distance(npc.transform.position, thisWP.position);
			if (distance < lastDist)
			{
				currentIndex = i - 1; 
				lastDist = distance;
			}
		}
		animator.SetTrigger("walk"); 
		base.Enter();
	}

	public override void Update()
	{

		if (agent.remainingDistance < 1)
		{

			
			if (currentIndex >= waypoints.Length - 1)
				currentIndex = 0;
			else
				currentIndex++;

			agent.SetDestination(waypoints[currentIndex].position); 
		}

		if (CanSeePlayer())
		{
			nextState = new Chase(npc, agent, animator, player, waypoints);
			stage = EVENT.EXIT;
		}

	}

	public override void Exit()
	{
		animator.ResetTrigger("walk");
		base.Exit();
	}


}


public class Chase : State
{
	public Chase(GameObject _npc, NavMeshAgent _agent, Animator _animator, Transform _player, Transform[] _waypoints) : base(_npc, _agent, _animator, _player, _waypoints)
	{
		stateName = STATE.CHASE; 
		agent.speed = 5; 
		agent.isStopped = false; 
	}

	public override void Enter()
	{
		animator.SetTrigger("run"); 
		base.Enter();
	}



	public override void Update()
	{
		agent.SetDestination(player.position);  
		if (agent.hasPath)                       
		{
			
			if (!CanSeePlayer())
			{
				nextState = new Idle(npc, agent, animator, player, waypoints);
				stage = EVENT.EXIT;
			}
		}
	}

	public override void Exit()
	{
		animator.ResetTrigger("run");
		base.Exit();
	}
}
