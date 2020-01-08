using MQTTnet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Threading;
using MQTTnet.Server;
using MQTTnet.Protocol;
using MQTTnet.Client.Disconnecting;

namespace MQTT_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IMqttServer mqttServer;

        MqttServerOptionsBuilder optionsBuilder = new MqttServerOptionsBuilder();

        List<string> clientSession = new List<string>();

        MqttServerConnectionValidatorDelegate connValidator = new MqttServerConnectionValidatorDelegate(c =>
        {
            if (c.ClientId.Length < 10)
            {
                c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedIdentifierRejected;
                return;
            }

            if (c.Username != "mySecretUser")
            {
                c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                return;
            }

            if (c.Password != "mySecretPassword")
            {
                c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                return;
            }

            c.ReturnCode = MqttConnectReturnCode.ConnectionAccepted;
        });

        public MainWindow()
        {
            InitializeComponent();

            // Configure MQTT server.
            //mqttServer = new MqttFactory().CreateMqttServer(options =>
            //{
            //    options.DefaultEndpointOptions.Port = 1884;
            //    //options.Storage = new RetainedMessageHandler();
            //    options.ConnectionValidator = c =>
            //    {
            //        if (c.ClientId.Length < 10)
            //        {
            //            return MqttConnectReturnCode.ConnectionRefusedIdentifierRejected;
            //        }

            //        //if (c.Username != "mySecretUser")
            //        //{
            //        //    return MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
            //        //}

            //        //if (c.Password != "mySecretPassword")
            //        //{
            //        //    return MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
            //        //}

            //        return MqttConnectReturnCode.ConnectionAccepted;
            //    };
            //});            

            optionsBuilder = new MqttServerOptionsBuilder()
                .WithConnectionBacklog(100)
                //.WithConnectionValidator(connValidator)
                .WithDefaultEndpointPort(60000);           

            mqttServer = new MqttFactory().CreateMqttServer();

            mqttServer.StartAsync(optionsBuilder.Build());
         
            mqttServer.UseApplicationMessageReceivedHandler(async e =>
            {
                string receivedMqtt = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                if (e.ApplicationMessage.Topic.StartsWith("Senzo/Mobile/Client/"))
                {
                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE FROM MOBILE ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine();
                    await Application.Current.Dispatcher.BeginInvoke(
                             DispatcherPriority.Background,
                             new Action(() =>
                             {
                                 txtMessage.Text = "";
                                 txtMessage.Text += "### RECEIVED APPLICATION MESSAGE ###";
                                 txtMessage.Text += "$ Topic = " + e.ApplicationMessage.Topic;
                                 txtMessage.Text += "$ Payload = " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                                 txtMessage.Text += "$ QoS = " + e.ApplicationMessage.QualityOfServiceLevel;
                                 txtMessage.Text += "$ Retain = " + e.ApplicationMessage.Retain;
                             })
                    );

                    var message2 = new MqttApplicationMessageBuilder();
                    //.WithTopic("Senzo/Server/+")
                    //.WithPayload("Receive And Forward Message")
                    //.WithExactlyOnceQoS()
                    //.WithRetainFlag()
                    //.Build();
                    //foreach (string clientId in clientSession) {
                    //    await mqttServer.PublishAsync(message2.WithTopic("Senzo/Gateway/Server/" +clientId)
                    //        .WithPayload(receivedMqtt)
                    //        .WithExactlyOnceQoS()
                    //        .WithRetainFlag()
                    //        .Build());
                    //}
                    //Console.WriteLine();
                }
                else if (e.ApplicationMessage.Topic.StartsWith("Senzo/Gateway/Client/"))
                {
                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE FROM GATEWAY ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine();
                    await Application.Current.Dispatcher.BeginInvoke(
                             DispatcherPriority.Background,
                             new Action(() =>
                             {
                                 txtMessage.Text = "";
                                 txtMessage.Text += "### RECEIVED APPLICATION MESSAGE ###";
                                 txtMessage.Text += "$ Topic = " + e.ApplicationMessage.Topic;
                                 txtMessage.Text += "$ Payload = " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                                 txtMessage.Text += "$ QoS = " + e.ApplicationMessage.QualityOfServiceLevel;
                                 txtMessage.Text += "$ Retain = " + e.ApplicationMessage.Retain;
                             })
                    );



                    var message = new MqttApplicationMessageBuilder();
                    //.WithTopic("Senzo/Server/+")
                    //.WithPayload("Receive And Forward Message")
                    //.WithExactlyOnceQoS()
                    //.WithRetainFlag()
                    //.Build();

                    //foreach (string clientId in clientSession)
                    //{
                    //    await mqttServer.PublishAsync(message.WithTopic("Senzo/Gateway/Server/" + clientId)
                    //        .WithPayload(receivedMqtt)
                    //        .WithExactlyOnceQoS()
                    //        .WithRetainFlag()
                    //        .Build());
                    //}

                    //Console.WriteLine();
                }
                else if ((string.Equals(e.ApplicationMessage.Topic, "test/topic")))
                {
                    Console.WriteLine("### IGNORED APPLICATION MESSAGE FROM GATEWAY ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine();

                   
                } else if (e.ApplicationMessage.Topic.StartsWith("MQTTnet.RPC") && e.ApplicationMessage.Topic.EndsWith("/Reply") && e.ClientId !=null) {
                    var message1 = new MqttApplicationMessageBuilder();
                    
                    Console.WriteLine("### RPC APPLICATION MESSAGE FROM GATEWAY ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine($"+ ContentType = {e.ApplicationMessage.ContentType}");
                    Console.WriteLine();
                  
                    await mqttServer.PublishAsync(message1.WithTopic(e.ApplicationMessage.Topic+ "/response")
                                .WithPayload("TestResponse")
                                .WithExactlyOnceQoS()
                                .Build());
                }
                else {
                    Console.WriteLine("### IGNORED APPLICATION MESSAGE FROM GATEWAY ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine();


                }

            });

            mqttServer.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(e => OnDisconnected(e));

            mqttServer.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(e => OnConnected(e));

            //mqttServer.ApplicationMessageReceivedHandler = new Mqtt(e => OnAppMessage(e));

            //mqttServer.UseClientConnectedHandler(e =>
            //    {
            //        Console.WriteLine("Client Connected:"+e.ClientId);
            //        clientSession.Add(e.ClientId);
            //    });

            //mqttServer.UseClientDisconnectedHandler(e =>
            //{
            //    Console.WriteLine("Client Disconnected:"+e.ClientId);
            //    clientSession.Remove(e.ClientId);
            //});
        }

        private void OnConnected(MqttServerClientConnectedEventArgs e)
        {
            Console.WriteLine("Client Connected:" + e.ClientId);
            clientSession.Add(e.ClientId);
        }

        private void OnDisconnected(MqttServerClientDisconnectedEventArgs e)
        {
            Console.WriteLine("Client Disconnected:" + e.ClientId);
            clientSession.Remove(e.ClientId);
        }

        private void OnDisconnected(MqttClientDisconnectedEventArgs e)
        {
            throw new NotImplementedException();
        }

        //MqttServerApplicationMessageInterceptorDelegate asd = new MqttServerApplicationMessageInterceptorDelegate(e =>
        //  {
        //      if (e.ApplicationMessage.Topic == "message")
        //      {
        //          Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
        //          Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
        //          Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
        //          Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
        //          Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
        //          Console.WriteLine();
        //          Application.Current.Dispatcher.BeginInvoke(
        //                   DispatcherPriority.Background,
        //                   new Action(() =>
        //                   {
        //                       txtMessage.Text = "";
        //                       txtMessage.Text += "### RECEIVED APPLICATION MESSAGE ###\n";
        //                       txtMessage.Text += "$ Topic = " + e.ApplicationMessage.Topic + "\n";
        //                       txtMessage.Text += "$ Payload = " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload) + "\n";
        //                       txtMessage.Text += "$ QoS = " + e.ApplicationMessage.QualityOfServiceLevel + "\n";
        //                       txtMessage.Text += "$ Retain = " + e.ApplicationMessage.Retain + "\n";
        //                   })
        //          );
        //          //Console.WriteLine();
        //      }
        //  } );
        private void MqttServer_ClientConnected(object sender, MqttServerClientConnectedEventArgs e)
        {
            Console.WriteLine(e.ClientId);
        }

        private void MqttServer_ApplicationMessageReceived(object s, MqttApplicationMessageReceivedEventArgs e)
        {
            if(e.ApplicationMessage.Topic == "message")
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
                Application.Current.Dispatcher.BeginInvoke(
                         DispatcherPriority.Background,
                         new Action(() =>
                         {
                             txtMessage.Text = "";
                             txtMessage.Text += "### RECEIVED APPLICATION MESSAGE ###\n";
                             txtMessage.Text += "$ Topic = " + e.ApplicationMessage.Topic + "\n";
                             txtMessage.Text += "$ Payload = " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload) + "\n";
                             txtMessage.Text += "$ QoS = " + e.ApplicationMessage.QualityOfServiceLevel + "\n";
                             txtMessage.Text += "$ Retain = " + e.ApplicationMessage.Retain + "\n";
                         })
                );
                //Console.WriteLine();
            }
        }

        public async void StartServer()
        {
            optionsBuilder = new MqttServerOptionsBuilder()
                .WithConnectionBacklog(100)
                //.WithConnectionValidator(connValidator)
                .WithDefaultEndpointPort(1884);
            await mqttServer.StartAsync(optionsBuilder.Build());
        }

        public async void StopServer()
        {
            await mqttServer.StopAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartServer();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StopServer();
        }
    }


    // The implementation of the storage:
    // This code uses the JSON library "Newtonsoft.Json".
    public class RetainedMessageHandler : IMqttServerStorage
    {
        private const string Filename = "D:\\RetainedMessages.json";

        public Task SaveRetainedMessagesAsync(IList<MqttApplicationMessage> messages)
        {
            File.WriteAllText(Filename, JsonConvert.SerializeObject(messages));
            return Task.FromResult(0);
        }

        public Task<IList<MqttApplicationMessage>> LoadRetainedMessagesAsync()
        {
            IList<MqttApplicationMessage> retainedMessages;
            if (File.Exists(Filename))
            {
                var json = File.ReadAllText(Filename);
                retainedMessages = JsonConvert.DeserializeObject<List<MqttApplicationMessage>>(json);
            }
            else
            {
                retainedMessages = new List<MqttApplicationMessage>();
            }

            return Task.FromResult(retainedMessages);
        }
        
    }

}
