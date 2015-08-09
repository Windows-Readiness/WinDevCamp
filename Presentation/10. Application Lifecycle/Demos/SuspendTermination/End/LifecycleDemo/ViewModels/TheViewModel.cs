using LifecycleDemo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LifecycleDemo.ViewModels
{
    class TheViewModel : ViewModelBase
    {
        private string field1;
        public string Field1 { get { return field1; } set { Set(ref field1, value); } }

        private string field2;
        public string Field2 { get { return field2; } set { Set(ref field2, value); } }

        #region Methods for handling the apps permanent data
        public void LoadData()
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("TheData"))
            {
                MyDataItem data = JsonConvert.DeserializeObject<MyDataItem>(
                    (string)ApplicationData.Current.RoamingSettings.Values["TheData"]);
                Field1 = data.Field1;
                Field2 = data.Field2;
            }
            else
            {
                // New start, initialize the data
                Field1 = string.Empty;
                Field2 = string.Empty;
            }
        }

        public void SaveData()
        {
            MyDataItem data = new MyDataItem { Field1 = this.Field1, Field2 = this.Field2 };
            ApplicationData.Current.RoamingSettings.Values["TheData"] =
                JsonConvert.SerializeObject(data);
        }
        #endregion

    }
}
