using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TicketMachineView : MonoBehaviour
{
    [Serializable]
    private class TicketLabel
    {
        public TicketType ticketType;
        public string text;

        public TicketLabel(TicketType ticketType, string text)
        {
            this.ticketType = ticketType;
            this.text = text;
        }
    }

    [Header("Referencias")]
    [SerializeField] private BotoneraManager machine;
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Button correctButton;
    [SerializeField] private Button confirmButton;

    [Header("Mensajes de pantalla")]
    [SerializeField, TextArea] private string idleMessage = "SELECCIONÁ TARIFA";
    [SerializeField, TextArea] private string blockedMessage = "BOLETERA BLOQUEADA";
    [SerializeField, TextArea] private string selectedMessage = "{ticket}";
    [SerializeField, TextArea] private string confirmedMessage = "CONFIRMADO: {ticket}";

    [Header("Nombre de cada boleto")]
    [SerializeField]
    private TicketLabel[] ticketLabels =
    {
        new TicketLabel(TicketType.Corto, "CORTO"),
        new TicketLabel(TicketType.Medio, "MEDIO"),
        new TicketLabel(TicketType.Largo, "LARGO"),
        new TicketLabel(TicketType.Jubilado, "JUBILADO"),
        new TicketLabel(TicketType.Discapacitado, "DISCAPACITADO"),
        new TicketLabel(TicketType.Escolar, "ESCOLAR"),
        new TicketLabel(TicketType.FreePass, "PASE LIBRE")
    };

    private void Awake()
    {
        if (machine != null && displayText != null &&
            correctButton != null && confirmButton != null) return;

        Debug.LogError("TicketMachineView: falta asignar alguna referencia.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        if (machine == null || displayText == null ||
            correctButton == null || confirmButton == null) return;

        correctButton.onClick.AddListener(ClearSelection);
        confirmButton.onClick.AddListener(ConfirmSelection);
        machine.StateChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (machine != null)
            machine.StateChanged -= Refresh;

        if (correctButton != null)
        {
            correctButton.onClick.RemoveListener(ClearSelection);
            correctButton.interactable = false;
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmSelection);
            confirmButton.interactable = false;
        }
    }

    private void ClearSelection() => machine.ClearSelection();

    private void ConfirmSelection() => machine.ConfirmSelection();

    private void Refresh()
    {
        correctButton.interactable = machine.IsAvailable &&
            (machine.SelectedTicket.HasValue || machine.LastConfirmedTicket.HasValue);

        confirmButton.interactable = machine.CanConfirm;

        if (!machine.IsAvailable)
            displayText.text = blockedMessage;
        else if (machine.SelectedTicket.HasValue)
            displayText.text = BuildMessage(selectedMessage, machine.SelectedTicket.Value);
        else if (machine.LastConfirmedTicket.HasValue)
            displayText.text = BuildMessage(confirmedMessage, machine.LastConfirmedTicket.Value);
        else
            displayText.text = idleMessage;
    }

    private string BuildMessage(string message, TicketType ticket)
    {
        // Reemplazo {ticket} por el nombre que pusiste en el Inspector.
        return (message ?? string.Empty).Replace("{ticket}", GetLabel(ticket));
    }

    private string GetLabel(TicketType ticket)
    {
        foreach (TicketLabel label in ticketLabels)
        {
            if (label != null && label.ticketType == ticket)
                return label.text ?? string.Empty;
        }

        return ticket.ToString();
    }
}
