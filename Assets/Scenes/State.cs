
using UnityEngine;
using UnityEngine.AI;

public class State
{
    public enum STATE
    {
        IDLE, PATROL, PURSUE, ATTACK, SLEEP
    }

    public enum EVENT
    {
        ENTER, UPDATE, EXIT
    }

    public STATE stateName;
    protected EVENT stage;
    protected GameObject npc;
    protected Animator anim;
    protected Transform player;
    protected State nextState;
    protected NavMeshAgent agent;

    float visDist = 10.0f;
    float visAngle = 30.0f;
    float shootDist = 7.0f;

    public State(GameObject _npc, NavMeshAgent _agent,
        Animator _anim, Transform _player)
    {
        npc = _npc;
        agent = _agent;
        anim = _anim;
        stage = EVENT.ENTER;
        player = _player;
    }

    public virtual void Enter() { stage = EVENT.UPDATE; }
    public virtual void Update() { stage = EVENT.UPDATE; }
    public virtual void Exit() { stage = EVENT.EXIT; }

    public State Process()
    {
        switch (stage)
        {
            case EVENT.ENTER:
                Enter();
                break;

            case EVENT.UPDATE:
                Update();
                break;

            case EVENT.EXIT:
                Exit();
                return nextState;
        }

        return this;
    }

    public bool CanSeePlayer()
    {
        Vector3 direction = player.position - npc.transform.position;
        float angle = Vector3.Angle(direction, npc.transform.forward);

        return direction.magnitude < visDist && angle < visAngle;
    }

    public bool CanAttackPlayer()
    {
        Vector3 direction = player.position - npc.transform.position;

        return direction.magnitude < shootDist;
    }
}

// IDLE
public class Idle : State
{
    public Idle(GameObject _npc, NavMeshAgent _agent,
        Animator _anim, Transform _player) :
        base(_npc, _agent, _anim, _player)
    {
        stateName = STATE.IDLE;
    }

    public override void Enter()
    {
        anim.SetTrigger("isIdle");
        base.Enter();
    }

    public override void Update()
    {
        if (Random.Range(0, 100) < 10)
        {
            nextState = new Patrol(npc, agent, anim, player);
            stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        anim.ResetTrigger("isIdle");
        base.Exit();
    }
}

// PATROL
public class Patrol : State
{
    int currentIndex = -1;

    public Patrol(GameObject _npc, NavMeshAgent _agent,
        Animator _anim, Transform _player) :
        base(_npc, _agent, _anim, _player)
    {
        stateName = STATE.PATROL;
        agent.speed = 2;
        agent.isStopped = false;
    }

    public override void Enter()
    {
        anim.SetTrigger("isWalking");
        SetNextCheckpoint();
        base.Enter();
    }

    void SetNextCheckpoint()
    {
        if (GameEnvironment.Singleton.Checkpoints.Count == 0)
            return;

        currentIndex++;

        if (currentIndex >= GameEnvironment.Singleton.Checkpoints.Count)
            currentIndex = 0;

        agent.SetDestination(
            GameEnvironment.Singleton.Checkpoints[currentIndex].transform.position
        );
    }

    public override void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 1)
        {
            SetNextCheckpoint();
        }
    }

    public override void Exit()
    {
        anim.ResetTrigger("isWalking");
        base.Exit();
    }
}

// PURSUE
public class Pursue : State
{
    public Pursue(GameObject _npc, NavMeshAgent _agent,
        Animator _anim, Transform _player) :
        base(_npc, _agent, _anim, _player)
    {
        stateName = STATE.PURSUE;
        agent.speed = 5;
        agent.isStopped = false;
    }

    public override void Enter()
    {
        anim.SetTrigger("isRunning");
        base.Enter();
    }

    public override void Update()
    {
        agent.SetDestination(player.position);

        if (agent.hasPath)
        {
            if (CanAttackPlayer())
            {
                nextState = new Attack(npc, agent, anim, player);
                stage = EVENT.EXIT;
            }
            else if (!CanSeePlayer())
            {
                nextState = new Patrol(npc, agent, anim, player);
                stage = EVENT.EXIT;
            }
        }
    }

    public override void Exit()
    {
        anim.ResetTrigger("isRunning");
        base.Exit();
    }
}

// ATTACK
public class Attack : State
{
    float rotationSpeed = 2.0f;
    AudioSource shoot;

    public Attack(GameObject _npc, NavMeshAgent _agent,
        Animator _anim, Transform _player) :
        base(_npc, _agent, _anim, _player)
    {
        stateName = STATE.ATTACK;
        shoot = _npc.GetComponent<AudioSource>();
    }

    public override void Enter()
    {
        anim.SetTrigger("isShooting");
        agent.isStopped = true;
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        anim.ResetTrigger("isShooting");
        agent.isStopped = false;
        base.Exit();
    }
}





