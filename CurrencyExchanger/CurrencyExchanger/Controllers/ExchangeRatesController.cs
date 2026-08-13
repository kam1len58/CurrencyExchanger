using CurrencyExchanger.Dao;
using CurrencyExchanger.Domain;
using CurrencyExchanger.Domain.Validators;
using CurrencyExchanger.Dto;
using CurrencyExchanger.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchanger.Controllers;

[ApiController]
[Route("exchangeRates")]
public class ExchangeRatesController : ControllerBase
{
    private readonly ExchangeRateValidator _exchangeRateValidator;
    private readonly DBConnectionProvider _connectionProvider;

    public ExchangeRatesController(DBConnectionProvider connectionProvider, ExchangeRateValidator exchangeRateValidator)
    {
        _connectionProvider = connectionProvider;
        _exchangeRateValidator = exchangeRateValidator;
    }

    [HttpGet]
    public ActionResult GetExchangesRates()
    {
        var currencyDao = new CurrencyDao(_connectionProvider);
        List<ExchangeRateDto> exchangesRates = new ExchangeRateDao(_connectionProvider, _exchangeRateValidator)
            .GetAllExchangeRates()
            .Select(er =>
            {
                var baseCurrency = currencyDao.GetCurrencyById(er.BaseCurrencyId);
                var targetCurrency = currencyDao.GetCurrencyById(er.TargetCurrencyId);

                if (baseCurrency is null || targetCurrency is null)
                    throw new Exception("Валюта не найдена");

                return ExchangeRateMapper.ToDto(er, baseCurrency, targetCurrency); 
            })
            .ToList();
            
        return Ok(exchangesRates);
    }

    [HttpPost]
    public ActionResult AddExchangeRate([FromForm] string baseCurrencyCode, [FromForm] string targetCurrencyCode, [FromForm] decimal rate)
    {
        var exchangeRateDao = new ExchangeRateDao(_connectionProvider, _exchangeRateValidator);
        var currencyDao = new CurrencyDao(_connectionProvider);
        var baseCurrencyId = currencyDao.GetCurrencyIdByCode(baseCurrencyCode);
        var targetCurrencyId = currencyDao.GetCurrencyIdByCode(targetCurrencyCode);

        if(baseCurrencyId==0 || targetCurrencyId==0)
            return NotFound(new { message = "Данной валюты не существует" });

        if(exchangeRateDao.IsExchangeRateExists(baseCurrencyId,targetCurrencyId))
            return Conflict(new { message = "Такой обменный курс уже существует" });

        _exchangeRateValidator.ValidateParameters(baseCurrencyCode, targetCurrencyCode, rate);
        var exchangeRate = exchangeRateDao.AddExchangeRate(baseCurrencyId, targetCurrencyId, rate);
        if (exchangeRate is null)
            return StatusCode(500, "Не получилось добавить обменный курс в базу данных");

        return Ok(exchangeRate);
    }
}
