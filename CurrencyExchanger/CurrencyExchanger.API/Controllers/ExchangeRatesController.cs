using CurrencyExchanger.BLL.Validators;
using CurrencyExchanger.DAL.Dao;
using CurrencyExchanger.DAL.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace CurrencyExchanger.API.Controllers;

[ApiController]
[Route("exchangeRates")]
public class ExchangeRatesController(
    ExchangeRateValidator exchangeRateValidator,
    ExchangeRateDao exchangeRateDao,
    CurrencyDao currencyDao
) : ControllerBase
{
    private readonly ExchangeRateValidator _exchangeRateValidator = exchangeRateValidator;
    private readonly ExchangeRateDao _exchangeRateDao = exchangeRateDao;
    private readonly CurrencyDao _currencyDao = currencyDao;

    [HttpGet]
    public async Task<ActionResult> GetExchangesRates()
    {
        try
        {
            var exchangesRates = (await Task.WhenAll((await _exchangeRateDao
                        .GetAllExchangeRatesAsync())
                        .Select(async er =>
                        {
                            var baseCurrency = await _currencyDao.GetCurrencyByIdAsync(er.BaseCurrencyId);
                            var targetCurrency = await _currencyDao.GetCurrencyByIdAsync(er.TargetCurrencyId);

                            if (baseCurrency is null || targetCurrency is null)
                            {
                                throw new Exception("Валюта не найдена");
                            }

                            return ExchangeRateMapper.ToDto(er, baseCurrency, targetCurrency);
                        })))
                        .ToList();

            return Ok(exchangesRates);
        }
        catch (SqliteException)
        {
            return StatusCode(500, new { message = "База данных недоступна" });
        }
    }

    [HttpPost]
    public async Task<ActionResult> AddExchangeRate([FromForm] string baseCurrencyCode, [FromForm] string targetCurrencyCode, [FromForm] decimal? rate)
    {
        try
        {
            if (baseCurrencyCode is null || targetCurrencyCode is null || rate is null)
            {
                return BadRequest(new { message = "Отсутствует нужное поле формы" });
            }

            var baseCurrencyId = await _currencyDao.GetCurrencyIdByCodeAsync(baseCurrencyCode);
            var targetCurrencyId = await _currencyDao.GetCurrencyIdByCodeAsync(targetCurrencyCode);

            if (baseCurrencyId == 0 || targetCurrencyId == 0)
            {
                return NotFound(new { message = "Данной валюты не существует" });
            }

            if (await _exchangeRateDao.IsExchangeRateExistsAsync(baseCurrencyId, targetCurrencyId))
            {
                return Conflict(new { message = "Такой обменный курс уже существует" });
            }

            _exchangeRateValidator.ValidateParameters(baseCurrencyCode, targetCurrencyCode, rate);

            var exchangeRate = await _exchangeRateDao.AddExchangeRateAsync(baseCurrencyId, targetCurrencyId, rate);

            return CreatedAtAction(nameof(GetExchangesRates), exchangeRate);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Отсутствует нужное поле формы" });
        }
        catch (SqliteException)
        {
            return StatusCode(500, "Не получилось добавить обменный курс в базу данных");
        }
    }
}
