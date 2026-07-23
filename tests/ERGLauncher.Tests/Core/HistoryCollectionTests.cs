using ERGLauncher.Core.Models;

namespace ERGLauncher.Tests.Core;

public sealed class HistoryCollectionTests
{
    [Test]
    public async Task PushEnablesUndoAndExposesPreviousValue()
    {
        var history = new HistoryCollection<string>("root");

        history.Push("brand");

        await Assert.That(history.Count).IsEqualTo(2);
        await Assert.That(history.Index).IsEqualTo(1);
        await Assert.That(history.CurrentValue).IsEqualTo("brand");
        await Assert.That(history.Peek()).IsEqualTo("root");
        await Assert.That(history.IsEnabledUndo).IsTrue();
        await Assert.That(history.IsEnabledRedo).IsFalse();
    }

    [Test]
    public async Task BackAndForwardStopAtBoundariesAndUpdateNavigationState()
    {
        var history = new HistoryCollection<string>("root");
        history.Push("brand");

        await Assert.That(history.Back()).IsEqualTo("root");
        await Assert.That(history.Back()).IsNull();
        await Assert.That(history.IsEnabledUndo).IsFalse();
        await Assert.That(history.IsEnabledRedo).IsTrue();
        await Assert.That(history.Forward()).IsEqualTo("brand");
        await Assert.That(history.Forward()).IsNull();
        await Assert.That(history.IsEnabledUndo).IsTrue();
        await Assert.That(history.IsEnabledRedo).IsFalse();
    }

    [Test]
    public async Task AtSelectsValidEntryAndDoesNotMoveForInvalidIndex()
    {
        var history = new HistoryCollection<string>("root");
        history.Push("brand");
        history.Push("product");

        await Assert.That(history.At(0)).IsEqualTo("root");
        await Assert.That(history.Index).IsEqualTo(0);
        await Assert.That(history.At(-1)).IsNull();
        await Assert.That(history.At(3)).IsNull();
        await Assert.That(history.Index).IsEqualTo(0);
        await Assert.That(history[2]).IsEqualTo("product");
    }

    [Test]
    public async Task PushAfterBackDiscardsRedoHistory()
    {
        var history = new HistoryCollection<string>("root");
        history.Push("brand-a");
        history.Push("product");
        history.Back();

        history.Push("brand-b");

        await Assert.That(history.Count).IsEqualTo(3);
        await Assert.That(history.CurrentValue).IsEqualTo("brand-b");
        await Assert.That(history.IsEnabledRedo).IsFalse();
        await Assert.That(history).IsEquivalentTo(["root", "brand-a", "brand-b"]);
    }

    [Test]
    public async Task RemoveAdjustsIndexBeforeAtAndAfterCurrentEntry()
    {
        var history = new HistoryCollection<string>("root");
        history.Push("brand");
        history.Push("product");
        history.At(1);

        await Assert.That(history.Remove("missing")).IsFalse();
        await Assert.That(history.Remove("root")).IsTrue();
        await Assert.That(history.Index).IsEqualTo(0);
        await Assert.That(history.CurrentValue).IsEqualTo("brand");
        await Assert.That(history.Remove("product")).IsTrue();
        await Assert.That(history.Index).IsEqualTo(0);
        await Assert.That(history.Remove("brand")).IsTrue();
        await Assert.That(history.Count).IsEqualTo(0);
        await Assert.That(history.Index).IsEqualTo(-1);
    }

    [Test]
    public async Task ClearResetsNavigationAndAllowsReuse()
    {
        var history = new HistoryCollection<string>("root");
        history.Push("brand");

        history.Clear();

        await Assert.That(history.Count).IsEqualTo(0);
        await Assert.That(history.Index).IsEqualTo(-1);
        await Assert.That(history.Peek()).IsNull();
        await Assert.That(history.IsEnabledUndo).IsFalse();
        await Assert.That(history.IsEnabledRedo).IsFalse();
        history.Push("new-root");
        await Assert.That(history.CurrentValue).IsEqualTo("new-root");
    }
}
