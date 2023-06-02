using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RCat_Controller
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditAction : ContentPage
    {
        Action a = new Action();
        string args = "";
        int index = 0;
        public EditAction(Action action, string arg)
        {
            InitializeComponent();
            args = arg;
            index = Action.AllActions.IndexOf(action);
            if (arg == "Edit")
            {
                a = action.CopyAction();
                
            }
            BindingContext = a;
        }
        private async void BtnSaveClicked(object sender, EventArgs e)
        {
            if (CheckAction())
            {
                if (args == "Edit")
                {
                    Action.AllActions[index] = a.CopyAction();
                    await Navigation.PopAsync();
                }
                else
                {
                    Action.AllActions.Add(a.CopyAction());
                    await Navigation.PopAsync();
                }
            }
            else
            {
                await DisplayAlert("Unable to save", "Some values are inappropriate!", "Ok");
            }
        }
        public bool CheckAction()
        {
            return a.Name != null && a.Name != string.Empty && a.Description != null && a.Description != string.Empty;
        }
    }
}