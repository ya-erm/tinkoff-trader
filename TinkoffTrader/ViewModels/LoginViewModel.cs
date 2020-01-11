using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using TinkoffTraderCore;

namespace TinkoffTrader.ViewModels
{
    internal class LoginViewModel : ABaseViewModel
    {
        /// <summary>
        /// True, если обнаружен файл токена
        /// </summary>
        public bool HasTokenFile { get => Get<bool>(); set => Set(value); }

        /// <summary>
        /// True, если токен загружен
        /// </summary>
        public bool TokenLoaded { get => Get<bool>(); set => Set(value); }

        /// <summary>
        /// Токен авторизации
        /// </summary>
        public string Token { get => Get<string>(); set => Set(value); }

        /// <summary>
        /// Информационное сообщение
        /// </summary>
        public string Info { get => Get<string>(); set => Set(value); }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        public string Error { get => Get<string>(); set => Set(value); }
        
        /// <summary>
        /// Текст команды
        /// </summary>
        [DependsOn(nameof(HasTokenFile))]
        public string CommandText => HasTokenFile ? "Загрузить" : "Сохранить";
        
        /// <summary>
        /// Команда
        /// </summary>
        [DependsOn(nameof(HasTokenFile))]
        public ICommand Command => HasTokenFile ? new BaseCommand(LoadAction) : new BaseCommand(SaveAction);


        [DependsOn(nameof(Info))] 
        public bool ShowInfo => !string.IsNullOrEmpty(Info);


        [DependsOn(nameof(HasTokenFile), nameof(TokenLoaded))] 
        public bool ShowToken => !HasTokenFile || TokenLoaded;


        [DependsOn(nameof(HasTokenFile))] 
        public bool ShowTips => !HasTokenFile;


        [DependsOn(nameof(Error))] 
        public bool ShowError => !string.IsNullOrEmpty(Error);


        [DependsOn(nameof(TokenLoaded))] 
        public bool ShowButton => !TokenLoaded;

        #region .ctor

        public LoginViewModel()
        {
            var token = TokenStorage.LoadToken();

            HasTokenFile = TokenStorage.HasTokenFile;

            if (HasTokenFile)
            {
                if (TokenStorage.IsTokenFileEncrypted)
                {
                    Info = "Введите пароль для расшифровки файла токена";
                }
                else
                {
                    Token = TokenStorage.LoadToken();
                    TokenLoaded = true;
                }
            }
            else
            {
                Info = "Вставьте токен и придумайте пароль для шифрования файла";
            }
        }

        #endregion

        private void SaveAction(object parameter)
        {
            if (string.IsNullOrEmpty(Token))
            {
                Error = "Токен авторизации не должен быть пустым";
                
                return;
            }

            Error = null;

            if (!(parameter is PasswordBox passwordBox)) return;

            var password = passwordBox.Password;

            if (!string.IsNullOrEmpty(password))
            {
                TokenStorage.SaveTokenEncrypted(Token, password);
            }
            else
            {
                TokenStorage.SaveToken(Token);
            }

            passwordBox.Clear();

            TokenLoaded = true;
            ShowInfoTemporary("Токен успешно сохранён");
        }

        private void LoadAction(object parameter)
        {
            if (!(parameter is PasswordBox passwordBox)) return;

            var password = passwordBox.Password;

            Token = !string.IsNullOrEmpty(password) 
                ? TokenStorage.LoadTokenEncrypted(password) 
                : TokenStorage.LoadToken();

            passwordBox.Clear();

            TokenLoaded = true;
            ShowInfoTemporary("Токен успешно загружен");
        }
        
        private void ShowInfoTemporary(string text, TimeSpan? duration = null)
        {
            Info = text;

            var timer = new Timer((_) => Info = null);
            timer.Change(duration ?? TimeSpan.FromSeconds(3), TimeSpan.Zero);
        }

    }
}
