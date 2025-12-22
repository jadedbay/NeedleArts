using HutongGames.PlayMaker;
using UnityEngine;

namespace NeedleArts;

public static class PlayMakerUtil {
    public static void AddActionAtIndex(this FsmState state, FsmStateAction action, int index) {
        index = Mathf.Clamp(index, 0, state.Actions.Length);
        FsmStateAction[] newActions = new FsmStateAction[state.Actions.Length + 1];
        
        for (int i = 0; i < index; i++) 
            newActions[i] = state.Actions[i];
        
        newActions[index] = action;
        
        for (int i = index; i < state.Actions.Length; i++) 
            newActions[i + 1] = state.Actions[i];
        
        state.Actions = newActions;
        action.Init(state);
    }
}