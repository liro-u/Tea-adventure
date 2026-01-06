using UnityEngine;

public class CharacterSkin : MonoBehaviour
{
    [Header("Skin")]
    [SerializeField] private Transform skinRoot; // where the skin is attached
    [SerializeField] private RuntimeAnimatorController animatorController;
    [SerializeField] private GameObject defaultSkin;
    [SerializeField] private bool applyRootMotion = true;

    private GameObject currentSkin;
    private CharacterAnimation characterAnimation;

    private void Awake()
    {
        for (int i = skinRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(skinRoot.GetChild(i).gameObject);
        }

        characterAnimation = GetComponent<CharacterAnimation>();
        SetSkin(defaultSkin);
    }

    public void SetSkin(GameObject newSkinPrefab)
    {
        if (newSkinPrefab == null)
        {
            Debug.LogWarning("SetSkin called with null prefab.");
            return;
        }

        // Remove old skin
        if (currentSkin != null)
        {
            Destroy(currentSkin);
        }

        // Spawn new skin
        GameObject newSkin = Instantiate(newSkinPrefab, skinRoot);
        newSkin.transform.localPosition = Vector3.zero;
        newSkin.transform.localRotation = Quaternion.identity;
        newSkin.transform.localScale = Vector3.one;

        // Assign animator controller
        Animator animator = newSkin.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("New skin has no Animator component.");
        }
        else if (animatorController != null)
        {
            animator.runtimeAnimatorController = animatorController;
            characterAnimation.animator = animator;
            animator.applyRootMotion = applyRootMotion;
        }

        currentSkin = newSkin;
    }
}
