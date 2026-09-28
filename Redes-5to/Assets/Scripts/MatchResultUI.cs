using Fusion;
using UnityEngine;

public class MatchResultUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("Paneles de resultado")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.ResultChanged += RefreshResult;
        }
        else
        {
            Debug.LogWarning(
                "Falta asignar GameManager en MatchResultUI.",
                this
            );
        }

        // Leer el estado una vez al activar la interfaz.
        RefreshResult();
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.ResultChanged -= RefreshResult;
        }
    }

    private void RefreshResult()
    {
        if (victoryPanel == null || defeatPanel == null)
        {
            Debug.LogWarning(
                "Asigna Victory Panel y Defeat Panel.",
                this
            );
            return;
        }

        bool showVictory = false;
        bool showDefeat = false;

        if (gameManager != null &&
            gameManager.IsNetworkReady &&
            gameManager.Object != null &&
            gameManager.Object.IsValid &&
            gameManager.GameOver)
        {
            PlayerRef localPlayer =
                gameManager.Runner.LocalPlayer;

            PlayerRef winner = gameManager.Winner;

            if (localPlayer != PlayerRef.None &&
                winner != PlayerRef.None)
            {
                showVictory = winner == localPlayer;
                showDefeat = !showVictory;
            }
        }

        SetPanelVisible(victoryPanel, showVictory);
        SetPanelVisible(defeatPanel, showDefeat);
    }

    private static void SetPanelVisible(
        GameObject panel,
        bool visible)
    {
        // Evitar reactivar un panel que ya tiene el estado correcto.
        if (panel.activeSelf != visible)
        {
            panel.SetActive(visible);
        }
    }
}