using ERGLauncher.Core.Models;

namespace ERGLauncher.Core.Tests.Models;

public sealed class HistoryCollectionTests
{
    [Test]
    public async Task Push_enables_undo_and_exposes_previous_value()
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
    public async Task Back_and_forward_move_between_existing_entries_and_stop_at_boundaries()
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
    public async Task At_selects_valid_entry_and_ignores_out_of_range_indices()
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
    public async Task Push_after_back_discards_forward_history()
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
    public async Task Remove_adjusts_index_for_items_before_at_and_after_current_entry()
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
    public async Task Clear_resets_navigation_and_allows_reuse()
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

    [Test]
    public async Task Clone_keeps_an_independent_copy_at_the_same_position()
    {
        var history = new HistoryCollection<string>("root");
        history.Push("brand");
        history.Back();

        var clone = (HistoryCollection<string>)history.Clone();
        clone.Push("other");

        await Assert.That(history).IsEquivalentTo(["root", "brand"]);
        await Assert.That(history.CurrentValue).IsEqualTo("root");
        await Assert.That(clone).IsEquivalentTo(["root", "other"]);
        await Assert.That(clone.CurrentValue).IsEqualTo("other");
    }
}
