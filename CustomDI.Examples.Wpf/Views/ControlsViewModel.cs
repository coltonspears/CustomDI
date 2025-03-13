using CustomDI.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomDI.Examples.Wpf.Views
{
    /// <summary>
    /// ViewModel for the ControlsView
    /// </summary>
    public class ControlsViewModel : ViewModelBase
    {
        private ObservableCollection<SampleDataItem> _sampleData;
        public ObservableCollection<SampleDataItem> SampleData
        {
            get => _sampleData;
            set => SetProperty(ref _sampleData, value, nameof(SampleData));
        }

        public ControlsViewModel()
        {
            // Initialize sample data for the DataGrid
            SampleData = new ObservableCollection<SampleDataItem>
            {
                new SampleDataItem { Id = 1, Name = "Item 1", Description = "Description for item 1", IsActive = true },
                new SampleDataItem { Id = 2, Name = "Item 2", Description = "Description for item 2", IsActive = false },
                new SampleDataItem { Id = 3, Name = "Item 3", Description = "Description for item 3", IsActive = true },
                new SampleDataItem { Id = 4, Name = "Item 4", Description = "Description for item 4", IsActive = false },
                new SampleDataItem { Id = 5, Name = "Item 5", Description = "Description for item 5", IsActive = true }
            };
        }
    }

    /// <summary>
    /// Sample data item for the DataGrid
    /// </summary>
    public class SampleDataItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
