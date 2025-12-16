using System;
using System.IO;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace FoutloosTypen.Views
{
    public partial class ImagePreviewPopup : Popup
    {
        private readonly string? _imagePath;
        private readonly string _text;

        public ImagePreviewPopup(string imagePath, string text)
        {
            _imagePath = imagePath;
            _text = text ?? string.Empty;
            InitializeComponent();

            CancelButton.Clicked += OnCancelClicked;
            ShareButton.Clicked += OnShareClicked;

            TweetLabel.Text = _text;

            try
            {
                if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
                {
                    PreviewImage.Source = ImageSource.FromFile(_imagePath);
                }
            }
            catch
            {
                // fallback: no image
            }
        }

        private void OnCancelClicked(object? sender, EventArgs e) => Close(false);
        private void OnShareClicked(object? sender, EventArgs e) => Close(true);
    }
}