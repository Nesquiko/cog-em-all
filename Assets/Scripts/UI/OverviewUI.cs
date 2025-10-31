using UnityEngine;

public class OverviewUI : MonoBehaviour
{
    public void StartOperation()
    {
        SceneLoader.LoadScene("GameScene", true);
    }
}
