using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Receiver_2.Services;

//Ela precisa ser um BACKGROUND SERVICES para poder rodar em segundo plano
//para sempre que receber uma mensagem ela rodar esse codigo
public class ReceiverServices : BackgroundService
{
    private readonly string _queueName;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public ReceiverServices(IConfiguration configuration)
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

        //criando uma queue caso nao exista
        _channel.QueueDeclare(queue: _queueName,exclusive: false,autoDelete: false);

        _channel.QueueBind(queue: _queueName,exchange: "Test",routingKey: "1");

        //Vai configurar quantas mensagens esse RECEIVER vai receber e trator por vez
        //nesse caso ele vai receber 1 mensagem por vez;
        _channel.BasicQos(0, 1, false);
    }


    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //Estamos criando alguem que vai consumir esse channel
        var consumer = new EventingBasicConsumer(_channel);

        //toda vez que esse canal receber alguma mensagem de uma queue ele
        //vai chamar esse metodo e rodar ele para cada mensagem.
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Message Received QUEUE 1: ({DateTime.Now}) - {message}");

            //Vai dizer que a mensagem foi processada com sucesso;
            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }
}
