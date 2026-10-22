using Unity.VisualScripting;
using UnityEngine;

public class TicketKeyboard : MonoBehaviour
{

    [SerializeField] private BotoneraManager botonera;

    private void Awake()
    {
        if (botonera != null) return;
        Debug.LogError("TicketMachineKeyboard: falta asignar Botonera.", this);
        enabled = false;
    }

    private void Update()
    {
        if (botonera == null || !botonera.IsAvailable) return;

        // Una sola orden por frame, aunque aprietes varias teclas juntas.
        if (Input.GetKeyDown(KeyCode.Keypad2)) botonera.ClearSelection();
        else if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.KeypadEnter))
            botonera.ConfirmSelection();
        else if (Input.GetKeyDown(KeyCode.Keypad7)) botonera.SelectTicket(TicketType.Corto);
        else if (Input.GetKeyDown(KeyCode.Keypad8)) botonera.SelectTicket(TicketType.Medio);
        else if (Input.GetKeyDown(KeyCode.Keypad9)) botonera.SelectTicket(TicketType.Largo);
        else if (Input.GetKeyDown(KeyCode.Keypad4)) botonera.SelectTicket(TicketType.Jubilado);
        else if (Input.GetKeyDown(KeyCode.Keypad5)) botonera.SelectTicket(TicketType.Discapacitado);
        else if (Input.GetKeyDown(KeyCode.Keypad6)) botonera.SelectTicket(TicketType.Escolar);
        else if (Input.GetKeyDown(KeyCode.Keypad1)) botonera.SelectTicket(TicketType.FreePass);
    }
}
