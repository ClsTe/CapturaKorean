using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Captura.Views;
using FirstFloor.ModernUI.Windows.Controls;

namespace Captura.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MessageProvider : IMessageProvider
    {
        readonly IAudioPlayer _audioPlayer;

        public MessageProvider(IAudioPlayer AudioPlayer)
        {
            _audioPlayer = AudioPlayer;
        }

        public void ShowError(string Message, string Header = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new ModernDialog
                {
                    Title = LanguageManager.Instance.ErrorOccurred,
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = Header,
                                Margin = new Thickness(0, 0, 0, 10),
                                FontSize = 15
                            },

                            new ScrollViewer
                            {
                                Content = Message,
                                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                                Padding = new Thickness(0, 0, 0, 10)
                            }
                        }
                    }
                };

                dialog.OkButton.Content = LanguageManager.Instance.Ok;
                dialog.Buttons = new[] { dialog.OkButton };

                dialog.BackgroundContent = new Grid
                {
                    Background = new SolidColorBrush(Color.FromArgb(255, 244, 67, 54)),
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = 10
                };

                _audioPlayer.Play(SoundKind.Error);

                dialog.ShowDialog();
            });
        }

        public void ShowFFmpegUnavailable()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new ModernDialog
                {
                    Title = "FFmpeg를 이용할 수 없음",
                    Content = "FFmpeg를 찾을 수 없습니다.\n\nFFmpeg가 있는 폴더를 지정하거나, FFmpeg를 다운로드 받아주세요."
                };

                // Yes -> Select FFmpeg Folder
                dialog.YesButton.Content = LanguageManager.Instance.SelectFFmpegFolder;
                dialog.YesButton.Click += (S, E) => FFmpegService.SelectFFmpegFolder();

                // No -> Download FFmpeg
                dialog.NoButton.Content = "FFmpeg 다운로드";
                dialog.NoButton.Click += (S, E) => FFmpegService.FFmpegDownloader?.Invoke();

                dialog.CancelButton.Content = "취소";

                dialog.Buttons = new[] { dialog.YesButton, dialog.NoButton, dialog.CancelButton };

                _audioPlayer.Play(SoundKind.Error);

                dialog.ShowDialog();
            });
        }

        public void ShowException(Exception Exception, string Message, bool Blocking = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var win = new ExceptionWindow(Exception, Message);

                _audioPlayer.Play(SoundKind.Error);

                if (Blocking)
                {
                    win.ShowDialog();
                }
                else win.ShowAndFocus();
            });
        }

        public bool ShowYesNo(string Message, string Title)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new ModernDialog
                {
                    Title = Title,
                    Content = new ScrollViewer
                    {
                        Content = Message,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Padding = new Thickness(0, 0, 0, 10)
                    }
                };

                var result = false;

                dialog.YesButton.Content = LanguageManager.Instance.Yes;
                dialog.YesButton.Click += (S, E) => result = true;

                dialog.NoButton.Content = LanguageManager.Instance.No;

                dialog.Buttons = new[] { dialog.YesButton, dialog.NoButton };

                dialog.ShowDialog();

                return result;
            });
        }
    }
}
