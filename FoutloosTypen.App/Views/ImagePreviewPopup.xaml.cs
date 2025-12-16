using System;
using System.IO;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace FoutloosTypen.Views
{
    public partial class ImagePreviewPopup : Popup
    {
        private readonly string _imagePath;
        private readonly string _tweetText;

        public ImagePreviewPopup(string imagePath, string tweetText)
        {
            _imagePath = imagePath ?? throw new ArgumentNullException(nameof(imagePath));
            _tweetText = tweetText ?? string.Empty;

            InitializeComponent();

            // Wire up UI events
            CancelButton.Clicked += OnCancelClicked;
            ShareButton.Clicked += OnShareClicked;

            TweetLabel.Text = _tweetText;

            try
            {
                if (File.Exists(_imagePath))
                {
                    PreviewImage.Source = ImageSource.FromFile(_imagePath);
                }
            }
            catch
            {
                // ignore image load failures
            }
        }

        private void OnCancelClicked(object? sender, EventArgs e)
        {
            Close(false);
        }

        private void OnShareClicked(object? sender, EventArgs e)
        {
            Close(true);
        }
    }
}