
public class BotStateMachine
{
    public BotState CurrenState { get; set; }

    public void Initialize(BotState startingState)
    {
        CurrenState = startingState;
        CurrenState.EnterState();
    }

    public void ChangeState(BotState newState)
    {
        CurrenState.ExitState();
        CurrenState = newState;
        CurrenState.EnterState();
    }
}
