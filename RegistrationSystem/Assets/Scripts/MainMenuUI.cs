using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] GameObject[] panels;

    public static MainMenuUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void OpenPanel(string panelName)
    {
        foreach (var panel in panels)
        {
            if (panel.name == panelName)
            {
                panel.SetActive(true);
            }
        }
    }

    public void OpenPanel(string panelName, string message, string panelToClose = null)
    {

        if (panelToClose != null)
        {
            foreach (var panel in panels)
            {
                if (panel.name == panelToClose)
                {
                    panel.SetActive(false);
                }
            }
        }

        foreach (var panel in panels)
        {
            if (panel.name == panelName)
            {
                panel.SetActive(true);
                TextMeshProUGUI text = panel.transform.Find("Panel/Text").GetComponent<TextMeshProUGUI>();
                text.text = message;
            }
        }
    }

    public void ClosePanel(string panelName)
    {
        foreach (var panel in panels)
        {
            if (panel.name == panelName)
            {
                panel.SetActive(false);
            }
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
