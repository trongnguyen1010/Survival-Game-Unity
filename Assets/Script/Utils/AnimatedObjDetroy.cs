using UnityEngine;

public class AnimatedObjDetroy : MonoBehaviour
{
    [SerializeField]  private Animator animator;


    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
    }
}
