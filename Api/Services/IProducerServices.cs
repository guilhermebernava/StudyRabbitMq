namespace Sender.Services;

public interface IProducerServices
{
    bool SendMessage(string message);
}
