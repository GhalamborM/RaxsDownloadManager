using System.Threading.Tasks;
using System.Windows;
using RD.Controls;
using MessageBoxButtons = RD.Controls.MessageBoxButtons;

namespace RD.Examples;

/// <summary>
/// Demonstrates usage of the CustomMessageBox with Mica Fluent UI design
/// </summary>
public static class CustomMessageBoxExample
{
    /// <summary>
    /// Shows various examples of CustomMessageBox usage
    /// </summary>
    public static void ShowExamples()
    {
        // Basic information message
        CustomMessageBox.Show("This is a simple information message.");

        
        // Information message with custom title
        CustomMessageBox.Show("Operation completed successfully!", "Success");
        

        // Warning message
        CustomMessageBox.Show(
            "The file you are trying to open might be corrupted. Do you want to continue?",
            "Warning",
            MessageBoxType.Warning,
            MessageBoxButtons.YesNo);
        

        // Error message
        CustomMessageBox.Show(
            "An unexpected error occurred while processing your request. Please try again later.",
            "Error",
            MessageBoxType.Error);

        // Success message
        CustomMessageBox.Show(
            "Your download has been completed successfully!",
            "Download Complete",
            MessageBoxType.Success);
        

        // Question with multiple options
        var result = CustomMessageBox.Show(
            "Do you want to save your changes before closing?",
            "Unsaved Changes",
            MessageBoxType.Question,
            MessageBoxButtons.YesNoCancel);

        // Handle the result
        switch (result)
        {
            case MessageBoxResult.Yes:
                // Save and close
                break;
            case MessageBoxResult.No:
                // Close without saving
                break;
            case MessageBoxResult.Cancel:
                // Cancel the operation
                break;
        }
        

        // Custom buttons example - using valid MessageBoxResult values
        var customResult = CustomMessageBox.ShowCustom(
            "Choose how you want to proceed with the download:",
            "Download Options",
            MessageBoxType.Question,
            ("Resume", MessageBoxResult.Yes, true, false),
            ("Restart", MessageBoxResult.No, false, false),
            ("Cancel", MessageBoxResult.Cancel, false, true));

        // Handle custom result
        switch (customResult)
        {
            case MessageBoxResult.Yes: // Resume
                // Resume download logic
                break;
            case MessageBoxResult.No: // Restart
                // Restart download logic
                break;
            case MessageBoxResult.Cancel:
                // Cancel operation
                break;
        }
    }

    /// <summary>
    /// Example of showing a message box with a specific owner window
    /// </summary>
    /// <param name="owner">The owner window</param>
    public static void ShowWithOwner(Window owner)
    {
        CustomMessageBox.Show(
            "This message box is owned by a specific window.",
            "Owned Message Box",
            MessageBoxType.Information,
            MessageBoxButtons.OK,
            owner);
    }

    /// <summary>
    /// Example for download manager specific scenarios
    /// </summary>
    public static class DownloadManagerExamples
    {
        public static bool ConfirmDeleteDownload()
        {
            var result = CustomMessageBox.Show(
                "Are you sure you want to delete this download? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxType.Warning,
                MessageBoxButtons.YesNo);

            return result == MessageBoxResult.Yes;
        }

        public static MessageBoxResult HandleDownloadError()
        {
            return CustomMessageBox.ShowCustom(
                "The download failed due to a network error. What would you like to do?",
                "Download Failed",
                MessageBoxType.Error,
                ("Retry", MessageBoxResult.Yes, true, false),
                ("Skip", MessageBoxResult.No, false, false),
                ("Cancel All", MessageBoxResult.Cancel, false, true));
        }

        public static void ShowDownloadComplete(string fileName)
        {
            CustomMessageBox.Show(
                $"'{fileName}' has been downloaded successfully!",
                "Download Complete",
                MessageBoxType.Success);
        }

        public static bool ConfirmOverwrite(string fileName)
        {
            var result = CustomMessageBox.Show(
                $"The file '{fileName}' already exists. Do you want to overwrite it?",
                "File Exists",
                MessageBoxType.Question,
                MessageBoxButtons.YesNo);

            return result == MessageBoxResult.Yes;
        }

        public static MessageBoxResult ShowQuotaExceeded()
        {
            return CustomMessageBox.ShowCustom(
                "You have exceeded your download quota for today. You can:",
                "Quota Exceeded",
                MessageBoxType.Warning,
                ("Upgrade Plan", MessageBoxResult.Yes, false, false),
                ("Continue Tomorrow", MessageBoxResult.No, true, false),
                ("Cancel", MessageBoxResult.Cancel, false, true));
        }
    }
}