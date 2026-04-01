using UnityEngine;

[CreateAssetMenu(fileName = "ConsentDialogConfig", menuName = "ScriptableObjects/ConsentDialogConfig")]
public class ConsentDialogConfig : ScriptableObject
{
    public string Title;
    public string Body;
    public string Footer;
}
