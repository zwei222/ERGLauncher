using ERGLauncher.Core.Models;

namespace ERGLauncher.Core.Tests.Models;

public sealed class HistoryCollectionTests
{
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
    }

    [Test]
    public async Task Remove_current_item_keeps_index_on_a_valid_neighbor()
    {
        var history = new HistoryCollection<string>("root");
        history.Push("brand");

        var removed = history.Remove("brand");

        await Assert.That(removed).IsTrue();
        await Assert.That(history.CurrentValue).IsEqualTo("root");
        await Assert.That(history.Index).IsEqualTo(0);
    }
}
