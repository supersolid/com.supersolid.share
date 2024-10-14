using UnityEngine;

public class ShareComponent : MonoBehaviour
{
    [SerializeField] private string title,
                                    subject,
                                    body;

    public void Share()
    {
        ShareUtils.OpenShareComposer(title, subject, body);
    }
}
