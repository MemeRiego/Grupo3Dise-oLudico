using UnityEngine;
using UnityEngine.UI;

public class TicketButton : MonoBehaviour
{
    [SerializeField] private BotoneraManager botonera;
    [SerializeField] private TicketType ticketType;
    [SerializeField] private Color selectedColor = new Color(1f, 0.8f, 0.35f);

    private Button button;
    private ColorBlock originalColors;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalColors = button.colors;

        if (botonera == null)
        {
            Debug.LogError("TicketFareButton: falta asignar Machine.", this);
            button.interactable = false;
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (botonera == null) return;
        button.onClick.AddListener(SelectTicket);
        botonera.StateChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(SelectTicket);
            button.colors = originalColors;
            button.interactable = false;
        }
        if (botonera != null) botonera.StateChanged -= Refresh;
    }

    private void SelectTicket()
    {
        botonera.SelectTicket(ticketType);
    }

    private void Refresh()
    {
        bool selected = botonera.SelectedTicket == ticketType;
        ColorBlock colors = originalColors;
        if (selected)
        {
            colors.normalColor = selectedColor;
            colors.highlightedColor = selectedColor;
            colors.selectedColor = selectedColor;
        }
        button.colors = colors;
        button.interactable = botonera.IsAvailable;
    }
}
