using BlazorBootstrap;

namespace VentasPOST.ServiceClient;

public class NotificationService(ToastService toastService)
{
    public void Notification(ToastType toastType, string tittle, string message) => messages.Add(ShowToast(toastType, tittle, message));
    public void Notification(ToastType toastType, string message) => messages.Add(ShowToast(toastType, message));
    public void Notification(ToastType toastType, string tittle, string message, string icon) => messages.Add(ShowToast(toastType, tittle, message, icon));

    private List<ToastMessage> messages = new List<ToastMessage>();

    private ToastMessage ShowToast(ToastType toastType, string tittle, string message)
    {
        var mensaje = new ToastMessage()
        {
            Type = toastType,
            Title = tittle,
            HelpText = $"{DateTime.Now.ToString("dd/MM/yyyy")}",
            Message = $"{message}. A las {DateTime.Now.ToString("hh:mm tt")}",
        };
        toastService.Notify(mensaje);
        return mensaje;
    }
    private ToastMessage ShowToast(ToastType toastType, string message)
    {
        var mensaje = new ToastMessage()
        {
            Type = toastType,
            Message = $"{message}. El {DateTime.Now.ToString("dd/MM/yyyy")} a las {DateTime.Now.ToString("hh:mm tt")}"
        };
        toastService.Notify(mensaje);
        return mensaje;
    }

    private ToastMessage ShowToast(ToastType toastType, string tittle, string message, string icon)
    {
        var mensaje = new ToastMessage()
        {
            Type = toastType,
            Title = tittle,
            HelpText = $"{DateTime.Now.ToString("dd/MM/yyyy")}",
            Message = $"{message}. A las {DateTime.Now.ToString("hh:mm tt")}",
            CustomIconName = icon
        };
        toastService.Notify(mensaje);
        return mensaje;
    }
}