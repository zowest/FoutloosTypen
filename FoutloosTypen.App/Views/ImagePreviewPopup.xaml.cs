using System;
using System.IO;
using System.ComponentModel;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using FoutloosTypen.ViewModels;

namespace FoutloosTypen.Views
{
    public partial class ImagePreviewPopup : Popup
    {
        private readonly SharePreviewViewModel _viewModel;

        public ImagePreviewPopup(SharePreviewViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();

            BindingContext = _viewModel;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            CancelButton.Clicked += OnCancelClicked;
            ShareButton.Clicked += OnShareClicked;

            if (_viewModel.ImageBytes != null && _viewModel.ImageBytes.Length > 0)
            {
                PreviewImage.Source = ImageSource.FromStream(() => new MemoryStream(_viewModel.ImageBytes));
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SharePreviewViewModel.IsSharing))
            {
                CancelButton.IsEnabled = !_viewModel.IsSharing;
                ShareButton.IsEnabled = !_viewModel.IsSharing;
            }
        }

        private void OnCancelClicked(object? sender, EventArgs e)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            Close(false);
        }
        
        private async void OnShareClicked(object? sender, EventArgs e)
        {
            await _viewModel.ShareCommand.ExecuteAsync(null);
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            Close(true);
        }
    }
}