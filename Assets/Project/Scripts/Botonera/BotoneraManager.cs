using System;
using UnityEngine;

public class BotoneraManager : MonoBehaviour
{
    [SerializeField] private bool available = true;

    public TicketType? SelectedTicket { get; private set; }
    public TicketType? LastConfirmedTicket { get; private set; }
    public bool IsAvailable => available && isActiveAndEnabled;
    public bool CanConfirm => IsAvailable && SelectedTicket.HasValue;

    public event Action StateChanged;
    public event Action<TicketType> TicketConfirmed;

    public bool SelectTicket(TicketType ticket)
    {
        if (!IsAvailable || (uint)ticket > (uint)TicketType.FreePass) return false;

        SelectedTicket = ticket;
        LastConfirmedTicket = null;
        StateChanged?.Invoke();
        return true;
    }

    public void ClearSelection()
    {
        if (!IsAvailable) return;

        SelectedTicket = null;
        LastConfirmedTicket = null;
        StateChanged?.Invoke();
    }

    public bool ConfirmSelection()
    {
        if (!CanConfirm) return false;

        TicketType ticket = SelectedTicket.Value;

        // Borro lo pendiente antes de avisar para evitar confirmar dos veces.
        SelectedTicket = null;
        LastConfirmedTicket = ticket;
        StateChanged?.Invoke();

        // Confirmar el boleto no significa que la SUBE ya lo haya pagado.
        TicketConfirmed?.Invoke(ticket);
        return true;
    }

    public void SetAvailable(bool value)
    {
        available = value;
        SelectedTicket = null;
        LastConfirmedTicket = null;
        StateChanged?.Invoke();
    }

    private void OnEnable()
    {
        StateChanged?.Invoke();
    }

    private void OnDisable()
    {
        SelectedTicket = null;
        LastConfirmedTicket = null;
        StateChanged?.Invoke();
    }
}
