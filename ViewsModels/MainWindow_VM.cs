using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.IO;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Windows;

namespace UrlVideoConverter.ViewsModels
{
    public class MainWindow_VM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand SubmitCommand { get; set; }
        public List<string> OptionsConverter { get;}

        public MainWindow_VM()
        {
            OptionsConverter = new List<string> { "MP4", "MP3" };
            SubmitCommand = new RelayCommand(SubmitAsync);
        }

        private string _selectedOption;
        public string SelectedOption
        {
            get { return _selectedOption; }
            set
            {
                if (OptionsConverter.Contains(value))
                {
                    _selectedOption = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _inputText;
        public string InputText
        {
            get { return _inputText; }
            set
            {
                if (Regex.IsMatch(value, @"^(http(s)?:\/\/)?((w){3}.)?youtu(be|.be)?(\.com)?\/.+"))
                {
                    _inputText = value;
                    OnPropertyChanged();
                }
            }
        }

        private async void SubmitAsync()
        {
            if (_inputText != null && _selectedOption != null)
            {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(_inputText);
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(_inputText);

                string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\";
                string? outputPath = null;

                IStreamInfo? streamInfo = null;

                try
                {
                    if (OptionsConverter[0] == _selectedOption)
                    {
                        streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
                        outputPath = Path.Combine(downloadsFolder, $"{CleanFileName(video.Title) + video.Id}.mp4");
                    }
                    else if (OptionsConverter[1] == _selectedOption)
                    {
                        streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                        outputPath = Path.Combine(downloadsFolder, $"{CleanFileName(video.Title) + video.Id}.mp3");
                    }

                    if (streamInfo != null)
                    {
                        MessageBox.Show("Download started", "Download Status", MessageBoxButton.OK, MessageBoxImage.Information);
                        await youtube.Videos.Streams.DownloadAsync(streamInfo, outputPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error has occurred : {ex.Message}");
                }
            }
        }

        string CleanFileName(string fileName)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            return new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        }
    }
}