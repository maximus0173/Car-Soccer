using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class MatchCamerasManager : MonoBehaviour
{

    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private PlayableDirector goalScoredForYellowTimeline;
    [SerializeField] private PlayableDirector goalScoredForRedTimeline;

    public void DoGoalScoredCelebration(MatchManager.Team forTeam, System.Action onComplete)
    {
        StartCoroutine(GoalScoredCelebration(forTeam, onComplete));
    }

    private IEnumerator GoalScoredCelebration(MatchManager.Team forTeam, System.Action onComplete)
    {
        this.cinemachineBrain.m_DefaultBlend.m_Time = 1f;

        PlayableDirector timeline = null;
        switch (forTeam)
        {
            case MatchManager.Team.Yellow:
                timeline = this.goalScoredForYellowTimeline;
                break;
            case MatchManager.Team.Red:
                timeline = this.goalScoredForRedTimeline;
                break;
        }

        timeline.Play();

        float time = (float)timeline.duration - 0.1f;
        yield return new WaitForSeconds(time);

        this.cinemachineBrain.m_DefaultBlend.m_Time = 0f;

        onComplete();
    }

}
