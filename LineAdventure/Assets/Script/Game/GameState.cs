using System;
using UniRx;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/GameState")]
public class GameState : ScriptableObject
{
    public enum State
    {
        Title,
        Game,
        Pose,
        Result,
    }

    public State state { get; private set; } = State.Title;

    public IObservable<State> ChangeState => changeState;
    private Subject<State> changeState = new Subject<State>();

    public void SetState(State state)
    {
        this.state = state;
        changeState.OnNext(state);
    }
}
