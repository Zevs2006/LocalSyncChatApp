using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalSyncChatApp
{
    public partial class MainPage : ContentPage
    {
        private const int Port = 8888; // Порт для связи
        private UdpClient udpClient;
        private ObservableCollection<MessageModel> messages = new(); // Коллекция сообщений для UI
        private string userName = "Гость";

        public MainPage()
        {
            InitializeComponent();

            // Привязка сообщений к списку в интерфейсе
            MessageList.ItemsSource = messages;

            // Запуск прослушивания входящих сообщений
            StartListening();
        }

        // Сохранение имени пользователя
        private void SaveUserInfo(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(UserNameEntry.Text))
            {
                userName = UserNameEntry.Text.Trim();
                DisplayAlert("Информация", $"Имя пользователя сохранено: {userName}", "OK");
            }
        }

        // Отправка сообщения
        private async void SendMessage(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MessageEntry.Text))
            {
                string message = $"{userName}: {MessageEntry.Text}"; // Форматирование сообщения
                messages.Add(new MessageModel { Message = message }); // Добавление в локальный список
                MessageEntry.Text = string.Empty; // Очистка поля ввода
                await SendMessageToNetwork(message); // Отправка сообщения по сети
            }
        }

        // Отправка сообщений в локальную сеть через UDP
        private async Task SendMessageToNetwork(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await udpClient.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, Port)); // Широковещательная отправка
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось отправить сообщение: {ex.Message}", "ОК");
            }
        }

        // Прослушивание сообщений от других пользователей
        private async void StartListening()
        {
            try
            {
                udpClient = new UdpClient(Port) { EnableBroadcast = true }; // Включение широковещательного режима
                while (true)
                {
                    var result = await udpClient.ReceiveAsync(); // Ожидание сообщения
                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer); // Декодирование сообщения
                    messages.Add(new MessageModel { Message = receivedMessage }); // Добавление в список сообщений
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Ошибка приема сообщений: {ex.Message}", "ОК");
            }
        }
    }

    // Модель данных для отображения сообщений
    public class MessageModel
    {
        public string Message { get; set; }
    }
}
