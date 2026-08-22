using CurrencyExchanger.BLL;
using CurrencyExchanger.BLL.Validators;
using CurrencyExchanger.DAL.Dao;
using CurrencyExchanger.DAL.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace CurrencyExchanger.API.Controllers;

[ApiController]
[Route("exchangeRate")]
public class ExchangeRateController(
    ExchangeRateValidator exchangeRateValidator,
    ExchangeRateDao exchangeRateDao,
    CurrencyDao currencyDao,
    ExchangeService exchangeService
) : ControllerBase
{
    private readonly ExchangeRateValidator _exchangeRateValidator = exchangeRateValidator;
    private readonly ExchangeRateDao _exchangeRateDao = exchangeRateDao;
    private readonly CurrencyDao _currencyDao = currencyDao;
    private readonly ExchangeService _exchangeService = exchangeService;


    [HttpGet("{pair?}")]
    public async Task<ActionResult> GetExchangeRate(string pair)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pair) || pair.Length != 6)
            {
                return BadRequest(new { message = "Коды валют пары отсутствуют в адресе" });
            }

            var exchangeRate = await _exchangeRateDao.GetExchangeRateByCodeAsync(pair);

            if (exchangeRate is null)
            {
                return NotFound(new { message = "Обменный курс для пары не найден" });
            }

            return Ok(exchangeRate);
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        }
    }

    [HttpPatch("{pair}")]
    public async Task<ActionResult> UpdateExchangeRate([FromRoute] string pair, [FromForm] decimal? rate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pair) || rate is null || pair.Length != 6)
            {
                return BadRequest(new { message = "Отсутствует нужное поле формы" });
            }

            var baseCurrencyCode = pair.Substring(0, 3);
            var targetCurrencyCode = pair.Substring(3, 3);
            var baseCurrencyId = await _currencyDao.GetCurrencyIdByCodeAsync(baseCurrencyCode);
            var targetCurrencyId = await _currencyDao.GetCurrencyIdByCodeAsync(targetCurrencyCode);

            if (!(await _exchangeRateDao.IsExchangeRateExistsAsync(baseCurrencyId, targetCurrencyId)))
            {
                return NotFound(new { message = "Валютная пара отсутствует в базе данных" });
            }

            _exchangeRateValidator.ValidateParameters(baseCurrencyCode, targetCurrencyCode, rate);
            var exchangeRate = await _exchangeRateDao.UpdateExchangeRateDtoAsync(pair, rate);

            return Ok(exchangeRate);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Отсутствует нужное поле формы" });
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        }
    }

    [HttpGet("exchange")]
    public async Task<ActionResult> Exchange([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal? amount)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to) || amount is null)
            {
                return BadRequest(new { message = "Отсутствует нужное поле формы" });
            }

            _exchangeRateValidator.ValidateParameters(from, to, amount);

            var baseCurrency = await _currencyDao.GetCurrencyByCodeAsync(from);
            var targetCurrency = await _currencyDao.GetCurrencyByCodeAsync(to);

            if (baseCurrency is null || targetCurrency is null)
            {
                return NotFound(new { message = "Валютная пара отсутствует в базе данных" });
            }

            var baseCurrencyId = baseCurrency.ID;
            var targetCurrencyId = targetCurrency.ID;
            var exchange = await _exchangeService.ExchangeAsync(baseCurrencyId, targetCurrencyId, amount);

            var rate = exchange.Rate;
            if (rate is null)
            {
                return NotFound(new { message = "Валютная пара отсутствует в базе данных" });
            }

            var convertedAmount = exchange.ConvertedAmount;
            var exchangeDto = ExchangeMapper.ToDto(baseCurrency, targetCurrency, rate, amount, convertedAmount);

            return Ok(exchangeDto);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Отсутствует нужное поле формы" });
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        }
    }
}