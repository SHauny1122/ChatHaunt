using UnityEngine;

/// <summary>
/// Preserves the approved skeletal Root curves, including inversion and drop.
/// Carries the authored 0.22 m displacement across repeating crawl cycles.
/// No gameplay, navigation or encounter behaviour.
/// </summary>
[RequireComponent(typeof(Animator))]
public sealed class GhostWomanLoopTravel : MonoBehaviour
{
    Animator animator;
    int previousState, previousCycle;
    bool initialized;
    public void ResetTracking(){initialized=false;previousState=0;previousCycle=0;}
    void Awake(){animator=GetComponent<Animator>();}
    void LateUpdate(){AccumulateCycleTravel();}
    public void AccumulateCycleTravel()
    {
        if(!animator)animator=GetComponent<Animator>();
        var state=animator.GetCurrentAnimatorStateInfo(0);
        int cycle=Mathf.Max(0,Mathf.FloorToInt(state.normalizedTime));
        if(!initialized || previousState!=state.fullPathHash)
        {previousState=state.fullPathHash;previousCycle=cycle;initialized=true;return;}
        int crossed=cycle-previousCycle;
        if(crossed!=0 && !animator.IsInTransition(0))
        {
            float travel=state.IsName("GW_FloorCrawl_Loop")?.22f:state.IsName("GW_CeilingCrawl_Loop")?-.22f:0;
            transform.position+=transform.TransformVector(new Vector3(0,0,travel*crossed));
        }
        previousCycle=cycle;
    }
}
