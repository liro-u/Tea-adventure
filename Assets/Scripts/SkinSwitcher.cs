using UnityEngine;

public class SkinSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject skinPrefab;
    TriggerZone triggerZone;

    private void Awake()
    {
        triggerZone = GetComponent<TriggerZone>();

        triggerZone.OnEnter.AddListener(changeSkin);
    }

    private void changeSkin(Collider other)
    {
        CharacterSkin characterSkin = other.GetComponent<CharacterSkin>();

        if (characterSkin != null)
        {
            characterSkin.SetSkin(skinPrefab);
        }
    }
}
