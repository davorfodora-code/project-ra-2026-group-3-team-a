using System.Windows;
using System.Windows.Controls;
using CommunityHub.Application.Dto.Citizen;
using CommunityHub.Ui.ViewModels.Citizen;

namespace CommunityHub.Ui.Views.Citizen;

public partial class CitizenCoordinatorReviewsWindow : Page
{
    private readonly long _userId;
    private readonly CitizenCoordinatorReviewsViewModel _viewModel;

    public CitizenCoordinatorReviewsWindow(long userId)
    {
        InitializeComponent();
        _userId = userId;
        _viewModel = new CitizenCoordinatorReviewsViewModel(userId);

        RatingComboBox.SelectedIndex = 4;
        LoadReviews();
    }

    private void LoadReviews()
    {
        List<CoordinatorReviewDto> reviews = _viewModel.LoadReviews();
        ReviewsDataGrid.ItemsSource = reviews;
        EmptyReviewsTextBlock.Visibility = reviews.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        ReviewsDataGrid.Visibility = reviews.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private int? GetSelectedRating()
    {
        if (RatingComboBox.SelectedItem is ComboBoxItem item &&
            item.Tag is string tag &&
            int.TryParse(tag, out int rating))
        {
            return rating;
        }

        return null;
    }

    private void CreateReviewButton_Click(object sender, RoutedEventArgs e)
    {
        int? rating = GetSelectedRating();
        if (!rating.HasValue)
        {
            MessageBox.Show("Izaberite ocenu.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        CoordinatorReviewActionResultDto result = _viewModel.CreateReview(
            rating.Value,
            JustificationTextBox.Text);

        MessageBox.Show(
            result.Message,
            result.Success ? "Uspeh" : "Greška",
            MessageBoxButton.OK,
            result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);

        if (result.Success)
        {
            JustificationTextBox.Text = string.Empty;
            RatingComboBox.SelectedIndex = 4;
            LoadReviews();
        }
    }

    private void ReportReviewButton_Click(object sender, RoutedEventArgs e)
    {
        if (ReviewsDataGrid.SelectedItem is not CoordinatorReviewDto selected)
        {
            MessageBox.Show("Izaberite recenziju koju želite da prijavite.", "Upozorenje",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        CoordinatorReviewActionResultDto result = _viewModel.ReportReview(selected.Id);

        MessageBox.Show(
            result.Message,
            result.Success ? "Uspeh" : "Greška",
            MessageBoxButton.OK,
            result.Success ? MessageBoxImage.Information : MessageBoxImage.Error);

        if (result.Success)
        {
            LoadReviews();
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadReviews();
    }
}