
public abstract class BotState
{
    protected BotController Bot;
    protected BotStateMachine BotStateMachine;
    
    public BotState(BotController bot, BotStateMachine botStateMachine)
    {
        Bot = bot;
        BotStateMachine = botStateMachine;
    }
    public virtual void EnterState(){}
    public virtual void ExitState(){}
    public virtual void FrameUpdate(){}
    //public virtual void PhysicsUpdate(){}
}
