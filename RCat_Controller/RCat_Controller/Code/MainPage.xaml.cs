﻿using System;
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

namespace RCat_Controller
{
    public partial class MainPage : ContentPage
    {
        Orientation orientation = new Orientation();
        public  MainPage()
        {
            InitializeComponent();
            Title = "RCat Controller";
            BackgroundColor = Color.White;
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
            await Navigation.PushAsync(new EditAction(new Action(), ""));
        }
        private async void BtnActionClicked(object sender, EventArgs e)
        {
            Xamarin.Forms.Button button = (Xamarin.Forms.Button)sender;
            Action a = Action.AllActions[Action.AllActions.IndexOf((Action)button.BindingContext)];
            if (CBEnableEditing.IsChecked)
            {
                await Navigation.PushAsync(new EditAction(a, "Edit"));
            }
            else
            {
                ConnectAndPublish(a.SerializeAction());
            }
        }
        private void BtnMoveUpClicked(object sender, EventArgs e)
        {
            orientation.Movement = 1;
            ConnectAndPublish(orientation.SerializeOrientation());
        }
        private void BtnRotationLeftClicked(object sender, EventArgs e)
        {
            orientation.Rotation -= 1;
            ConnectAndPublish(orientation.SerializeOrientation());
        }
        private void BtnRotationRightClicked(object sender, EventArgs e)
        {
            orientation.Rotation += 1;
            ConnectAndPublish(orientation.SerializeOrientation());
        }
        private void BtnMoveDownClicked(object sender, EventArgs e)
        {
            orientation.Movement = -1;
            ConnectAndPublish(orientation.SerializeOrientation());
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
                if (value == 360)
                {
                    rotation = 0;
                }
                else if (value == -1)
                {
                    rotation = 359;
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

        public Orientation() { rotation = 0; Movement = 0; }
        
        public string SerializeOrientation() => JsonSerializer.Serialize(this);
    }
}
