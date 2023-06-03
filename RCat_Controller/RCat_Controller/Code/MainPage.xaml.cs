using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Essentials;
using static Xamarin.Essentials.AppleSignInAuthenticator;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Timers;
using System.Threading;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;

namespace RCat_Controller
{
    public partial class MainPage : ContentPage
    {
        Orientation orientation = new Orientation();
        System.Threading.Timer timer;
        public  MainPage()
        {
            InitializeComponent();
            Title = "RCat Controller";
            BackgroundColor = Color.White;
            LbSensitivity.Text = sensitivitySlider.Value.ToString();
            ReadActionsFromFile();
        }
        public void UpdateActionsListView()
        {
            ActionsLW.ItemsSource = null;
            ActionsLW.ItemsSource = Action.AllActions;
        }

        public static void WriteActionsToFile()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Actions.txt")))
                {
                    foreach (var item in Action.AllActions)
                    {
                        sw.WriteLine(item.SerializeAction());
                    }
                    sw.Flush();
                }
            }
            catch (Exception) { }
        }
        public void ReadActionsFromFile()
        {
            try
            {
                string[] lines = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Actions.txt"));
                foreach (var item in lines)
                {
                    Action.AllActions.Add(Action.DeserializeAction(item));
                }
                UpdateActionsListView();
            }
            catch (Exception) { }
        }


        private async void BtnAddClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditAction(new Action(), "", false));
        }
        private async void BtnActionClicked(object sender, EventArgs e)
        {
            Xamarin.Forms.Button button = (Xamarin.Forms.Button)sender;
            Action a = Action.AllActions[Action.AllActions.IndexOf((Action)button.BindingContext)];
            if (CBEnableEditing.IsChecked)
            {
                await Navigation.PushAsync(new EditAction(a, "Edit", true));
            }
            else
            {
                ConnectAndPublish(a.SerializeAction());
            }
        }

        private void BtnMoveUpPressed(object sender, EventArgs e)
        {
            timer = new System.Threading.Timer(OnBtnUpTimerElapsed, null, 0, 200);
        }
        private void BtnMoveUpReleased(object sender, EventArgs e)
        {
            timer.Dispose();
        }
        private void OnBtnUpTimerElapsed(object state)
        {
            if (BtnMoveUp.IsPressed)
            {
                orientation.Movement = 1 * (int)sensitivitySlider.Value;
                orientation.Sensitivity = (int)sensitivitySlider.Value;
                ConnectAndPublish(orientation.SerializeOrientation());
            }
        }

        private void BtnRotationLeftPressed(object sender, EventArgs e)
        {
            timer = new System.Threading.Timer(OnBtnLeftTimerElapsed, null, 0, 200);
        }
        private void BtnRotationLeftReleased(object sender, EventArgs e)
        {
            timer.Dispose();
        }
        private void OnBtnLeftTimerElapsed(object state)
        {
            if (BtnRotateLeft.IsPressed)
            {
                orientation.Rotation -= 1 * (int)sensitivitySlider.Value;
                orientation.Sensitivity = (int)sensitivitySlider.Value;
                ConnectAndPublish(orientation.SerializeOrientation());
            }
        }

        private void BtnRotationRightPressed(object sender, EventArgs e)
        {
            timer = new System.Threading.Timer(OnBtnRightTimerElapsed, null, 0, 200);
        }
        private void BtnRotationRightReleased(object sender, EventArgs e)
        {
            timer.Dispose();
        }
        private void OnBtnRightTimerElapsed(object state)
        {
            if (BtnRotateRight.IsPressed)
            {
                orientation.Rotation += 1 * (int)sensitivitySlider.Value;
                orientation.Sensitivity = (int)sensitivitySlider.Value;
                ConnectAndPublish(orientation.SerializeOrientation());
            }
        }

        private void BtnMoveDownPressed(object sender, EventArgs e)
        {
            timer = new System.Threading.Timer(OnBtnDownTimerElapsed, null, 0, 200);
        }
        private void BtnMoveDownReleased(object sender, EventArgs e)
        {
            timer.Dispose();
        }
        private void OnBtnDownTimerElapsed(object state)
        {
            if (BtnMoveDown.IsPressed)
            {
                orientation.Movement = -1 * (int)sensitivitySlider.Value;
                orientation.Sensitivity = (int)sensitivitySlider.Value;
                ConnectAndPublish(orientation.SerializeOrientation());
            }
        }

        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            int value = (int)sensitivitySlider.Value;
            LbSensitivity.Text = value.ToString();
        }

        public async void ConnectAndPublish(string textToSend)
        {
            var factory = new MqttFactory();
            var client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("test.mosquitto.org", 1883)
                .Build();

            await client.ConnectAsync(options);

            // Publish a message
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("RCat/Pepa")
                .WithPayload(textToSend)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await client.PublishAsync(message);

            await client.DisconnectAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            WriteActionsToFile();
            UpdateActionsListView();
        }
    }

    public class Action
    {
        [JsonIgnore]
        public static List<Action> AllActions = new List<Action>();
        public Guid Id { get; set; }
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
            }
        }
        private string description;
        public string Description
        {
            get => description;
            set
            {
                description = value;
            }
        }
        public Action() { Id = Guid.NewGuid(); }
        public Action(Guid ID, string aName, string aDescription)
        {
            Id = ID;
            Name = aName;
            Description = aDescription;
        }
        public Action CopyAction() => new Action(Id, Name, Description);
        public string SerializeAction() => JsonSerializer.Serialize(this);
        public static Action DeserializeAction(string json) 
        {
            Action a = JsonSerializer.Deserialize<Action>(json);
            a.Id = Guid.NewGuid();
            return a.CopyAction();
        }
    }
    class Orientation
    {
        private int rotation;
        public int Rotation { 
            get => rotation; 
            set 
            {
                if (value >= 360)
                {
                    rotation = value % 360;
                }
                else if (value <= -1)
                {
                    rotation = 360 + value;
                }
                else
                {
                    rotation = value;
                }
                Movement = 0;
            } 
        }

        private int movement;
        public int Movement { get { return movement; } set {  movement = value; } }

        private int sensitivity;
        public int Sensitivity { get { return sensitivity; } set { sensitivity = value; } }

        public Orientation() { rotation = 0; Movement = 0; }
        
        public string SerializeOrientation() => JsonSerializer.Serialize(this);
    }
}
