using UnityEngine;

public class ClawAnimationController : MonoBehaviour
{
    private Animator animator;
    private float animationProgress = 0f;
    private bool isPlayingForward = false;
    private bool isPlayingBackward = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.enabled = true;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.C))
        {
            isPlayingForward = true;
            isPlayingBackward = false;
        }
        else if (Input.GetKey(KeyCode.V))
        {
            isPlayingBackward = true;
            isPlayingForward = false;
        }
        else
        {
            isPlayingForward = false;
            isPlayingBackward = false;
        }

        if (isPlayingForward)
        {
            animationProgress += Time.deltaTime;
        }
        else if (isPlayingBackward)
        {
            animationProgress -= Time.deltaTime;
        }

        animationProgress = Mathf.Clamp(animationProgress, 0f, 1f);
        animator.Play("Base Layer.Forward", 0, animationProgress);
    }

    public void SetClawProgress(float progress)
    {
        animationProgress = Mathf.Clamp01(progress);
        animator.Play("Base Layer.Forward", 0, animationProgress);
    }

    public float GetClawProgress() => animationProgress;
}