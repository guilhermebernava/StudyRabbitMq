using RabbitMQ.Client;
using System.Text;

namespace Sender.Services;

public class ProducerServices : IProducerServices
{
    private readonly string _queueName;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public ProducerServices(IConfiguration configuration)
    {
        //Criando uma classe que vai conseguir gerar connections com o rabbitMq
        var connectionFactory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQConnection:HostName"],
            Port = int.Parse(configuration["RabbitMQConnection:Port"]),
            UserName = configuration["RabbitMQConnection:UserName"],
            Password = configuration["RabbitMQConnection:Password"],
            VirtualHost = configuration["RabbitMQConnection:VirtualHost"]
        };

        //definindo o nome da queue, lembrando que precisa
        //ser o mesmo lado do receiver
        _queueName = configuration["QueueName"];

        //criando uma connection de fato com o rabbitMQ
        _connection = connectionFactory.CreateConnection();

        //criando um canal para poder criar uma queue
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: "Test", type: ExchangeType.Fanout);
    }
    public bool SendMessage(string message)
    {
        try
        {
            //está pegando a mensagem e transformando em bytes para poder mandar para a QUEUE.
            var body = Encoding.UTF8.GetBytes(message);

            //está de fato enviando a mensagem para QUEUE
            _channel.BasicPublish(exchange: "Test", routingKey: _queueName, basicProperties: null, body: body);
            return true;
        }
        catch
        {
            return false;
        }

    }
}
