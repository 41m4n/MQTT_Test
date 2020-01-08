using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.Rpc;
using MQTTnet.Protocol;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MQTT_TestClient_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string clientId;

        int retryConnect = 0;

        IMqttClient mqttClient;

        MqttRpcClient rpcClient;

        TimeSpan timeout = TimeSpan.FromSeconds(5);

        MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtMostOnce;

        public MainWindow()
        {
            InitializeComponent();
            // Create a new MQTT client.
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            clientId = Guid.NewGuid().ToString();

            mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("IsConnected:" + mqttClient.IsConnected);
                retryConnect = 0;
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                var message = new MqttApplicationMessageBuilder()
                           .WithTopic("test/topic")
                           .WithPayload("test")
                           .WithExactlyOnceQoS()
                           //.WithRetainFlag()
                           .Build();

                //for (int i= 0; i < 50; i++) {
                //await mqttClient.PublishAsync(message);
                // Subscribe to a topic
               await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(" anonymous/Server").Build());

                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("GatewayIP/#").Build());
                Console.WriteLine("### SUBSCRIBED ###");
            });

            //mqttClient.UnsubscribeAsync();

            //mqttClient.UseApplicationMessageReceivedHandler(async e =>
            //{
            //    if (e.ApplicationMessage.Topic == "/anonymous/Server/ClientMob")
            //    {
            //        Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            //        Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            //        Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            //        Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            //        Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            //        Console.WriteLine();
            //        await Application.Current.Dispatcher.BeginInvoke(
            //                 DispatcherPriority.Background,
            //                 new Action(() =>
            //                 {
            //                     txtMessage.Text = "";
            //                     txtMessage.Text += "### RECEIVED APPLICATION MESSAGE ###";
            //                     txtMessage.Text += "$ Topic = " + e.ApplicationMessage.Topic;
            //                     txtMessage.Text += "$ Payload = " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            //                     txtMessage.Text += "$ QoS = " + e.ApplicationMessage.QualityOfServiceLevel;
            //                     txtMessage.Text += "$ Retain = " + e.ApplicationMessage.Retain;
            //                 })
            //        );
            //        //Console.WriteLine();
            //    }
            //    else
            //    {
            //        Console.WriteLine("### IGNORED APPLICATION MESSAGE ###");
            //        Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            //        Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            //        Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            //        Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            //        Console.WriteLine();
            //    }
            //});

            mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(async e =>
            {
                Console.WriteLine("IsConnected:"+mqttClient.IsConnected);
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine("MQTT User "+clientId+" received from:" + e.ClientId);
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
               // await mqttClient.PublishAsync(e.ApplicationMessage.Topic + "/response", "pong");
            });
        }

        //void mqtt_Callback(string topic, string payload, int payloadLength)
        //{
        //    string topicString = topic;

        //    if (topicString.StartsWith("MQTTnet.RPC/"))
        //    {
        //        string responseTopic = topicString + ("/response");

        //        if (topicString.EndsWith("/deviceA.ping"))
        //        {
        //            mqtt_publish(responseTopic, "pong", false);
        //            return;
        //        }
        //    }
        //}

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            // Use WebSocket connection.
            var options = new MqttClientOptionsBuilder()
                //.WithWebSocketServer("192.168.1.106:1883/mqtt") 
                .WithClientId("123")
                .WithTcpServer("localhost", 60000)
                .WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    CertificateValidationCallback = (X509Certificate x, X509Chain y, SslPolicyErrors z, IMqttClientOptions o) =>
                    {
                        // TODO: Check conditions of certificate by using above parameters.
                        return true;
                    }
                })
                .Build();

            mqttClient.UseDisconnectedHandler(async c =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    //if (retryConnect > 3)
                    //{
                    //    await mqttClient.DisconnectAsync();
                    //}
                    //else
                    //{
                    //    Console.WriteLine("IsConnected:" + mqttClient.IsConnected);
                    //    //if (retryConnect > 3)
                    //    //{
                    //    //    Console.WriteLine("Exiting Program");
                    //    //}
                    //    //else
                    //    //{
                    //    //    Console.WriteLine("Retry");
                    //    await mqttClient.ConnectAsync(options);
                    //    //}
                    //    //retryConnect++;
                    //    //Console.WriteLine("asda:"+c.Exception.ToString());
                    //}
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                    retryConnect++;
                    //Console.WriteLine("asda:" + c.Exception.ToString());
                }
            });

            try
            {
                await mqttClient.ConnectAsync(options);

                //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("MQTTnet.RPC/+/Reply").Build());

                rpcClient = new MqttRpcClient(mqttClient);

            }
            catch (Exception ex) {
                Console.WriteLine("Error When Connecting:" + ex);
            }
            
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
                                .WithTopic(txtTopic.Text)
                                .WithPayload(txtMessage.Text)
                                .WithExactlyOnceQoS()
                                //.WithRetainFlag()
                                .Build();

                //for (int i= 0; i < 50; i++) {
                //await mqttClient.PublishAsync(message);

                var response = await rpcClient.ExecuteAsync(timeout, "Reply", "testRPC", qos);

                Console.WriteLine("RPC Response: "+Encoding.UTF8.GetString(response));


                //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("MQTTnet.RPC/+/Response").Build());
            }
            catch (Exception ex) {
                Console.WriteLine("Error When Sending RPC:" + ex);            }
            //}

            
        }

        protected virtual void OnAppMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage.Topic == "/anonymous/Server")
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
                             txtMessage.Text += "### RECEIVED APPLICATION MESSAGE ###";
                             txtMessage.Text += "$ Topic = " + e.ApplicationMessage.Topic;
                             txtMessage.Text += "$ Payload = " + Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                             txtMessage.Text += "$ QoS = " + e.ApplicationMessage.QualityOfServiceLevel;
                             txtMessage.Text += "$ Retain = " + e.ApplicationMessage.Retain;
                         })
                );
                //Console.WriteLine();
            }
        }
    }
}
