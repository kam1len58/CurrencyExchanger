using CurrencyExchanger.Dao;
using CurrencyExchanger.Domain;
using CurrencyExchanger.Domain.Validators;
using CurrencyExchanger.Dto;
using CurrencyExchanger.Mappers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Authentication;

namespace CurrencyExchanger.Controllers;

[ApiController]
[Route("exchangeRate")]
public class ExchangeRateController : ControllerBase
{
    private readonly ExchangeRateValidator _exchangeRateValidator;
    private readonly DBConnectionProvider _connectionProvider;

    public ExchangeRateController(DBConnectionProvider connectionProvider, ExchangeRateValidator exchangeRateValidator)
    {
        _connectionProvider = connectionProvider;
        _exchangeRateValidator = exchangeRateValidator;
    }

    [HttpGet("{code?}")]
    public ActionResult GetExchangeRate(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { message = "Коды валют пары отсутствуют в адресе" });

            var exchangeRateDao = new ExchangeRateDao(_connectionProvider, _exchangeRateValidator);
            var exchangeRate = exchangeRateDao.GetExchangeRateByCode(code);

            if (exchangeRate is null)
                return NotFound(new { message = "Обменный курс для пары не найден" });

            return Ok(exchangeRate);
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        }
    }

    [HttpPatch("{pair}")]
    public ActionResult UpdateExchangeRate([FromRoute] string pair, [FromForm] decimal? rate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pair) || rate is null)
                return BadRequest(new { message = "Отсутствует нужное поле формы" });

            if (pair.Length == 6)
            {
                var baseCurrencyCode = pair.Substring(0, 3);
                var targetCurrencyCode = pair.Substring(3, 3);
                var currencyDao = new CurrencyDao(_connectionProvider);
                var baseCurrencyId = currencyDao.GetCurrencyIdByCode(baseCurrencyCode);
                var targetCurrencyId = currencyDao.GetCurrencyIdByCode(targetCurrencyCode);
                var exchangeRateDao = new ExchangeRateDao(_connectionProvider, _exchangeRateValidator);
                if (!exchangeRateDao.IsExchangeRateExists(baseCurrencyId, targetCurrencyId))
                    return NotFound(new { message = "Валютная пара отсутствует в базе данных" });

                _exchangeRateValidator.ValidateParameters(baseCurrencyCode, targetCurrencyCode, rate);
                var exchangeRate = exchangeRateDao.UpdateExchangeRateDto(pair, rate);
                return Ok(exchangeRate);
            }

            return BadRequest(new { message = "Отсутствует нужное поле формы" });
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        }
    }

    [HttpGet("exchange")]
    public ActionResult Exchange([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal? amount)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to) || amount is null)
                return BadRequest(new { message = "Отсутствует нужное поле формы" });

            _exchangeRateValidator.ValidateParameters(from, to, amount);

            var currencyDao = new CurrencyDao(_connectionProvider);
            var exchangeDao = new ExchangeRateDao(_connectionProvider, _exchangeRateValidator);
            var baseCurrency = currencyDao.GetCurrencyByCode(from);
            var targetCurrency = currencyDao.GetCurrencyByCode(to);

            if (baseCurrency is null || targetCurrency is null)
                return NotFound(new { message = "Валютная пара отсутствует в базе данных" });

            var currencies = currencyDao.GetAllCurrencies();
            var exchangeRates = exchangeDao.GetAllExchangeRates();
            var exchangeService = new ExchangeService(currencies, exchangeRates);
            var baseCurrencyId = baseCurrency.ID;
            var targetCurrencyId = targetCurrency.ID;
            var exchange = exchangeService.Exchange(baseCurrencyId, targetCurrencyId, amount);
            var rate = exchange.Rate;
            if(rate is null)
                return NotFound(new { message = "Валютная пара отсутствует в базе данных" });
            var convertedAmount = exchange.ConvertedAmount;
            var exchangeDto = ExchangeMapper.ToDto(baseCurrency, targetCurrency, rate, amount, convertedAmount);
            return Ok(exchangeDto);
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        }
    }
}