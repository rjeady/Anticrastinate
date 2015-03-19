using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfTesting.Properties;

namespace WpfTesting
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SessionEnding += (s, e) =>
            {
                MessageBox.Show("ok, clean up since we're shutting down");
            };

            WpfTesting.Properties.Settings.Default.Volume = 5;
            WpfTesting.Properties.Settings.Default.Save();
            

        }


    }

    internal class Testing
    {
        public Testing(Settings settings)
        {
            settings.Volume = 5;
        }
    }
}

namespace WpfTesting.Properties 
{
    // [SettingsProvider(typeof(MyProvider))]
    internal partial class Settings
    {

    }

    internal class MyProvider : SettingsProvider
    {
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            throw new NotImplementedException();
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }

}
