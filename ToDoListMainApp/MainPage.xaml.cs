using Microsoft.Maui.ApplicationModel;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace ToDoListMainApp
{
    public partial class MainPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        public ObservableCollection<TaskItem> TaskList { get; set; }


        public MainPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            TaskList = new ObservableCollection<TaskItem>();
            TaskListView.ItemsSource = TaskList;

            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                TaskAdd.IsVisible = false;
            }
            else
            {
                TaskAdd.IsVisible = true;
            }
        }

        private void ThemeButton(object sender, EventArgs e)
        {
            if(Application.Current.RequestedTheme == AppTheme.Dark)
            {
                Application.Current.UserAppTheme = AppTheme.Light;
            }
            else
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
        }
        private async void OnSubmitButtonClicked(object sender, EventArgs e)
        {
            string taskText = TaskEntry.Text;

            if (!string.IsNullOrWhiteSpace(taskText))
            {
                var taskData = new { name = " " + taskText, isComplete = false };
                var json = JsonConvert.SerializeObject(taskData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string apiUrl = "http://localhost:5160/api/TodoItems";
                if (DeviceInfo.Current.Platform == DevicePlatform.Android)
                {
                    apiUrl = "http://10.0.2.2:5160/api/TodoItems";
                }


                try
                {
                    var response = await _httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        TaskEntry.Text = string.Empty;
                        var newTask = JsonConvert.DeserializeObject<TaskItem>(json);
                        TaskList.Add(newTask);
                        await LoadTasksAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to submit task.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK" + apiUrl);
                }
            }
            else
            {
                await DisplayAlert("Validation", "Please enter a task.", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTasksAsync();
        }

        private async Task LoadTasksAsync()
        {
            string apiUrl = "http://localhost:5160/api/TodoItems";
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                apiUrl = "http://10.0.2.2:5160/api/TodoItems";
            }
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tasks = JsonConvert.DeserializeObject<List<TaskItem>>(json);

                    TaskList.Clear();
                    foreach (var task in tasks)
                    {
                        TaskList.Add(task);
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Failed to load tasks.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
        private async void OnTaskCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var task = checkBox?.BindingContext as TaskItem;
            if (task != null)
            {
                task.IsComplete = e.Value;
                UpdateTaskAsync(task);
            }
        }

        private async Task UpdateTaskAsync(TaskItem task)
        {
            var taskData = new { id = task.Id, name = task.Name, isComplete = task.IsComplete };
            var json = JsonConvert.SerializeObject(taskData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string apiUrl = $"http://localhost:5160/api/TodoItems/{task.Id}";
            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                apiUrl = $"http://10.0.2.2:5160/api/TodoItems/{task.Id}";
            }

            try
            {
                var response = await _httpClient.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {

                }
                else
                {
                    await DisplayAlert("Error", "Failed to update task status.", "OK");
                    task.IsComplete = !task.IsComplete;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                task.IsComplete = !task.IsComplete;
            }
        }

        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var task = button?.BindingContext as TaskItem;

            if (task != null)
            {
                string apiUrl = $"http://localhost:5160/api/TodoItems/{task.Id}";
                if (DeviceInfo.Current.Platform == DevicePlatform.Android)
                {
                    apiUrl = $"http://10.0.2.2:5160/api/TodoItems/{task.Id}";
                }
                try
                {
                    var response = await _httpClient.DeleteAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        TaskList.Remove(task);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to delete the task.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                }
            }
        }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
