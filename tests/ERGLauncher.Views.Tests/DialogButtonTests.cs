using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using ERGLauncher;
using ERGLauncher.Services;

namespace ERGLauncher.Views.Tests;

[NotInParallel]
public class DialogButtonTests
{
    [Test]
    public async Task ProgrammaticDialogButtonsUseSharedCenteredControlStyleForConfirmationAndMessageVariants()
    {
        var result = await AvaloniaTestSession.RunAsync(() =>
        {
            var confirmationButtons = AvaloniaDialogButtonFactory.CreateActionButtons(showCancel: true);
            var messageButtons = AvaloniaDialogButtonFactory.CreateActionButtons(showCancel: false);
            var window = new Window
            {
                Content = new StackPanel
                {
                    Children =
                    {
                        new StackPanel { Children = { confirmationButtons[0], confirmationButtons[1] } },
                        new StackPanel { Children = { messageButtons[0] } },
                    },
                },
            };

            window.Show();
            window.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var snapshot = (
                confirmation: confirmationButtons.Select(button =>
                    (Content: button.Content?.ToString(), button.IsCancel, button.IsDefault,
                        button.MinWidth, Classes: button.Classes.ToArray())).ToArray(),
                message: messageButtons.Select(button =>
                    (Content: button.Content?.ToString(), button.IsCancel, button.IsDefault,
                        button.MinWidth, Classes: button.Classes.ToArray())).ToArray(),
                confirmationAlignment: confirmationButtons.Select(button =>
                    (button.HorizontalContentAlignment, button.VerticalContentAlignment)).ToArray(),
                messageAlignment: messageButtons.Select(button =>
                    (button.HorizontalContentAlignment, button.VerticalContentAlignment)).ToArray());
            window.Close();
            return snapshot;
        });

        await Assert.That(result.confirmation).Count().IsEqualTo(2);
        await Assert.That(result.confirmation[0].Content).IsEqualTo(Properties.Resources.Cancel);
        await Assert.That(result.confirmation[0].IsCancel).IsTrue();
        await Assert.That(result.confirmation[0].IsDefault).IsFalse();
        await Assert.That(result.confirmation[0].MinWidth).IsEqualTo(90);
        await Assert.That(result.confirmation[0].Classes).Contains("dialog-control");
        await Assert.That(result.confirmation[1].Content).IsEqualTo(Properties.Resources.Ok);
        await Assert.That(result.confirmation[1].IsDefault).IsTrue();
        await Assert.That(result.confirmation[1].IsCancel).IsFalse();
        await Assert.That(result.confirmation[1].MinWidth).IsEqualTo(90);
        await Assert.That(result.confirmation[1].Classes).Contains("dialog-control");
        await Assert.That(result.confirmation[1].Classes).Contains("accent");
        await Assert.That(result.message).Count().IsEqualTo(1);
        await Assert.That(result.message[0].Content).IsEqualTo(Properties.Resources.Ok);
        await Assert.That(result.message[0].IsDefault).IsTrue();
        await Assert.That(result.message[0].Classes).Contains("dialog-control");
        await Assert.That(result.message[0].Classes).Contains("accent");
        await Assert.That(result.confirmationAlignment).IsEquivalentTo(new[]
        {
            (HorizontalAlignment.Center, VerticalAlignment.Center),
            (HorizontalAlignment.Center, VerticalAlignment.Center),
        });
        await Assert.That(result.messageAlignment).IsEquivalentTo(new[]
        {
            (HorizontalAlignment.Center, VerticalAlignment.Center),
        });
    }
}
