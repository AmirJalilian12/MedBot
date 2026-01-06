using MedBot.Entities;
using MedBot.Repositories;
using MedBot.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MedBot.Controllers
{
    [ApiController]
    [Route("bot")]
    public class BotController : ControllerBase
    {
        private readonly ITelegramBotClient _bot;

        public BotController(ITelegramBotClient bot)
        {
            _bot = bot;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> SetWebhook()
        {
            var allowedUpdates = new[]
            {
        UpdateType.Message,
        UpdateType.CallbackQuery
    };

            await _bot.SetWebhook(
                url: "https://medcoding.ir/bot/update",
                allowedUpdates: allowedUpdates,
                dropPendingUpdates: true
            );

            return Ok("Webhook set successfully ✅");
        }

        [HttpPost("update")]
        [AllowAnonymous]
        public async Task<IActionResult> ReceiveUpdate([FromBody] Update update,CancellationToken cancellationToken)
        {
            if (update == null)
                return Ok();

            if (update.Type == UpdateType.Message && update.Message != null)
            {
                await HandleMessage(update.Message, cancellationToken);
            }

            return Ok();
        }
        private async Task HandleMessage(Message message, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(message.Text))
                return;

            if (message.Text.StartsWith(DefaultContents.Start))
            {
                await _bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "this is a test for check",
                    replyMarkup: GenerateMainKeyboard(),
                    cancellationToken: ct
                );
            }
            else if (message.Text.StartsWith(""))
            {

            }
        }
        // --- متدهای تولید کیبورد ---
        private InlineKeyboardMarkup GenerateConfirmationInlineKeyboard(string text)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData(DefaultContents.Confirmed, $"Confirmed_{text}") },
                new[] { InlineKeyboardButton.WithCallbackData(DefaultContents.Canceled, "Canceled") }
            });
        }

        private ReplyKeyboardMarkup GenerateProfileKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton(DefaultContents.EditFirstName) },
                new[] { new KeyboardButton(DefaultContents.EditLastName) },
                new[] { new KeyboardButton(DefaultContents.BackToMainMenu) }
            })
            { ResizeKeyboard = true };
        }

        private ReplyKeyboardMarkup GenerateMainKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton(DefaultContents.Search) },
                new[] { new KeyboardButton(DefaultContents.HeadOfNews), new KeyboardButton(DefaultContents.Money) },
                new[] { new KeyboardButton(DefaultContents.Location), new KeyboardButton(DefaultContents.ContactUs) },
                new[] { new KeyboardButton(DefaultContents.Profile) }
            })
            { ResizeKeyboard = true };
        }
    }
}
