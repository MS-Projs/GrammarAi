using GrammarAi.Domain.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = GrammarAi.Domain.Entities.User;

namespace GrammarAi.Infrastructure.Bot;

public class BotHandlerContext(
    Update update,
    User user,
    BotSession session,
    ITelegramBotClient bot,
    CancellationToken ct)
{
    public Update Update { get; } = update;
    public User User { get; } = user;
    public BotSession Session { get; } = session;
    public ITelegramBotClient Bot { get; } = bot;
    public CancellationToken Ct { get; } = ct;

    public long ChatId => Session.ChatId;
    public string? MessageText => Update.Message?.Text;
    public string? CallbackData => Update.CallbackQuery?.Data;
    public Document? Document => Update.Message?.Document;
    public PhotoSize[]? Photos => Update.Message?.Photo;
}
