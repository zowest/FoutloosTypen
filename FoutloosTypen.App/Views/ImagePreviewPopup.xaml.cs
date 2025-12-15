using CommunityToolkit.Maui.Views;

namespace FoutloosTypen.Views;

public partial class ImagePreviewPopup : Popup
{
    private readonly string _imagePath;
    private readonly string _tweetText;

    public ImagePreviewPopup(string imagePath, string tweetText)
    {
        InitializeComponent();
        _imagePath = imagePath;
        _tweetText = tweetText;

        PreviewImage.Source = ImageSource.FromFile(_imagePath);
        PreviewText.Text = _tweetText;
    }

    private void OnCancel(object? sender, EventArgs e)
    {
        Close(false);
    }

    private void OnConfirm(object? sender, EventArgs e)
    {
        Close(true);
    }
}
