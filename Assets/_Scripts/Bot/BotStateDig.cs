
using UnityEngine;

public class BotDigState : BotState
{
    public Vector3 _targetPos;
    private Vector3 _direction;
    
    public BotDigState(BotController bot, BotStateMachine botStateMachine) : base(bot, botStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        _direction = (_targetPos - Bot.transform.position).normalized;
    }
}
