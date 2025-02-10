using Discord.Interactions;

namespace OPZBackup.Modules;

public class MultiTableInteractionState<T>
{
    public string selectBoxOption;
    public readonly T data;
    public SocketInteractionContext Interaction { get; private set; }

    public Dictionary<string, int> currentTablePage;

    public MultiTableInteractionState(SocketInteractionContext interaction, T data, Dictionary<string, int> currentTablePage)
    {
        this.data = data;
        this.currentTablePage = currentTablePage;
        Interaction = interaction;
        selectBoxOption = currentTablePage.First().Key;
    }
}