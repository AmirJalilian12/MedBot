using AutoMapper;
using MedBot.Dtos;
using MedBot.Entities;
using MedBot.Repositories;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;

namespace MedBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly IBaseRepository<News> _newsRepository;
        private readonly IBaseRepository<KeyWord> _keyWordRepository;
        private readonly IBaseRepository<NewsKeyWord> _newKeyWordRepository;
        private readonly IMapper _mapper;
        private readonly ITelegramBotClient _bot;

        public NewsController(
            IBaseRepository<News> newsRepository,
            IBaseRepository<KeyWord> keyWordRepository,
            IBaseRepository<NewsKeyWord> newKeyWordRepository,
            IMapper mapper,
            ITelegramBotClient bot)
        {
            _newsRepository = newsRepository;
            _keyWordRepository = keyWordRepository;
            _newKeyWordRepository = newKeyWordRepository;
            _mapper = mapper;
            _bot = bot;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NewsDto>> Get(int id, CancellationToken cancellationToken)
        {
            var obj = await _newsRepository.GetByIdAsync(cancellationToken, id);
            if (obj == null) return NotFound();
            return Ok(_mapper.Map<NewsDto>(obj));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsDto>>> Get()
        {
            return Ok(_mapper.Map<List<NewsDto>>(_newsRepository.TableNoTracking));
        }

        [HttpPost]
        public async Task<ActionResult<NewsDto>> Post(NewsDto model, CancellationToken cancellationToken)
        {
            var news = _mapper.Map<News>(model);
            string text = $"<b>{model.Title}</b>\n\n";

            if (model.KeyWords != null && model.KeyWords.Any())
            {
                news.NewsKeyWords = new List<NewsKeyWord>();
                foreach (var keywordId in model.KeyWords)
                {
                    var keyword = await _keyWordRepository.GetByIdAsync(cancellationToken, keywordId);
                    if (keyword == null) continue;
                    text += $"#{keyword.Title} ";
                    news.NewsKeyWords.Add(new NewsKeyWord { KeyWordId = keywordId });
                }
            }

            var message = await _bot.SendMessage("@MedBotChan", text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            news.MessageId = message.MessageId;
            var obj = await _newsRepository.AddAsync(news, cancellationToken);
            return Ok(_mapper.Map<NewsDto>(obj));
        }

        [HttpPut]
        public async Task<ActionResult<NewsDto>> Put(NewsDto model, CancellationToken cancellationToken)
        {
            var obj = await _newsRepository.GetByIdAsync(cancellationToken, model.Id);
            if (obj == null) return NotFound();

            string text = $"<b>{model.Title}</b>\n{model.Desc}\n\n";
            _mapper.Map(model, obj);

            if (model.KeyWords != null && model.KeyWords.Any())
            {
                var currentKeywords = _newKeyWordRepository.Table.Where(x => x.NewsId == model.Id).ToList();
                foreach (var k in currentKeywords) await _newKeyWordRepository.DeleteAsync(k, cancellationToken);

                obj.NewsKeyWords = new List<NewsKeyWord>();
                foreach (var keywordId in model.KeyWords)
                {
                    var keyword = await _keyWordRepository.GetByIdAsync(cancellationToken, keywordId);
                    if (keyword == null) continue;
                    text += $"#{keyword.Title} ";
                    obj.NewsKeyWords.Add(new NewsKeyWord { KeyWordId = keywordId });
                }
            }

            await _bot.EditMessageText("@MedBotChan", obj.MessageId, text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            await _newsRepository.UpdateAsync(obj, cancellationToken);
            return Ok(model);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var obj = await _newsRepository.GetByIdAsync(cancellationToken, id);
            if (obj == null) return NotFound();

            var keywords = _newKeyWordRepository.Table.Where(x => x.NewsId == id).ToList();
            foreach (var k in keywords) await _newKeyWordRepository.DeleteAsync(k, cancellationToken);

            await _bot.DeleteMessage("@MedBotChan", obj.MessageId);
            await _newsRepository.DeleteAsync(obj, cancellationToken);
            return Ok();
        }
    }
}