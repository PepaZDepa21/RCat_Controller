using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RCat_Controller
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Title = "RCat Controller";
            BackgroundColor = Color.White;
            List<Action> actions = new List<Action>() 
            { 
                new Action(Guid.NewGuid(), "Action1", "Print"), 
                new Action(Guid.NewGuid(), "Action2", "Prints"),
            };
            ActionsLW.ItemsSource = actions;
        }

    }

    public class Action
    {
        public Guid Id { get; set; }
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        private string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }
        public Action() { }
        public Action(Guid ID, string aName, string aDescription)
        {
            Id = ID;
            Name = aName;
            Description = aDescription;
        }
    }
}
