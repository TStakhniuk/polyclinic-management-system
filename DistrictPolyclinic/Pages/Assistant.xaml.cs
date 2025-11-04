using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DistrictPolyclinic.Chat;
using Newtonsoft.Json.Linq;

namespace DistrictPolyclinic.Pages
{
    public partial class Assistant : Page
    {
        private ObservableCollection<Message> messages;

        public Assistant()
        {
            InitializeComponent();
            messages = SessionManager.SessionMessages;

            if (messages.Count == 0)
            {
                messages.Add(new Message
                {
                    IsRequest = false,
                    Text = "Привіт! Як я можу допомогти вам сьогодні?"
                });
            }

            messagesItemsControl.ItemsSource = messages;
        }

        private async void messageTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;

                if (!string.IsNullOrWhiteSpace(messageTextBox.Text))
                    await SendUserMessageAsync(messageTextBox.Text.Trim());
            }
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(messageTextBox.Text))
                await SendUserMessageAsync(messageTextBox.Text.Trim());
        }

        private void MessagesItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            messagesScrollViewer?.ScrollToEnd();
        }


        private async Task SendUserMessageAsync(string userMessage)
        {
            try
            {
                var requestMessage = new Message { IsRequest = true, Text = userMessage };
                messages.Add(requestMessage);
                messageTextBox.Text = "";

                var assistantMessage = new Message { IsRequest = false, Text = "" };
                messages.Add(assistantMessage);

                messagesScrollViewer.ScrollToEnd();

                await GetAssistantResponseStreamingAsync(userMessage, assistantMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка");
            }
        }

        private async Task GetAssistantResponseStreamingAsync(string userMessage, Message assistantMessage)
        {
            try
            {
                string apiUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

                using (var client = new HttpClient { BaseAddress = new Uri(apiUrl) })
                {
                    var content = new StringContent(
                    $"{{\"model\": \"medical-assistant\", \"prompt\": \"{userMessage}\", \"stream\": true}}",
                    Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Post, "api/generate") { Content = content };
                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    if (!response.IsSuccessStatusCode)
                    {
                        assistantMessage.Text = "Сталась помилка при отриманні відповіді від асистента!";
                        return;
                    }

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            try
                            {
                                var json = JObject.Parse(line);
                                var part = json["response"]?.ToString() ?? json["text"]?.ToString();
                                if (!string.IsNullOrEmpty(part))
                                {
                                    assistantMessage.Text += part;
                                    assistantMessage.NotifyTextChanged();
                                    messagesScrollViewer.ScrollToEnd();
                                    await Task.Delay(5); // print effect
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                assistantMessage.Text = $"Помилка при зв'язку з сервером: {ex.Message}";
            }
        }

    }

    public class Message : INotifyPropertyChanged
    {
        private string _text;
        private bool _isRequest;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsRequest
        {
            get => _isRequest;
            set
            {
                _isRequest = value;
                OnPropertyChanged(nameof(IsRequest));
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public void NotifyTextChanged() => OnPropertyChanged(nameof(Text));

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
