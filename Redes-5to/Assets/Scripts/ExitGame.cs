using Fusion;
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private bool _isLeaving;

    public async void Exit()
    {
        NetworkRunner runner =
                 gameManager != null ? gameManager.Runner : null;

        if (runner != null)
            await runner.Shutdown();
        Application.Quit();
    }
    

    
}